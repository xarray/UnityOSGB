CMAKE_MINIMUM_REQUIRED(VERSION 3.1)
PROJECT(OSG_Unity_Plugin)

SET(CMAKE_DEBUG_POSTFIX "d")
SET(CMAKE_CXX_STANDARD 11)
SET(CMAKE_MODULE_PATH "${PROJECT_SOURCE_DIR};${CMAKE_MODULE_PATH}")

MACRO(NEW_LIBRARY LIBRARY_NAME LIBRARY_TYPE)
    ADD_LIBRARY(${LIBRARY_NAME} ${LIBRARY_TYPE} ${LIBRARY_FILES})

    SET_TARGET_PROPERTIES(${LIBRARY_NAME} PROPERTIES DEBUG_POSTFIX "${CMAKE_DEBUG_POSTFIX}")
    SET_TARGET_PROPERTIES(${LIBRARY_NAME} PROPERTIES ARCHIVE_OUTPUT_DIRECTORY_DEBUG "${PROJECT_BINARY_DIR}/lib"
                                                     ARCHIVE_OUTPUT_DIRECTORY_RELEASE "${PROJECT_BINARY_DIR}/lib"
                                                     LIBRARY_OUTPUT_DIRECTORY_DEBUG "${PROJECT_BINARY_DIR}/lib"
                                                     LIBRARY_OUTPUT_DIRECTORY_RELEASE "${PROJECT_BINARY_DIR}/lib"
                                                     RUNTIME_OUTPUT_DIRECTORY_DEBUG "${PROJECT_BINARY_DIR}/bin"
                                                     RUNTIME_OUTPUT_DIRECTORY_RELEASE "${PROJECT_BINARY_DIR}/bin")

    INSTALL(TARGETS ${LIBRARY_NAME} RUNTIME DESTINATION ${CMAKE_INSTALL_PREFIX}/bin
                                    LIBRARY DESTINATION ${CMAKE_INSTALL_PREFIX}/lib
                                    ARCHIVE DESTINATION ${CMAKE_INSTALL_PREFIX}/lib)

    TARGET_INCLUDE_DIRECTORIES(${LIBRARY_NAME} PUBLIC ${EXTERNAL_INCLUDES})
    TARGET_LINK_LIBRARIES(${LIBRARY_NAME} ${EXTERNAL_LIBRARIES})
ENDMACRO(NEW_LIBRARY)

IF(NOT WIN32)
    SET(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -pedantic")
    SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -frtti -std=c++11")
    SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wno-deprecated-register")
ENDIF(NOT WIN32)

IF(MSVC)
    SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /MP /bigobj")
    SET(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /SAFESEH:NO")
ENDIF(MSVC)

FIND_PACKAGE(OpenGL)
INCLUDE_DIRECTORIES(. ${OPENGL_INCLUDE_DIR})
ADD_DEFINITIONS(-DBUILD_UNITY_PLUGIN)

FIND_PATH(OSG_INCLUDE_DIR osg/Referenced
    PATHS
    $ENV{OSG_ROOT}/include
    $ENV{OSG_DIR}/include
    /usr/include
    /usr/local/include
)

FIND_PATH(OSG_LIB_DIR libosg.so osg.lib
    PATHS
    $ENV{OSG_ROOT}/lib
    $ENV{OSG_DIR}/lib
    /usr/lib
    /usr/local/lib
)

FIND_PATH(OSG_3RDPARTY_LIB_DIR libjpeg.lib
    PATHS
    $ENV{OSG_ROOT}/lib
    $ENV{OSG_DIR}/lib
    /usr/lib
    /usr/local/lib
)

MACRO(LINK_OSG_LIBRARY)
    FOREACH(OSG_LIB_NAME ${ARGN})  
        TARGET_LINK_LIBRARIES(${LIB_NAME} debug ${OSG_PREFIX}${OSG_LIB_NAME}d
                                      optimized ${OSG_PREFIX}${OSG_LIB_NAME})  
    ENDFOREACH()
ENDMACRO(LINK_OSG_LIBRARY)

MACRO(LINK_OSG_PLUGIN)
    FOREACH(PLUGIN_NAME ${ARGN})  
        TARGET_LINK_LIBRARIES(${LIB_NAME} debug ${OSG_PLUGINS_PREFIX}osgdb_${PLUGIN_NAME}d
                                      optimized ${OSG_PLUGINS_PREFIX}osgdb_${PLUGIN_NAME})  
    ENDFOREACH()
ENDMACRO(LINK_OSG_PLUGIN)

MACRO(LINK_OSG_PLUGIN_DEP)
    FOREACH(DEP_NAME ${ARGN})  
        TARGET_LINK_LIBRARIES(${LIB_NAME} ${DEP_NAME})  
    ENDFOREACH()
ENDMACRO(LINK_OSG_PLUGIN_DEP)

IF(OSG_INCLUDE_DIR AND OSG_LIB_DIR)
    SET(LIB_NAME Mana_OpenSceneGraphLoader)
    SET(LIBRARY_FILES
        InterfaceBase.h
        Export.h
        Export.cpp
        UpdateDatabasePager.h
        UpdateDatabasePager.cpp
		Intersector.h
		Intersector.cpp
    )
    SET(LIBRARY_HEADERS Export.h)

    INCLUDE_DIRECTORIES(${OSG_INCLUDE_DIR})
    LINK_DIRECTORIES(${OSG_LIB_DIR} ${OSG_3RDPARTY_LIB_DIR})
    NEW_LIBRARY(${LIB_NAME} SHARED)
    
	OPTION(OSG_USE_STATIC_BUILD "Use static OpenSceneGraph" OFF)
	IF (OSG_USE_STATIC_BUILD)
		ADD_DEFINITIONS(-DOSG_LIBRARY_STATIC)
	ENDIF()

    IF (WIN32)
        IF (OSG_USE_STATIC_BUILD)
			SET(OSG_PREFIX "osg160-" CACHE STRING "The prefix of OSG libraries")
			SET(OSG_PLUGINS_PREFIX "osgPlugins-3.7.0/" CACHE STRING "The prefix of OSG plugins")
			SET(OSG_OT_PREFIX "ot21-" CACHE STRING "The prefix of OpenThreads libraries")
	    ELSE()
			SET(OSG_PREFIX "")
			SET(OSG_PLUGINS_PREFIX "")
			SET(OSG_OT_PREFIX "")
		ENDIF()
        
        TARGET_LINK_LIBRARIES(${LIB_NAME} ${OPENGL_gl_LIBRARY} debug ${OSG_OT_PREFIX}OpenThreadsd
                                                            optimized ${OSG_OT_PREFIX}OpenThreads)
        LINK_OSG_LIBRARY(osg osgDB osgUtil osgGA osgText osgSim osgTerrain osgViewer)
        IF (OSG_USE_STATIC_BUILD)
			LINK_OSG_PLUGIN(3ds bmp dds gif ive jpeg obj openflight osg png rgb shp stl tga txp vtf
							deprecated_osg deprecated_osgsim deprecated_osgterrain
							serializers_osg serializers_osgsim serializers_osgterrain)
			LINK_OSG_PLUGIN_DEP(libjpeg libpng libungif zlib)
		ENDIF()
    ENDIF(WIN32)

    IF(APPLE)
        #TARGET_LINK_LIBRARIES(${LIB_NAME} ...)
    ENDIF(APPLE)

    TARGET_LINK_LIBRARIES(${LIB_NAME} ${OPENGL_gl_LIBRARY})
    TARGET_COMPILE_OPTIONS(${LIB_NAME} PUBLIC -D_SCL_SECURE_NO_WARNINGS)
    IF(APPLE)
        TARGET_LINK_LIBRARIES(${LIB_NAME} objc)
    ENDIF(APPLE)
ENDIF(OSG_INCLUDE_DIR AND OSG_LIB_DIR)
