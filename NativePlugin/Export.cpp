#if _WIN32
    #include <windows.h>
#endif
#include "Export.h"
#include "UpdateDatabasePager.h"

#include <osg/ValueObject>
#include <osg/TriangleLinePointIndexFunctor>
#include <osg/Texture2D>
#include <osg/Geometry>
#include <osg/Geode>
#include <osg/PagedLOD>
#include <osg/ProxyNode>
#include <osg/Transform>
#include <osgDB/Registry>

#ifdef OSG_LIBRARY_STATIC
USE_OSGPLUGIN(3ds)
USE_OSGPLUGIN(bmp)
USE_OSGPLUGIN(dds)
USE_OSGPLUGIN(gif)
USE_OSGPLUGIN(jpeg)
USE_OSGPLUGIN(obj)
USE_OSGPLUGIN(OpenFlight)
USE_OSGPLUGIN(osg)
USE_OSGPLUGIN(osg2)
USE_OSGPLUGIN(png)
USE_OSGPLUGIN(rgb)
USE_OSGPLUGIN(shp)
USE_OSGPLUGIN(stl)
USE_OSGPLUGIN(tga)
USE_OSGPLUGIN(txp)
USE_OSGPLUGIN(vtf)

USE_DOTOSGWRAPPER_LIBRARY(osg)
USE_DOTOSGWRAPPER_LIBRARY(osgSim)
USE_DOTOSGWRAPPER_LIBRARY(osgTerrain)
USE_SERIALIZER_WRAPPER_LIBRARY(osg)
USE_SERIALIZER_WRAPPER_LIBRARY(osgSim)
USE_SERIALIZER_WRAPPER_LIBRARY(osgTerrain)
#endif

#include <stdarg.h>
#include <fstream>
#include <sstream>
#include <iostream>

// Global structures and variables
void defaultLog(const char* msg, int level) { printf("Log-%d: %s\n", level, msg); }
enum UserLogLevel { LV_DEBUG = 0, LV_WARNING, LV_FATAL };
static UserLogFunction Logger = defaultLog;

const char* UNITY_INTERFACE_API getPluginName() { return MANA_PLUGIN_NAME; }
int UNITY_INTERFACE_API getPluginVersion() { return MANA_PLUGIN_VERSION; }
void UNITY_INTERFACE_API setUserLogFunction(UserLogFunction func) { Logger = func; }

void printUserLog(int level, const char* format, ...)
{
    char buffer[512] = {0};
    va_list arg;
    va_start(arg, format);
    vsnprintf(buffer, 511, format, arg);
    va_end(arg);
    Logger(buffer, level);
}

// OSG variables
std::map<int, osg::observer_ptr<osg::Node> > g_readNodeMap;
std::map<osg::Node*, int> g_nodeRootIdMap;
osg::ref_ptr<osg::Group> g_nodeRoot = new osg::Group;
unsigned int g_newNodeCounter = 1;  // 0 is reserved

// Data obtain functions
enum NodeType
{
    UNSET_NODE = 0, GEOMETRY_NODE = 1, GROUP_NODE = 2,
    LOD_NODE = 3, PAGEDLOD_NODE = 4
};

class NodeDataVisitor : public osg::NodeVisitor
{
public:
    void apply(osg::Transform& transform)
    {
        osg::Matrix matrix;
        transform.computeLocalToWorldMatrix(matrix, this);
        applyNode(transform, &matrix);
        
        pushMatrix(matrix);
        pushStateSet(transform.getStateSet());
        traverse(transform);
        popStateSet(transform.getStateSet());
        popMatrix();
        popNodeStack();
    }
    
    virtual void apply(osg::PagedLOD& node)
    {
        applyNode(node, NULL, PAGEDLOD_NODE);
        pushStateSet(node.getStateSet());
        traverse(node);
        popStateSet(node.getStateSet());
        popNodeStack();
    }
    
    virtual void apply(osg::Node& node)
    {
        applyNode(node, NULL);
        pushStateSet(node.getStateSet());
        traverse(node);
        popStateSet(node.getStateSet());
        popNodeStack();
    }
    
    virtual void apply(osg::Geode& node)
    {
        applyNode(node, NULL);
        pushStateSet(node.getStateSet());
        for (unsigned int i = 0; i < node.getNumDrawables(); ++i)
        {
            osg::Geometry* geom = node.getDrawable(i)->asGeometry();
            if (geom)
            {
                pushStateSet(geom->getStateSet());
                applyGeometry(*geom);
                popStateSet(geom->getStateSet());
            }
        }
        popStateSet(node.getStateSet());
        popNodeStack();
    }
    
    void pushMatrix(osg::Matrix& matrix)
    {
        if (!_matrixStack.empty()) _matrixStack.push_back(_matrixStack.back() * matrix);
        else _matrixStack.push_back(matrix);
    }
    void popMatrix() { _matrixStack.pop_back(); }
    
    void pushStateSet(osg::StateSet* ss)
    {
        if (!ss) return;
        osg::StateSet* newSS = new osg::StateSet(*_statesetStack.back());
        osg::StateSet::TextureAttributeList tattrs = ss->getTextureAttributeList();
        
        for ( unsigned int u=0; u<tattrs.size(); ++u )
        {
            osg::StateSet::AttributeList::iterator itr =
                tattrs[u].find( osg::StateAttribute::TypeMemberPair(osg::StateAttribute::TEXTURE, 0) );
            if ( itr==tattrs[u].end() ) continue;
            
            osg::Texture* texture = texture = static_cast<osg::Texture*>(itr->second.first.get());
            if ( texture )
            {
                _textureList.push_back( TextureData(u, ss, texture) );
                newSS->setTextureAttributeAndModes(u, texture, osg::StateAttribute::ON);
            }
        }
        _statesetStack.push_back(newSS);
    }
    
    void popStateSet(osg::StateSet* ss)
    { if (ss) _statesetStack.pop_back(); }
    
    void applyNode(osg::Node& node, osg::Matrix* matrix, int nodeType = UNSET_NODE)
    {
        NodeData data;
        data.node = &node;
        data.specialNodeType = nodeType;
        if (matrix) data.transform = *matrix;
        if (_nodeIdStack.size() > 0)
        {
            data.parentNode = _nodeIdStack.back();
            if (_nodeDataMap.find(data.parentNode) != _nodeDataMap.end())
                _nodeDataMap[data.parentNode].childNodes.push_back(_idCounter);
        }
        
        _nodeDataMap[_idCounter] = data;
        _nodeIdStack.push_back(_idCounter);
        _idCounter++;
    }
    
    void popNodeStack()
    {
        _nodeIdStack.pop_back();
    }
    
    void applyGeometry(osg::Geometry& geometry)
    {
        MeshData data;
        geometry.accept(data.functor);
        data.geometry = &geometry;
        data.stateset = _statesetStack.back();
        data.worldTransform = _matrixStack.empty() ? osg::Matrixf() : _matrixStack.back();
        if (_nodeIdStack.size() > 0)
        {
            data.parentNode = _nodeIdStack.back();
            if (_nodeDataMap.find(data.parentNode) != _nodeDataMap.end())
                _nodeDataMap[data.parentNode].childMeshes.push_back(_idCounter);
        }
        
        _meshDataMap[_idCounter] = data;
        _idCounter++;
    }
    
    NodeDataVisitor(int id) : osg::NodeVisitor(osg::NodeVisitor::TRAVERSE_ALL_CHILDREN)
    { _statesetStack.push_back(new osg::StateSet); _idCounter = 0; _nodeID = id; }
    
public:
    struct NodeData
    {
        osg::Matrixf transform;
        osg::ref_ptr<osg::Node> node;
        std::vector<int> childNodes, childMeshes;
        int parentNode, specialNodeType;
    };
    std::map<int, NodeData> _nodeDataMap;
    
    struct CollecTrianglesOperator
    {
        std::vector<int> triangles, lines, points;
        void operator()(unsigned int i0) { points.push_back(i0); }

        void operator()(unsigned int i0, unsigned int i1)
        {
            if (i0 == i1) return;
            lines.push_back(i0); lines.push_back(i1);
        }

        void operator()(unsigned int i0, unsigned int i1, unsigned int i2)
        {
            if (i0 == i1 || i0 == i2 || i1 == i2) return;  // change clockwise for LH use
            triangles.push_back(i0); triangles.push_back(i2); triangles.push_back(i1);
        }
    };
    
    struct MeshData
    {
        osg::ref_ptr<osg::Geometry> geometry;
        osg::ref_ptr<osg::StateSet> stateset;
        osg::TriangleLinePointIndexFunctor<CollecTrianglesOperator> functor;
        osg::Matrixf worldTransform;
        int parentNode;
    };
    std::map<int, MeshData> _meshDataMap;
    
    struct TextureData
    {
        osg::StateSet* stateset;
        osg::Texture* texture;
        unsigned int unit;
        
        TextureData(unsigned int u, osg::StateSet* ss, osg::Texture* t)
        : stateset(ss), texture(t), unit(u) {}
    };
    std::vector<TextureData> _textureList;
    
    std::vector<int> _nodeIdStack;
    int _idCounter, _nodeID;
    
    typedef std::vector< osg::ref_ptr<osg::StateSet> > StateSetStack;
    StateSetStack _statesetStack;
    std::vector<osg::Matrix> _matrixStack;
};
NodeDataVisitor* g_visitor = NULL;

bool UNITY_INTERFACE_API beginReadNode(int id)
{
    std::map<int, osg::observer_ptr<osg::Node> >::iterator itr = g_readNodeMap.find(id);
    if (itr == g_readNodeMap.end()) return false;
    
    if (g_visitor) delete g_visitor;
    g_visitor = new NodeDataVisitor(id);
    itr->second->accept(*g_visitor);
    return true;
}

bool UNITY_INTERFACE_API beginReadPagedNode(int id, int location)
{
    std::map<int, osg::observer_ptr<osg::Node> >::iterator itr = g_readNodeMap.find(id);
    if (itr == g_readNodeMap.end()) return false;
    
    osg::Group* group = dynamic_cast<osg::Group*>(itr->second.get());
    if (!group)
    {
        g_readNodeMap.erase(itr);
        return false;
    }
    
    if (g_visitor) delete g_visitor;
    g_visitor = new NodeDataVisitor(-1);
    if (location < (int)group->getNumChildren())
        group->getChild(location)->accept(*g_visitor);
    return true;
}

bool UNITY_INTERFACE_API endReadNode(bool eraseNodeData)
{
    if (!g_visitor) return false;
    int id = g_visitor->_nodeID;
    
    if (eraseNodeData && id >= 0)
    {
        for (std::map<int, NodeDataVisitor::MeshData>::iterator itr = g_visitor->_meshDataMap.begin();
             itr != g_visitor->_meshDataMap.end(); ++itr)
        {
            NodeDataVisitor::MeshData& meshData = itr->second;
            if (!meshData.geometry) continue;
            
            meshData.geometry->setVertexArray(NULL);
            meshData.geometry->setNormalArray(NULL);
            meshData.geometry->setColorArray(NULL);
            meshData.geometry->setTexCoordArray(0, NULL);
            meshData.geometry->removePrimitiveSet(0, meshData.geometry->getNumPrimitiveSets());
        }
        
        for (unsigned int i = 0; i < g_visitor->_textureList.size(); ++i)
        {
            NodeDataVisitor::TextureData& texData = g_visitor->_textureList[i];
            if (texData.stateset) texData.stateset->removeTextureAttribute(texData.unit, texData.texture);
        }
    }
    delete g_visitor; g_visitor = NULL;
    return true;
}

int* UNITY_INTERFACE_API updatePagedNodeState(int id, int* count)
{
    std::map<int, osg::observer_ptr<osg::Node> >::iterator itr = g_readNodeMap.find(id);
    if (itr == g_readNodeMap.end()) return NULL;
    
    ExtendedPagedLOD* explod = dynamic_cast<ExtendedPagedLOD*>(itr->second.get());
    if (explod)
    {
        if (count) *count = explod->lastTraversedChildID.size();
        if (!explod->lastTraversedChildID.empty())
            return &(explod->lastTraversedChildID[0]);
    }
    else if (count)
    {
        if (itr->second.valid()) *count = -1;  // invalid state
        else *count = -2;  // node already removed
    }
    return NULL;
}

bool UNITY_INTERFACE_API removeNode(int id)
{
    std::map<int, osg::observer_ptr<osg::Node> >::iterator itr = g_readNodeMap.find(id);
    if (itr != g_readNodeMap.end())
    {
        if (g_nodeRoot->removeChild(itr->second.get()))
        {
            std::map<osg::Node*, int>::iterator itr2 = g_nodeRootIdMap.find(itr->second.get());
            if (itr2 != g_nodeRootIdMap.end()) g_nodeRootIdMap.erase(itr2);
            g_readNodeMap.erase(itr);
            return true;
        }
    }
    return false;
}

#define GET_NODE_DATA(data, subID, rtn) if (!g_visitor || subID < 0) return rtn; \
                                        NodeDataVisitor::NodeData& data = g_visitor->_nodeDataMap[subID]; \
                                        if (!data.node) return rtn;
#define GET_MESH_DATA(data, subID, rtn) if (!g_visitor || subID < 0) return rtn; \
                                        NodeDataVisitor::MeshData& data = g_visitor->_meshDataMap[subID]; \
                                        if (!data.geometry) return rtn;

float* UNITY_INTERFACE_API readNodeLocalTransform(int subID, int* count)
{
    GET_NODE_DATA(data, subID, NULL);
    if (count) *count = 16;
    return data.transform.ptr();
}

bool UNITY_INTERFACE_API readNodeLocalTransformDecomposition(int subID, float* pos, float* quat, float* scale)
{
    GET_NODE_DATA(data, subID, false);
    osg::Vec3f p, s; osg::Quat r, so;
    data.transform.decompose(p, r, s, so);
    
    if (pos) { for (int i = 0; i < 3; ++i) *(pos + i) = p[i]; }
    if (quat) { for (int i = 0; i < 4; ++i) *(quat + i) = r[i]; }
    if (scale) { for (int i = 0; i < 3; ++i) *(scale + i) = s[i]; }
    return true;
}

const char* UNITY_INTERFACE_API readNodeNameAndType(int subID, int* type)
{
    GET_NODE_DATA(data, subID, NULL);
    if (type)
    {
        if (data.specialNodeType > 0) *type = data.specialNodeType;
        else if (data.childMeshes.size() > 0) *type = GEOMETRY_NODE;
        else if (data.childNodes.size() > 0) *type = GROUP_NODE;
        else *type = UNSET_NODE;
    }
    return data.node->getName().c_str();
}

int UNITY_INTERFACE_API readNodeChildrenCount(int subID)
{
    GET_NODE_DATA(data, subID, 0);
    return data.childNodes.size();
}

int* UNITY_INTERFACE_API readNodeChildren(int subID, int* count)
{
    GET_NODE_DATA(data, subID, false);
    if (count) *count = data.childNodes.size();
    return &(data.childNodes[0]);
}

int UNITY_INTERFACE_API readNodeGlobalID(int subID)
{
    int globalID = 0;
    GET_NODE_DATA(data, subID, 0);
    if (!data.node->getUserValue("GlobalID", globalID)) return 0;
    
    std::map<int, osg::observer_ptr<osg::Node> >::iterator itr = g_readNodeMap.find(globalID);
    if (itr != g_readNodeMap.end()) return itr->first;
    return 0;
}

int UNITY_INTERFACE_API readMeshCount(int subID)
{
    GET_NODE_DATA(data, subID, 0);
    return data.childMeshes.size();
}

int* UNITY_INTERFACE_API readMeshes(int subID, int* count)
{
    GET_NODE_DATA(data, subID, NULL);
    if (count) *count = data.childMeshes.size();
    return &(data.childMeshes[0]);
}

float* UNITY_INTERFACE_API readMeshWorldTransform(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    if (count) *count = 16;
    return data.worldTransform.ptr();
}

bool UNITY_INTERFACE_API readPrimitiveCounts(int subID, int* pts, int* lines, int* tris)
{
    GET_MESH_DATA(data, subID, false);
    if (pts) *pts = data.functor.points.size();
    if (lines) *lines = data.functor.lines.size();
    if (tris) *tris = data.functor.triangles.size();
    return true;
}

int* UNITY_INTERFACE_API readPoints(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    if (count) *count = data.functor.points.size();
    return &(data.functor.points[0]);
}

int* UNITY_INTERFACE_API readLines(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    if (count) *count = data.functor.lines.size();
    return &(data.functor.lines[0]);
}

int* UNITY_INTERFACE_API readTriangles(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    if (count) *count = data.functor.triangles.size();
    return &(data.functor.triangles[0]);
}

int UNITY_INTERFACE_API readVertexCount(int subID)
{
    GET_MESH_DATA(data, subID, 0);
    osg::Vec3Array* va = dynamic_cast<osg::Vec3Array*>(data.geometry->getVertexArray());
    return va ? va->size() : 0;
}

float* UNITY_INTERFACE_API readVertices(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    osg::Vec3Array* va = dynamic_cast<osg::Vec3Array*>(data.geometry->getVertexArray());
    if (!va) return NULL;
    
    if (count) *count = va->size() * 3;
    return (float*)&(va->front());
}

float* UNITY_INTERFACE_API readVertexColors(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    osg::Vec4Array* va = dynamic_cast<osg::Vec4Array*>(data.geometry->getColorArray());
    if (!va) return NULL;
    
    if (count) *count = va->size() * 4;
    return (float*)&(va->front());
}

char* UNITY_INTERFACE_API readVertexColorsUB(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    osg::Vec4ubArray* va = dynamic_cast<osg::Vec4ubArray*>(data.geometry->getColorArray());
    if (!va) return NULL;

    if (count) *count = va->size() * 4;
    return (char*)&(va->front());
}

float* UNITY_INTERFACE_API readNormals(int subID, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    osg::Vec3Array* va = dynamic_cast<osg::Vec3Array*>(data.geometry->getNormalArray());
    if (!va) return NULL;
    
    if (count) *count = va->size() * 3;
    return (float*)&(va->front());
}

float* UNITY_INTERFACE_API readUV(int subID, int channel, int* count)
{
    GET_MESH_DATA(data, subID, NULL);
    osg::Vec2Array* va = dynamic_cast<osg::Vec2Array*>(data.geometry->getTexCoordArray(channel));
    if (!va) return NULL;
    
    if (count) *count = va->size() * 2;
    return (float*)&(va->front());
}

static osg::Texture2D* getTextureFromMeshData(NodeDataVisitor::MeshData& data, int unit = 0)
{
    if (!data.stateset) return NULL;
    osg::StateSet::TextureAttributeList tattrs = data.stateset->getTextureAttributeList();
    if (tattrs.size() > unit)
    {
        osg::StateSet::AttributeList::iterator itr =
            tattrs[unit].find(osg::StateAttribute::TypeMemberPair(osg::StateAttribute::TEXTURE, 0));
        if (itr != tattrs[unit].end())
            return static_cast<osg::Texture2D*>(itr->second.first.get());
    }
    return NULL;
}

const char* UNITY_INTERFACE_API readTextureName(int subID)
{
    GET_MESH_DATA(data, subID, NULL);
    osg::Texture2D* tex2D = getTextureFromMeshData(data);
    if (tex2D && tex2D->getImage()) return tex2D->getImage()->getFileName().c_str();
    return NULL;
}

bool UNITY_INTERFACE_API readTextureSize(int subID, int* w, int* h)
{
    GET_MESH_DATA(data, subID, false);
    osg::Texture2D* tex2D = getTextureFromMeshData(data);
    if (tex2D && tex2D->getImage()) 
    {
        if (w) *w = tex2D->getImage()->s();
        if (h) *h = tex2D->getImage()->t();
        return true;
    }
    return false;
}

int UNITY_INTERFACE_API readTextureFormat(int subID, int* wrapMode)
{
    GET_MESH_DATA(data, subID, 0);
    osg::Texture2D* tex2D = getTextureFromMeshData(data);
    if (tex2D && tex2D->getImage())
    {
        if (wrapMode)
        {
            switch (tex2D->getWrap(osg::Texture::WRAP_S))
            {
            case osg::Texture::REPEAT: *wrapMode = 1; break;
            case osg::Texture::MIRROR: *wrapMode = 2; break;
            default: *wrapMode = 0; break;
            }
        }
        
        switch (tex2D->getImage()->getPixelFormat())
        {
        case GL_DEPTH_COMPONENT: case GL_LUMINANCE: case GL_ALPHA: return 1;
        case GL_LUMINANCE_ALPHA: return 2;
        case GL_RGB: case GL_BGR: return 3;
        case GL_RGBA: case GL_BGRA: return 4;
        case GL_COMPRESSED_RGB_S3TC_DXT1_EXT: return 13;
        case GL_COMPRESSED_RGBA_S3TC_DXT1_EXT: return 14;
        case GL_COMPRESSED_RGBA_S3TC_DXT5_EXT: return 54;
        default: return 0;  // 0: invalid, 1-4: rgba, 13-14: DXT1, 54: DXT5
        }
    }
    return 0;
}

void* UNITY_INTERFACE_API readTexture(int subID, int* dataSize)
{
    GET_MESH_DATA(data, subID, NULL);
    osg::Texture2D* tex2D = getTextureFromMeshData(data);
    if (tex2D && tex2D->getImage())
    {
        osg::Image* image = tex2D->getImage();
        if (dataSize) *dataSize = image->getTotalSizeInBytes();
        return (void*)image->data();
    }
    return NULL;
}
