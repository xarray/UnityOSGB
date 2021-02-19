#ifndef OSG_LOADER_INTERSECTOR_HPP
#define OSG_LOADER_INTERSECTOR_HPP

#include <osg/Geometry>
#include <osg/PagedLOD>
#include <osg/MatrixTransform>
#include <osgUtil/LineSegmentIntersector>
#include <osgUtil/PolytopeIntersector>

struct ReadIntersectionFileCallback : public osgUtil::IntersectionVisitor::ReadCallback
{
    virtual osg::ref_ptr<osg::Node> readNodeFile(const std::string& filename);
    osg::ref_ptr<osgDB::Options> options;
};

class IntersectionVisitorEx : public osgUtil::IntersectionVisitor
{
public:
    IntersectionVisitorEx(osgUtil::Intersector* i) : osgUtil::IntersectionVisitor(i) {}
    virtual void apply(osg::PagedLOD& plod);
};

class PointIntersector : public osgUtil::LineSegmentIntersector
{
public:
    PointIntersector()
        : osgUtil::LineSegmentIntersector(MODEL, 0.0, 0.0), _pickBias(0.01f) {}
    PointIntersector(const osg::Vec3& start, const osg::Vec3& end)
        : osgUtil::LineSegmentIntersector(MODEL, start, end), _pickBias(0.01f) {}
    PointIntersector(CoordinateFrame cf, double x, double y)
        : osgUtil::LineSegmentIntersector(cf, x, y), _pickBias(0.01f) {}

    void setPickBias(float bias) { _pickBias = bias; }
    float getPickBias() const { return _pickBias; }

    virtual Intersector* clone(osgUtil::IntersectionVisitor& iv);
    virtual void intersect(osgUtil::IntersectionVisitor& iv, osg::Drawable* drawable);

protected:
    virtual ~PointIntersector() {}
    float _pickBias;
};

class PointIntersector2 : public osgUtil::PolytopeIntersector
{
public:
    PointIntersector2(const osg::Polytope& polytope)
        : PolytopeIntersector(MODEL, polytope), allPoints(NULL), allColors(NULL) {}
    PointIntersector2(CoordinateFrame cf, const osg::Polytope& polytope)
        : PolytopeIntersector(cf, polytope), allPoints(NULL), allColors(NULL) {}
    PointIntersector2(CoordinateFrame cf, double xMin, double yMin, double xMax, double yMax)
        : PolytopeIntersector(cf, xMin, yMin, xMax, yMax), allPoints(NULL), allColors(NULL) {}

    virtual Intersector* clone(osgUtil::IntersectionVisitor& iv);
    virtual void intersect(osgUtil::IntersectionVisitor& iv, osg::Drawable* drawable);
    
    std::vector<osg::Vec3>* allPoints;
    std::vector<osg::Vec4>* allColors;

protected:
    virtual ~PointIntersector2() {}
};

#endif
