#if _WIN32
    #include <windows.h>
#endif

#include <osg/io_utils>
#include <osg/ValueObject>
#include <osgUtil/CullVisitor>
#include "Export.h"
#include "UpdateDatabasePager.h"

typedef std::vector< std::pair<int, int> > PagedIdAndChildList;
static std::map<int, PagedIdAndChildList> g_addedPagingNodes, g_removedPagingNodes;
static std::vector<int> g_addedDataList, g_removedDataList;

extern std::map<int, osg::observer_ptr<osg::Node> > g_readNodeMap;
extern std::map<osg::Node*, int> g_nodeRootIdMap;
extern osg::ref_ptr<osg::Group> g_nodeRoot;
extern unsigned int g_newNodeCounter;

enum UserLogLevel { LV_DEBUG = 0, LV_WARNING, LV_FATAL };
extern void printUserLog(int level, const char* format, ...);

class NotifyHandler : public osg::NotifyHandler
{
public:
    virtual void notify(osg::NotifySeverity severity, const char* message)
    {
        switch (severity)
        {
        case osg::INFO: case osg::NOTICE: printUserLog(LV_DEBUG, message); break;
        case osg::WARN: printUserLog(LV_WARNING, message); break;
        case osg::FATAL: printUserLog(LV_FATAL, message); break;
        }
    }
};

// Read functions
class PagedLODReplacer : public osg::NodeVisitor
{
public:
    PagedLODReplacer() : osg::NodeVisitor(osg::NodeVisitor::TRAVERSE_ALL_CHILDREN) {}
    virtual void apply(osg::Node& node) { traverse(node); }
    
    virtual void apply(osg::PagedLOD& node)
    {
        ExtendedPagedLOD* plodEx = dynamic_cast<ExtendedPagedLOD*>(&node);
        if (!plodEx) _pagedLODs.push_back(&node);
        traverse(node);
    }
    
    osg::ref_ptr<osg::Node> replacePagedLODs(osg::Node* root)
    {
        osg::ref_ptr<osg::Node> rootEx;
        for (unsigned int i = 0; i < _pagedLODs.size(); ++i)
        {
            osg::PagedLOD* plod = _pagedLODs[i].get();
            osg::Node::ParentList parents = plod->getParents();
            
            osg::ref_ptr<ExtendedPagedLOD> plodEx = new ExtendedPagedLOD(*plod);
            if (plod == root)
                rootEx = plodEx;
            else
            {
                for (unsigned int j = 0; j < parents.size(); ++j)
                    parents[j]->replaceChild(plod, plodEx.get());
            }

            // Provide a unique ID for each paged node, so we can query them in Unity
            int id = g_newNodeCounter++;
            g_readNodeMap[id] = plodEx.get();
            plodEx->setUserValue("GlobalID", id);
        }
        _pagedLODs.clear();
        return rootEx;
    }
    
protected:
    std::vector< osg::observer_ptr<osg::PagedLOD> > _pagedLODs;
};

void ExtendedPagedLOD::traverse(osg::NodeVisitor& nv)
{
    if (nv.getFrameStamp() && nv.getVisitorType() == osg::NodeVisitor::CULL_VISITOR)
        setFrameNumberOfLastTraversal(nv.getFrameStamp()->getFrameNumber());

    double timeStamp = nv.getFrameStamp() ? nv.getFrameStamp()->getReferenceTime() : 0.0;
    unsigned int frameNumber = nv.getFrameStamp() ? nv.getFrameStamp()->getFrameNumber() : 0;
    bool updateTimeStamp = nv.getVisitorType() == osg::NodeVisitor::CULL_VISITOR;
    switch (nv.getTraversalMode())
    {
    case osg::NodeVisitor::TRAVERSE_ALL_CHILDREN:
        std::for_each(_children.begin(), _children.end(), osg::NodeAcceptOp(nv));
        break;
    case osg::NodeVisitor::TRAVERSE_ACTIVE_CHILDREN:
    {
        float required_range = 0.0f;
        SceneViewVisitor* sv = dynamic_cast<SceneViewVisitor*>(&nv);
        if (sv)
        {
            if (_rangeMode == DISTANCE_FROM_EYE_POINT)
                required_range = sv->getDistanceToViewPoint(getCenter(), true);
            else
                required_range = sv->clampedPixelSize(getBound()) / sv->getLODScale();
        }
        
        int lastChildTraversed = -1; bool needToLoadChild = false;
        lastTraversedChildID.clear();
        for (unsigned int i = 0; i < _rangeList.size(); ++i)
        {
            if (_rangeList[i].first <= required_range && required_range < _rangeList[i].second)
            {
                if (i < _children.size())
                {
                    if (updateTimeStamp)
                    {
                        _perRangeDataList[i]._timeStamp = timeStamp;
                        _perRangeDataList[i]._frameNumber = frameNumber;
                    }
                    _children[i]->accept(nv);
                    lastTraversedChildID.push_back((int)i);
                    lastChildTraversed = (int)i;
                }
                else needToLoadChild = true;
                
                //printUserLog(LV_WARNING, "%s (PLOD %d): %lg < %lg < %lg", _perRangeDataList[i]._filename.c_str(),
                //             i, _rangeList[i].first, required_range, _rangeList[i].second);
            }
        }
        
        if (needToLoadChild)
        {
            unsigned int numChildren = _children.size();
            if (numChildren > 0 && ((int)numChildren - 1) != lastChildTraversed)
            {
                // select the last valid child.
                if (updateTimeStamp)
                {
                    _perRangeDataList[numChildren-1]._timeStamp = timeStamp;
                    _perRangeDataList[numChildren-1]._frameNumber = frameNumber;
                }
                _children[numChildren-1]->accept(nv);
                lastTraversedChildID.push_back((int)numChildren - 1);
            }
            
            // now request the loading of the next unloaded child.
            if (!_disableExternalChildrenPaging && nv.getDatabaseRequestHandler() && numChildren<_perRangeDataList.size())
            {
                // compute priority from where abouts in the required range the distance falls.
                float priority = (_rangeList[numChildren].second-required_range)
                               / (_rangeList[numChildren].second-_rangeList[numChildren].first);
                if (_rangeMode == PIXEL_SIZE_ON_SCREEN) priority = -priority;

                // modify the priority according to the child's priority offset and scale.
                priority = _perRangeDataList[numChildren]._priorityOffset
                         + priority * _perRangeDataList[numChildren]._priorityScale;
                if (_databasePath.empty())
                {
                    nv.getDatabaseRequestHandler()->requestNodeFile(
                        _perRangeDataList[numChildren]._filename, nv.getNodePath(), priority, nv.getFrameStamp(), 
                        _perRangeDataList[numChildren]._databaseRequest, _databaseOptions.get());
                }
                else  // prepend the databasePath to the child's filename.
                {
                    nv.getDatabaseRequestHandler()->requestNodeFile(
                        _databasePath + _perRangeDataList[numChildren]._filename, nv.getNodePath(), priority,
                        nv.getFrameStamp(), _perRangeDataList[numChildren]._databaseRequest, _databaseOptions.get());
                }
            }
        }
        break;
    }
    default: break;
    }
}

bool ExtendedPagedLOD::removeExpiredChildren(double expiryTime, unsigned int expiryFrame, osg::NodeList& removedChildren)
{
    if (_children.size() > _numChildrenThatCannotBeExpired)
    {
        unsigned cindex = _children.size() - 1;
        if (!_perRangeDataList[cindex]._filename.empty() &&
            _perRangeDataList[cindex]._timeStamp + _perRangeDataList[cindex]._minExpiryTime < expiryTime &&
            _perRangeDataList[cindex]._frameNumber + _perRangeDataList[cindex]._minExpiryFrames < expiryFrame)
        {
            int globalID = 0;
            if (getUserValue("GlobalID", globalID))
            {
                osg::NodePathList nodePaths = getParentalNodePaths(g_nodeRoot.get());
                if (!nodePaths.empty() && nodePaths[0].size() > 1)
                {
                    int rootID = g_nodeRootIdMap[nodePaths[0][1]];
                    PagedIdAndChildList& removedNodes = g_removedPagingNodes[rootID];
                    removedNodes.push_back(std::pair<int, int>(globalID, cindex));
                }
            }

            osg::Node* nodeToRemove = _children[cindex].get();
            removedChildren.push_back(nodeToRemove);
            return Group::removeChildren(cindex, 1);
        }
    }
    return false;
}

void DatabasePagerBridge::updateSceneGraph(const osg::FrameStamp& frameStamp)
{
    removeExpiredSubgraphsFromBridge(frameStamp);
    addLoadedDataToBridge(frameStamp);
}

void DatabasePagerBridge::removeExpiredSubgraphsFromBridge(const osg::FrameStamp& frameStamp)
{
    if (frameStamp.getFrameNumber() == 0) return; // No need to remove anything on first frame.

    // numPagedLODs >= actual number of PagedLODs. There can be
    // invalid observer pointers in _activePagedLODList.
    unsigned int numPagedLODs = _activePagedLODList->size();
    if (numPagedLODs <= _targetMaximumNumberOfPageLOD) return;

    int numToPrune = numPagedLODs - _targetMaximumNumberOfPageLOD;
    double expiryTime = frameStamp.getReferenceTime() - 0.1;
    unsigned int expiryFrame = frameStamp.getFrameNumber() - 1;

    // First traverse inactive PagedLODs, as their children will
    // certainly have expired. Then traverse active nodes if we still
    // need to prune.
    ObjectList childrenRemoved;
    if (numToPrune > 0)
        _activePagedLODList->removeExpiredChildren(
            numToPrune, expiryTime, expiryFrame, childrenRemoved, false);

    numToPrune = _activePagedLODList->size() - _targetMaximumNumberOfPageLOD;
    if (numToPrune > 0)
        _activePagedLODList->removeExpiredChildren(
            numToPrune, expiryTime, expiryFrame, childrenRemoved, true);

    if (!childrenRemoved.empty())
    {
        // pass the objects across to the database pager delete list
        if (_deleteRemovedSubgraphsInDatabaseThread)
        {
            OpenThreads::ScopedLock<OpenThreads::Mutex> lock(_fileRequestQueue->_requestMutex);
            // splice transfers the entire list in constant time.
            _fileRequestQueue->_childrenToDeleteList.splice(
                _fileRequestQueue->_childrenToDeleteList.end(), childrenRemoved);
            _fileRequestQueue->updateBlock();
        }
        else
            childrenRemoved.clear();
    }
}

void DatabasePagerBridge::addLoadedDataToBridge(const osg::FrameStamp& frameStamp)
{
    RequestQueue::RequestList localFileLoadedList;
    _dataToMergeList->swap(localFileLoadedList);

    double timeStamp = frameStamp.getReferenceTime();
    unsigned int frameNumber = frameStamp.getFrameNumber();
    for (RequestQueue::RequestList::iterator itr = localFileLoadedList.begin();
        itr != localFileLoadedList.end(); ++itr)
    {
        DatabaseRequest* databaseRequest = itr->get();
        osg::ref_ptr<osg::Group> group;
        if (!databaseRequest->_groupExpired && databaseRequest->_group.lock(group))
        {
            if (osgDB::Registry::instance()->getSharedStateManager())
                osgDB::Registry::instance()->getSharedStateManager()->share(databaseRequest->_loadedModel.get());

            osg::PagedLOD* plod = dynamic_cast<osg::PagedLOD*>(group.get());
            if (plod)
            {
                plod->setTimeStamp(plod->getNumChildren(), timeStamp);
                plod->setFrameNumber(plod->getNumChildren(), frameNumber);
                plod->getDatabaseRequest(plod->getNumChildren()) = 0;
            }
            else
            {
                osg::ProxyNode* proxyNode = dynamic_cast<osg::ProxyNode*>(group.get());
                if (proxyNode) proxyNode->getDatabaseRequest(proxyNode->getNumChildren()) = 0;
            }

            int globalID = 0;
            if (group->getUserValue("GlobalID", globalID))
            {
                PagedLODReplacer replacer;
                databaseRequest->_loadedModel->accept(replacer);

                osg::ref_ptr<osg::Node> newRoot = replacer.replacePagedLODs(databaseRequest->_loadedModel.get());
                if (newRoot.valid())
                {
                    int id = g_newNodeCounter++;
                    g_readNodeMap[id] = newRoot;
                    newRoot->setUserValue("GlobalID", id);
                    databaseRequest->_loadedModel = newRoot.get();
                }

                osg::NodePathList nodePaths = group->getParentalNodePaths(g_nodeRoot.get());
                if (!nodePaths.empty() && nodePaths[0].size() > 1)
                {
                    int rootID = g_nodeRootIdMap[nodePaths[0][1]];
                    PagedIdAndChildList& addedNodes = g_addedPagingNodes[rootID];
                    addedNodes.push_back(std::pair<int, int>(globalID, group->getNumChildren()));
                }
            }
            else
            {
                printUserLog(LV_WARNING, "No global ID for parent (%s) while adding new node %s",
                    group->getName().c_str(), databaseRequest->_fileName.c_str());
            }

            databaseRequest->_loadedModel->setName(osgDB::getSimpleFileName(databaseRequest->_fileName));
            group->addChild(databaseRequest->_loadedModel.get());
            //printUserLog(LV_WARNING, "New node (%s) added to %d", databaseRequest->_fileName.c_str(), globalID);

            // Check if parent plod was already registered if not start visitor from parent
            if (plod && !_activePagedLODList->containsPagedLOD(plod)) registerPagedLODs(plod, frameNumber);
            else registerPagedLODs(databaseRequest->_loadedModel.get(), frameNumber);

            // Insert loaded model into Registry ObjectCache
            if (databaseRequest->_objectCache.valid() && osgDB::Registry::instance()->getObjectCache())
                osgDB::Registry::instance()->getObjectCache()->addObjectCache(databaseRequest->_objectCache.get());

            double timeToMerge = timeStamp - databaseRequest->_timestampFirstRequest;
            if (timeToMerge < _minimumTimeToMergeTile) _minimumTimeToMergeTile = timeToMerge;
            if (timeToMerge > _maximumTimeToMergeTile) _maximumTimeToMergeTile = timeToMerge;
            _totalTimeToMergeTiles += timeToMerge;
            ++_numTilesMerges;
        }
        databaseRequest->_loadedModel = 0;
    }
}

int UNITY_INTERFACE_API requestNodeFile(const char* file)
{
    osg::ref_ptr<osg::Node> node = osgDB::readNodeFile(file);
    if (!node) return 0;
    
    PagedLODReplacer replacer;
    node->accept(replacer);
    osg::ref_ptr<osg::Node> newRoot = replacer.replacePagedLODs(node.get());
    if (newRoot.valid()) node = newRoot.get();
    
    int id = g_newNodeCounter++;
    g_readNodeMap[id] = node.get();
    node->setUserValue("GlobalID", id);
    node->setName(osgDB::getSimpleFileName(file));
    
    g_nodeRoot->addChild(node.get());
    g_nodeRootIdMap[node.get()] = id;
    return id;
}

int* UNITY_INTERFACE_API takeNewlyAddedNodes(int root, int* count)
{
    PagedIdAndChildList& addedNodes = g_addedPagingNodes[root];
    if (addedNodes.empty()) return NULL;
    if (count) *count = addedNodes.size();
    
    g_addedDataList.resize(addedNodes.size() * 2);
    for (unsigned int i = 0; i < addedNodes.size(); ++i)
    {
        g_addedDataList[2 * i] = addedNodes[i].first;
        g_addedDataList[2 * i + 1] = addedNodes[i].second;
    }
    addedNodes.clear();
    return &(g_addedDataList[0]);
}

int* UNITY_INTERFACE_API takeNewlyRemovedNodes(int root, int* count)
{
    PagedIdAndChildList& removedNodes = g_removedPagingNodes[root];
    if (removedNodes.empty()) return NULL;
    if (count) *count = removedNodes.size();
    
    g_removedDataList.resize(removedNodes.size() * 2);
    for (unsigned int i = 0; i < removedNodes.size(); ++i)
    {
        g_removedDataList[2 * i] = removedNodes[i].first;
        g_removedDataList[2 * i + 1] = removedNodes[i].second;
    }
    removedNodes.clear();
    return &(g_removedDataList[0]);
}

// Global updating functions
static osg::ref_ptr<DatabasePagerBridge> g_pager;
static osg::ref_ptr<SceneViewVisitor> g_sceneVisitor = new SceneViewVisitor;
static osg::ref_ptr<osg::FrameStamp> g_frameStamp = new osg::FrameStamp;
static double frameNumberCounter = 0;

void UNITY_INTERFACE_API setEyePosition(float x, float y, float z) { g_sceneVisitor->setEyePoint(osg::Vec3(x, y, z)); }
void UNITY_INTERFACE_API setViewTarget(float x, float y, float z) { g_sceneVisitor->setViewPoint(osg::Vec3(x, y, z)); }
void UNITY_INTERFACE_API setCameraUpDirection(float x, float y, float z) { g_sceneVisitor->setCameraUp(osg::Vec3(x, y, z)); }

void UNITY_INTERFACE_API setCameraProperties(float width, float height, float vFov, float zNear, float zFar, float lodScale)
{
    osg::Vec3 dir = g_sceneVisitor->getViewPoint() - g_sceneVisitor->getEyePoint(); dir.normalize();
    osg::Vec3 side = dir ^ g_sceneVisitor->getCameraUp(); side.normalize();
    osg::Vec3 up = side ^ dir;
    g_sceneVisitor->setCameraUp(side ^ dir);

    osg::Matrix M = osg::Matrix::lookAt(
        g_sceneVisitor->getEyePoint(), g_sceneVisitor->getViewPoint(), g_sceneVisitor->getCameraUp());
    osg::Matrix P = osg::Matrix::perspective(vFov, width / height, zNear, zFar);
    g_sceneVisitor->setLODScale(lodScale);
    g_sceneVisitor->computePixelSizeVector(width, height, P, M);
}

bool UNITY_INTERFACE_API updateDatabasePager(float deltaTime)
{
    frameNumberCounter += deltaTime * 60.0;
    g_frameStamp->setFrameNumber((int)frameNumberCounter);
    g_frameStamp->setReferenceTime(g_frameStamp->getReferenceTime() + deltaTime);
    g_frameStamp->setSimulationTime(g_frameStamp->getSimulationTime() + deltaTime);
    
    if (!g_pager)
    {
        osg::setNotifyLevel(osg::NOTICE);
        osg::setNotifyHandler(new NotifyHandler);
        g_pager = new DatabasePagerBridge;
        g_sceneVisitor->setDatabaseRequestHandler(g_pager.get());
    }

    g_sceneVisitor->setFrameStamp(g_frameStamp.get());
    g_sceneVisitor->reset();
    g_nodeRoot->accept(*g_sceneVisitor);
    
    g_pager->signalBeginFrame(g_frameStamp.get());
    g_pager->updateSceneGraph(*g_frameStamp);
    g_pager->signalEndFrame();

    printUserLog(LV_WARNING, "PAGER: %d, %lg", g_pager->getFileRequestListSize(), g_pager->getAverageTimeToMergeTiles());
    return true;
}

void UNITY_INTERFACE_API closeAll()
{
    g_sceneVisitor->setDatabaseRequestHandler(NULL);
    if (g_pager.valid()) g_pager->cancel();
    g_pager = NULL;
    g_addedPagingNodes.clear();
    g_removedPagingNodes.clear();
    
    g_nodeRoot->removeChildren(0, g_nodeRoot->getNumChildren());
    g_nodeRootIdMap.clear();
    g_readNodeMap.clear();
    g_newNodeCounter = 1;
}
