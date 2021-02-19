#include <osg/Version>
#include <osgDB/ReadFile>
#include <osgDB/WriteFile>
#include "Intersector.h"
#include "Export.h"

osg::ref_ptr<osg::Node> ReadIntersectionFileCallback::readNodeFile(const std::string& filename)
{
    return osgDB::readNodeFile(filename, options.get());
}

void IntersectionVisitorEx::apply(osg::PagedLOD& plod)
{
    if (_readCallback.valid())
    {
        ReadIntersectionFileCallback* rfc = static_cast<ReadIntersectionFileCallback*>(_readCallback.get());
        rfc->options = static_cast<osgDB::Options*>(plod.getDatabaseOptions());  // for EPT data...
    }
    osgUtil::IntersectionVisitor::apply(plod);
}

osgUtil::Intersector* PointIntersector::clone(osgUtil::IntersectionVisitor& iv)
{
    if (_coordinateFrame == MODEL && iv.getModelMatrix() == 0)
    {
        osg::ref_ptr<PointIntersector> cloned = new PointIntersector(_start, _end);
        cloned->_parent = this; cloned->_pickBias = _pickBias;
        return cloned.release();
    }

    osg::Matrix matrix;
    switch (_coordinateFrame)
    {
    case WINDOW:
        if (iv.getWindowMatrix()) matrix.preMult(*iv.getWindowMatrix());
        if (iv.getProjectionMatrix()) matrix.preMult(*iv.getProjectionMatrix());
        if (iv.getViewMatrix()) matrix.preMult(*iv.getViewMatrix());
        if (iv.getModelMatrix()) matrix.preMult(*iv.getModelMatrix());
        break;
    case PROJECTION:
        if (iv.getProjectionMatrix()) matrix.preMult(*iv.getProjectionMatrix());
        if (iv.getViewMatrix()) matrix.preMult(*iv.getViewMatrix());
        if (iv.getModelMatrix()) matrix.preMult(*iv.getModelMatrix());
        break;
    case VIEW:
        if (iv.getViewMatrix()) matrix.preMult(*iv.getViewMatrix());
        if (iv.getModelMatrix()) matrix.preMult(*iv.getModelMatrix());
        break;
    case MODEL:
        if (iv.getModelMatrix()) matrix = *iv.getModelMatrix();
        break;
    }

    osg::Matrix inverse = osg::Matrix::inverse(matrix);
    osg::ref_ptr<PointIntersector> cloned = new PointIntersector(_start * inverse, _end * inverse);
    cloned->_parent = this; cloned->_pickBias = _pickBias;
    return cloned.release();
}

void PointIntersector::intersect(osgUtil::IntersectionVisitor& iv, osg::Drawable* drawable)
{
    osg::BoundingBox bb = drawable->getBoundingBox();
    bb.xMin() -= _pickBias; bb.xMax() += _pickBias;
    bb.yMin() -= _pickBias; bb.yMax() += _pickBias;
    bb.zMin() -= _pickBias; bb.zMax() += _pickBias;

    osg::Vec3d s(_start), e(_end);
    if (!intersectAndClip(s, e, bb)) return;
    if (iv.getDoDummyTraversal()) return;

    osg::Geometry* geometry = drawable->asGeometry();
    if (geometry)
    {
        osg::Vec3Array* vertices = dynamic_cast<osg::Vec3Array*>(geometry->getVertexArray());
        if (!vertices) return;

        osg::Vec3d dir = e - s;
        double invLength = 1.0 / dir.length();
        for (unsigned int i = 0; i < vertices->size(); ++i)
        {
            double distance = fabs((((*vertices)[i] - s) ^ dir).length());
            distance *= invLength;
            if (_pickBias < distance) continue;

            Intersection hit;
            hit.ratio = distance;
            hit.nodePath = iv.getNodePath();
            hit.drawable = drawable;
            hit.matrix = iv.getModelMatrix();
            hit.localIntersectionPoint = (*vertices)[i];
            hit.indexList.push_back(i);
            insertIntersection(hit);
        }
    }
}

osgUtil::Intersector* PointIntersector2::clone(osgUtil::IntersectionVisitor& iv)
{
    if (_coordinateFrame == MODEL && iv.getModelMatrix() == 0)
    {
        osg::ref_ptr<PointIntersector2> cloned = new PointIntersector2(_polytope);
        cloned->_parent = this; cloned->allPoints = allPoints; cloned->allColors = allColors;
        return cloned.release();
    }

    osg::Matrix matrix;
    switch (_coordinateFrame)
    {
    case WINDOW:
        if (iv.getWindowMatrix()) matrix.preMult(*iv.getWindowMatrix());
        if (iv.getProjectionMatrix()) matrix.preMult(*iv.getProjectionMatrix());
        if (iv.getViewMatrix()) matrix.preMult(*iv.getViewMatrix());
        if (iv.getModelMatrix()) matrix.preMult(*iv.getModelMatrix());
        break;
    case PROJECTION:
        if (iv.getProjectionMatrix()) matrix.preMult(*iv.getProjectionMatrix());
        if (iv.getViewMatrix()) matrix.preMult(*iv.getViewMatrix());
        if (iv.getModelMatrix()) matrix.preMult(*iv.getModelMatrix());
        break;
    case VIEW:
        if (iv.getViewMatrix()) matrix.preMult(*iv.getViewMatrix());
        if (iv.getModelMatrix()) matrix.preMult(*iv.getModelMatrix());
        break;
    case MODEL:
        if (iv.getModelMatrix()) matrix = *iv.getModelMatrix();
        break;
    }

    osg::Polytope transformedPolytope;
    transformedPolytope.setAndTransformProvidingInverse(_polytope, matrix);
    osg::ref_ptr<PointIntersector2> cloned = new PointIntersector2(transformedPolytope);
    cloned->_parent = this; cloned->allPoints = allPoints; cloned->allColors = allColors;
    return cloned.release();
}

void PointIntersector2::intersect(osgUtil::IntersectionVisitor& iv, osg::Drawable* drawable)
{
    osg::Geometry* geom = drawable->asGeometry();
    if (!allPoints || !geom || !(geom && geom->getVertexArray())) return;
    if (!_polytope.contains(drawable->getBoundingBox())) return;

    Intersection hit;
    hit.nodePath = iv.getNodePath();
    hit.drawable = drawable;
    hit.matrix = iv.getModelMatrix();

    osg::Vec3Array* va = static_cast<osg::Vec3Array*>(geom->getVertexArray());
    osg::Vec4Array* ca = dynamic_cast<osg::Vec4Array*>(geom->getColorArray());
    osg::Vec4ubArray* caub = dynamic_cast<osg::Vec4ubArray*>(geom->getColorArray());

    osg::Matrix m = hit.matrix.valid() ? *hit.matrix : osg::Matrix();
    osg::Vec3 center; int numData = 0;
    for (unsigned int i = 0; i < va->size(); ++i)
    {
        const osg::Vec3& v = (*va)[i];
        if (_polytope.contains(v))
        {
            center += v;
            allPoints->push_back(v * m);
            if (ca)
                allColors->push_back((*ca)[i]);
            else if (caub)
            {
                const osg::Vec4ub& c = (*caub)[i];
                allColors->push_back(osg::Vec4((float)c[0] / 255.0f, (float)c[1] / 255.0f,
                                               (float)c[2] / 255.0f, (float)c[3] / 255.0f));
            }
            numData++;
        }
    }
    hit.localIntersectionPoint = center / (float)numData;
    insertIntersection(hit);
}

extern std::map<osg::Node*, int> g_nodeRootIdMap;
extern osg::ref_ptr<osg::Group> g_nodeRoot;
extern void printUserLog(int level, const char* format, ...);

struct InteresctionResult
{
    std::vector<osg::Vec3> allPoints;
    std::vector<osg::Vec4> allColors;
};
static InteresctionResult g_intersection;
static osg::ref_ptr<ReadIntersectionFileCallback> g_readCallback = new ReadIntersectionFileCallback;

int UNITY_INTERFACE_API beginIntersectWithLineSegment(float* s, float* e, float pointChkBias,
                                                      bool pointsOnly, bool readAll)
{
    osg::Vec3 start(s[0], s[1], s[2]), end(e[0], e[1], e[2]);
    osg::ref_ptr<osgUtil::LineSegmentIntersector> intersector = pointsOnly
        ? new PointIntersector(start, end)
        : new osgUtil::LineSegmentIntersector(osgUtil::Intersector::MODEL, start, end);
    if (pointsOnly)
        ((PointIntersector*)intersector.get())->setPickBias(pointChkBias);

    IntersectionVisitorEx iv(intersector.get());
    iv.setReadCallback(readAll ? g_readCallback.get() : NULL);
    intersector->setIntersectionLimit(osgUtil::Intersector::LIMIT_NEAREST);
    g_nodeRoot->accept(iv);

    static float result[3] = { 0.0f };
    if (intersector->containsIntersections())
    {
        osgUtil::LineSegmentIntersector::Intersections& all = intersector->getIntersections();
        for (osgUtil::LineSegmentIntersector::Intersections::const_iterator itr = all.begin();
             itr != all.end(); ++itr)
        {
            const osgUtil::LineSegmentIntersector::Intersection& is = *itr;
            g_intersection.allPoints.push_back(is.getWorldIntersectPoint());
        }
        return g_intersection.allPoints.size();
    }
    return 0;
}

int UNITY_INTERFACE_API beginIntersectWithPolytope(float* planeNormals, float* distances, int planeCount,
                                                   bool pointsOnly, bool readAll)
{
    osg::Polytope polytope;
    for (int i = 0; i < planeCount; ++i)
    {
        int index = i * 3;
        osg::Vec3 n(planeNormals[index + 0], planeNormals[index + 1], planeNormals[index + 2]);
        polytope.add(osg::Plane(n, distances[i]));
    }

    osg::ref_ptr<osgUtil::PolytopeIntersector> intersector = pointsOnly
        ? new PointIntersector2(polytope)
        : new osgUtil::PolytopeIntersector(osgUtil::Intersector::MODEL, polytope);
    if (pointsOnly)
    {
        ((PointIntersector2*)intersector.get())->allPoints = &(g_intersection.allPoints);
        ((PointIntersector2*)intersector.get())->allColors = &(g_intersection.allColors);
    }

    IntersectionVisitorEx iv(intersector.get());
    iv.setReadCallback(readAll ? g_readCallback.get() : NULL);
    g_nodeRoot->accept(iv);
    if (intersector->containsIntersections())
    {
        if (!pointsOnly)
        {
            osgUtil::PolytopeIntersector::Intersections& all = intersector->getIntersections();
            for (osgUtil::PolytopeIntersector::Intersections::const_iterator itr = all.begin();
                 itr != all.end(); ++itr)
            {
                const osgUtil::PolytopeIntersector::Intersection& is = *itr;
                g_intersection.allPoints.push_back(is.localIntersectionPoint);
            }
        }
        return g_intersection.allPoints.size();
    }
    return 0;
}

float* UNITY_INTERFACE_API getIntersectedPositions(int* resultCount)
{
    if (resultCount) *resultCount = g_intersection.allPoints.size();
    if (g_intersection.allPoints.empty()) return NULL;
    return (float*)&(g_intersection.allPoints[0]);
}

float* UNITY_INTERFACE_API getIntersectedColors(int* resultCount)
{
    if (resultCount) *resultCount = g_intersection.allColors.size();
    if (g_intersection.allColors.empty()) return NULL;
    return (float*)&(g_intersection.allColors[0]);
}

void UNITY_INTERFACE_API endIntersection()
{
    g_intersection.allPoints.clear();
    g_intersection.allColors.clear();
}
