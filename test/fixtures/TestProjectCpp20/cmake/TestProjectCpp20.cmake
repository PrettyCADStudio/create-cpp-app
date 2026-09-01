# TestProjectCpp20 cmake functions

# Add every child project below MY_SRC_DIR.
# Usage: call add_projects() once from the root CMakeLists.txt after including this file.
# Each child directory must contain CMakeLists.txt, define a target named after its directory, and be independent of scan order.
# CONFIGURE_DEPENDS makes CMake rescan when projects are added or removed; rerun CMake if your generator does not support it.
function(add_projects)
    set(installable_projects)
    file(GLOB_RECURSE cmake_lists RELATIVE "${MY_SRC_DIR}" CONFIGURE_DEPENDS "${MY_SRC_DIR}/*/CMakeLists.txt")
    foreach(cmake_list IN LISTS cmake_lists)
        get_filename_component(project_directory "${cmake_list}" DIRECTORY)
        if(NOT project_directory STREQUAL ".")
            add_subdirectory("${MY_SRC_DIR}/${project_directory}")
            get_filename_component(project_name "${project_directory}" NAME)
            get_filename_component(project_folder "${project_directory}" DIRECTORY)
            if(TARGET "${project_name}")
                set_property(TARGET "${project_name}" PROPERTY FOLDER "${project_folder}")
                get_target_property(project_type "${project_name}" TYPE)
                if(project_type STREQUAL "EXECUTABLE" OR project_type STREQUAL "STATIC_LIBRARY" OR project_type STREQUAL "SHARED_LIBRARY" OR project_type STREQUAL "MODULE_LIBRARY")
                    list(APPEND installable_projects "${project_name}")
                endif()
            endif()
        endif()
    endforeach()
    set_property(GLOBAL PROPERTY PROJECT_INSTALLABLE_TARGETS "${installable_projects}")
endfunction()

# Install all executable and library targets discovered by add_projects.
# Usage: call install_projects() after add_projects() in the root CMakeLists.txt.
# Executables and shared libraries go to bin/; static and import libraries go to lib/ under CMAKE_INSTALL_PREFIX.
function(install_projects)
    get_property(installable_projects GLOBAL PROPERTY PROJECT_INSTALLABLE_TARGETS)
    if(installable_projects)
        install(TARGETS ${installable_projects}
            RUNTIME DESTINATION bin
            LIBRARY DESTINATION lib
            ARCHIVE DESTINATION lib)
    endif()
endfunction()

# Collect C/C++ source and header files recursively.
# Usage: search_project_files(<project-directory> <output-variable>).
# The output variable is set in the caller's scope. CMake reconfigures when matching files change.
function(search_project_files project_directory project_files)
    file(GLOB_RECURSE files CONFIGURE_DEPENDS
        "${project_directory}/*.c"
        "${project_directory}/*.cc"
        "${project_directory}/*.cpp"
        "${project_directory}/*.cxx"
        "${project_directory}/*.h"
        "${project_directory}/*.hh"
        "${project_directory}/*.hpp"
        "${project_directory}/*.hxx")
    set(${project_files} "${files}" PARENT_SCOPE)
endfunction()

# Put files in IDE source groups that mirror their directories.
# Usage: group_project_files(<project-directory> <file>...). This only affects IDE presentation.
function(group_project_files project_directory)
    foreach(project_file IN LISTS ARGN)
        get_filename_component(project_file_directory "${project_file}" DIRECTORY)
        file(RELATIVE_PATH project_filter "${project_directory}" "${project_file_directory}")
        source_group("${project_filter}" FILES "${project_file}")
    endforeach()
endfunction()

# Configure conventional Public/ and Private/ include directories for a target.
# Usage: include_project_directories(<target> <project-directory>).
# Headers in Public/ are exposed to consumers; Private/ is used only while compiling this target.
function(include_project_directories target project_directory)
    if(EXISTS "${project_directory}/Public")
        target_include_directories(${target} PUBLIC "${project_directory}/Public")
    endif()
    if(EXISTS "${project_directory}/Private")
        target_include_directories(${target} PRIVATE "${project_directory}" "${project_directory}/Private")
    endif()
endfunction()

# Define an executable from files in the current project directory.
# Usage in a child CMakeLists.txt: project(MyApp) followed by define_executable().
# Call link_internal_projects(...) afterwards to link targets defined elsewhere in this solution.
function(define_executable)
    search_project_files("${CMAKE_CURRENT_SOURCE_DIR}" project_files)
    add_executable(${PROJECT_NAME} ${project_files})
    group_project_files("${CMAKE_CURRENT_SOURCE_DIR}" ${project_files})
    include_project_directories(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}")
endfunction()

# Define a static library from files in the current project directory.
# Usage in a child CMakeLists.txt: project(MyLibrary) followed by define_static_library().
# Public/ headers become part of the library's public include interface.
function(define_static_library)
    search_project_files("${CMAKE_CURRENT_SOURCE_DIR}" project_files)
    add_library(${PROJECT_NAME} STATIC ${project_files})
    group_project_files("${CMAKE_CURRENT_SOURCE_DIR}" ${project_files})
    include_project_directories(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}")
endfunction()

# Define a shared library from files in the current project directory.
# Usage in a child CMakeLists.txt: project(MyLibrary) followed by define_shared_library().
# Exported symbols still need platform-appropriate export macros in public headers.
function(define_shared_library)
    search_project_files("${CMAKE_CURRENT_SOURCE_DIR}" project_files)
    add_library(${PROJECT_NAME} SHARED ${project_files})
    group_project_files("${CMAKE_CURRENT_SOURCE_DIR}" ${project_files})
    include_project_directories(${PROJECT_NAME} "${CMAKE_CURRENT_SOURCE_DIR}")
endfunction()

# Link targets from this CMake solution to the current project.
# Usage: link_internal_projects(TargetA TargetB ...). Targets are linked with PUBLIC visibility.
# Use target_link_libraries directly when PRIVATE or INTERFACE visibility is required.
function(link_internal_projects)
    target_link_libraries(${PROJECT_NAME} PUBLIC ${ARGN})
endfunction()
