#ifndef DYNAMIC_LIB_H
#define DYNAMIC_LIB_H

#include <string>

#ifdef Dynamic_EXPORTS
    #define DYNAMIC_API __declspec(dllexport)
#else
    #define DYNAMIC_API __declspec(dllimport)
#endif

DYNAMIC_API std::string GetDynamicMessage();

#endif // DYNAMIC_LIB_H
