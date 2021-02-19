#ifndef OSG_LOADER_UPDATEDATABASEPAGER_HPP
#define OSG_LOADER_UPDATEDATABASEPAGER_HPP

#include <osg/CullingSet>
#include <osg/Geode>
#include <osg/PagedLOD>
#include <osg/ProxyNode>
#include <osgDB/Registry>
#include <osgDB/DatabasePager>
#include <osgDB/FileNameUtils>
#include <osgDB/ReadFile>
#include <osgDB/WriteFile>

class DatabasePagerBridge : public osgDB::DatabasePager
{
public:
    virtual void updateSceneGraph(const osg::FrameStamp& frameStamp);
    void addLoadedDataToBridge(const osg::FrameStamp& frameStamp);
    void removeExpiredSubgraphsFromBridge(const osg::FrameStamp& frameStamp);
};

class ExtendedPagedLOD : public osg::PagedLOD
{
public:
    ExtendedPagedLOD() : osg::PagedLOD(), lastTraversedChildID(-1) {}
    ExtendedPagedLOD(const osg::PagedLOD& copy, const osg::CopyOp& copyop = osg::CopyOp::SHALLOW_COPY)
    : osg::PagedLOD(copy, copyop) {}
    
    virtual bool removeExpiredChildren(double expiryTime, unsigned int expiryFrame, osg::NodeList& removedChildren);
    virtual void traverse(osg::NodeVisitor& nv);
    
    std::vector<int> lastTraversedChildID;
};

class SceneViewVisitor : public osg::NodeVisitor
{
public:
    SceneViewVisitor() : osg::NodeVisitor(CULL_VISITOR, TRAVERSE_ACTIVE_CHILDREN)
    {
        _viewport = new osg::Viewport;
        _pixelSizeVector.set(0.0f, 0.0f, 0.0f, 1.0f); _lodScale = 1.0f;
    }
    META_NodeVisitor("ManaVR", "SceneViewVisitor")
    
    void setEyePoint(const osg::Vec3& p) { _eye = p; }
    void setViewPoint(const osg::Vec3& p) { _viewPoint = p; }
    void setCameraUp(const osg::Vec3& p) { _up = p; }
    const osg::Vec3& getCameraUp() const { return _up; }
    
    void setLODScale(float v) { _lodScale = v; }
    float getLODScale() const { return _lodScale; }

    void computePixelSizeVector(float width, float height, const osg::Matrix& P, const osg::Matrix& M)
    {
        _viewport->width() = width; _viewport->height() = height;
        _pixelSizeVector = osg::CullingSet::computePixelSizeVector(*_viewport, P, M);
    }
    
    float clampedPixelSize(const osg::BoundingSphere& bs)
    { return fabs(bs.radius() / (bs.center() * _pixelSizeVector)); }
    
    virtual osg::Vec3 getEyePoint() const { return _eye; }
    virtual osg::Vec3 getViewPoint() const { return _viewPoint; }
    virtual void reset() {}
    
    virtual float getDistanceToEyePoint(const osg::Vec3& pos, bool useLODScale) const
    {
        if (useLODScale) return (pos- _eye).length() * getLODScale();
        else return (pos - _eye).length();
    }
    
    virtual float getDistanceToViewPoint(const osg::Vec3& pos, bool useLODScale) const
    {
        if (useLODScale) return (pos - _eye).length() * getLODScale();
        else return (pos - _eye).length();
    }
    
    virtual float getDistanceFromEyePoint(const osg::Vec3& pos, bool useLODScale) const
    { /*NOT IMPLEMENTED*/return 0.0f; }
    
protected:
    osg::ref_ptr<osg::Viewport> _viewport;
    osg::Vec3 _eye, _viewPoint, _up;
    osg::Vec4 _pixelSizeVector;
    float _lodScale;
};

#endif
