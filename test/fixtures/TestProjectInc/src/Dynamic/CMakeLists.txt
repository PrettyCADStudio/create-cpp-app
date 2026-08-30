add_library(Dynamic SHARED Private/DynamicLib.cpp)
target_include_directories(Dynamic PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/Public)
