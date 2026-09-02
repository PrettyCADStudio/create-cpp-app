project(App)
define_executable()
target_include_directories(App PRIVATE ${MY_INC_DIR})
link_internal_projects(Static Dynamic)
