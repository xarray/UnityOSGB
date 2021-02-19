#ifndef OSG_LOADER_EXPORT_HPP
#define OSG_LOADER_EXPORT_HPP

#include <InterfaceBase.h>
#include <map>
#include <set>
#include <string>
#include <vector>

#define MANA_PLUGIN_NAME "[Mana_OpenSceneGraphLoader]"
#define MANA_PLUGIN_VERSION 100
typedef void (*UserLogFunction)(const char*, int);

extern "C"
{
    UNITY_INTERFACE_EXPORT const char* UNITY_INTERFACE_API getPluginName();
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API getPluginVersion();
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API setUserLogFunction(UserLogFunction func);
    
    // Read functions
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API requestNodeFile(const char* file);
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API takeNewlyAddedNodes(int root, int* count);
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API takeNewlyRemovedNodes(int root, int* count);
    
    // Data obtain functions
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API beginReadNode(int id);
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API beginReadPagedNode(int id, int location);
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API endReadNode(bool eraseNodeData);
    
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API updatePagedNodeState(int id, int* count);
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API removeNode(int id);
    
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API readNodeLocalTransform(int subID, int* count);
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API readNodeLocalTransformDecomposition(
        int subID, float* pos, float* quat, float* scale);
    
    UNITY_INTERFACE_EXPORT const char* UNITY_INTERFACE_API readNodeNameAndType(int subID, int* type);
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API readNodeChildrenCount(int subID);
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API readNodeChildren(int subID, int* count);
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API readNodeGlobalID(int subID);
    
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API readMeshCount(int subID);
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API readMeshes(int subID, int* count);
    
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API readMeshWorldTransform(int subID, int* count);
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API readPrimitiveCounts(int subID, int* pts, int* lines, int* tris);
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API readPoints(int subID, int* count);
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API readLines(int subID, int* count);
    UNITY_INTERFACE_EXPORT int* UNITY_INTERFACE_API readTriangles(int subID, int* count);
    
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API readVertexCount(int subID);
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API readVertices(int subID, int* count);
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API readVertexColors(int subID, int* count);
    UNITY_INTERFACE_EXPORT char* UNITY_INTERFACE_API readVertexColorsUB(int subID, int* count);
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API readNormals(int subID, int* count);
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API readUV(int subID, int channel, int* count);
    
    UNITY_INTERFACE_EXPORT const char* UNITY_INTERFACE_API readTextureName(int subID);
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API readTextureSize(int subID, int* w, int* h);
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API readTextureFormat(int subID, int* wrapMode);
    UNITY_INTERFACE_EXPORT void* UNITY_INTERFACE_API readTexture(int subID, int* dataSize);

    // Intersection with linesegment: beginIntersectWithLineSegment(xyz, xyz, ref n, bool)
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API beginIntersectWithLineSegment(
            float* start, float* end, float pointChkBias, bool pointsOnly, bool readAll);
    // Intersection with polytope: beginIntersectWithPolytope((xyz * m),(d * m), m, ref n, bool)
    UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API beginIntersectWithPolytope(
            float* planeNormals, float* distances, int planeCount, bool pointsOnly, bool readAll);
    
    // Intersection result: position (xyz * n), color (rgba * n)
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API getIntersectedPositions(int* resultCount);
    UNITY_INTERFACE_EXPORT float* UNITY_INTERFACE_API getIntersectedColors(int* resultCount);

    // Must always use a begin/end pair
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API endIntersection();

    // Global updating functions
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API setEyePosition(float x, float y, float z);
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API setViewTarget(float x, float y, float z);
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API setCameraUpDirection(float x, float y, float z);
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API setCameraProperties(float width, float height, float vFov,
                                                                        float zNear, float zFar, float lodScale);
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API updateDatabasePager(float deltaTime);
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API closeAll();
}

#endif
