#pragma once
#include <vector>
#include "EfpFile.h"



struct BmpData
{
	uint16_t m_width;
	uint16_t m_height;
	std::vector<Pixel> m_data;
};


int bmpextract(std::string fileName, BmpData* out);




