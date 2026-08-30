add_library(Static STATIC StaticLib.cpp)
target_include_directories(Static PUBLIC ${MY_INC_DIR})
