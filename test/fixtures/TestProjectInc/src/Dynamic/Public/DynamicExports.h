#pragma once

/* Cross-platform export macro */
#if defined(_WIN32) || defined(__CYGWIN__)
  #ifdef Dynamic_EXPORTS
    #define DYNAMIC_API __declspec(dllexport)
  #else
    #define DYNAMIC_API __declspec(dllimport)
  #endif
#elif defined(__GNUC__) && __GNUC__ >= 4
  #define DYNAMIC_API __attribute__ ((visibility ("default")))
#else
  #define DYNAMIC_API
#endif
