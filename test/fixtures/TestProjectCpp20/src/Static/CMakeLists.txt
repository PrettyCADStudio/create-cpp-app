add_library(Static STATIC Private/StaticLib.cpp)
target_include_directories(Static PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/Public)
