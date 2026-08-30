#include <iostream>
#include "StaticLib.h"
#include "DynamicLib.h"

int main() {
    std::cout << "Hello, World!" << std::endl;
    std::cout << GetStaticMessage() << std::endl;
    std::cout << GetDynamicMessage() << std::endl;
    return 0;
}
