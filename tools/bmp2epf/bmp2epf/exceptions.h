#pragma once
#include <exception>
#include <string>



class usage_exception: public std::exception
{
public:
	usage_exception(const char* v) : std::exception(v) {};
	usage_exception(std::string v) : std::exception(v.c_str()) {};
};


