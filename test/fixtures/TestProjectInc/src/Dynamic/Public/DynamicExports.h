#pragma once

#ifdef Dynamic_EXPORTS
    #define DYNAMIC_API __declspec(dllexport)
#else
    #define DYNAMIC_API __declspec(dllimport)
#endif
