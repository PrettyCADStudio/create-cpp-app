add_executable(App main.cpp)
include_directories(${MY_INC_DIR})
target_link_libraries(App PRIVATE Static)
target_link_libraries(App PRIVATE Dynamic)
