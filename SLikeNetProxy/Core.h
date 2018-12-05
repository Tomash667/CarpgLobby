#pragma once

#include <cassert>
#include <vector>
#include <string>

typedef unsigned int uint;
typedef const char* cstring;

using std::vector;
using std::string;

cstring Format(cstring fmt, ...);
