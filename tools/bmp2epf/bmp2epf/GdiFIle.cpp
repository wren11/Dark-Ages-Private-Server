#include <iostream>
#include <windows.h>
#include <gdiplus.h>
#include "GdiFIle.h"
using namespace Gdiplus;
using namespace std;




void gdi_extract(std::wstring fname, BmpData* outa)
{
	GdiplusStartupInput gpStartupInput;
	ULONG_PTR gpToken;
	GdiplusStartup(&gpToken, &gpStartupInput, NULL);
	HBITMAP result = NULL;

	Gdiplus::Bitmap* bitmap = Gdiplus::Bitmap::FromFile(fname.c_str(), false);

	if (!bitmap)
		throw exception("cant open input file");
	
	int width = bitmap->GetWidth();
	int height = bitmap->GetHeight();

	if (height > 0xFFFF || width > 0xFFFF)
		throw std::exception("Too big image");

	if (height == 0 || width  == 0)
		throw std::exception("cant open image file");

	outa->reset(height,width);

	Color pixelColor;
	for (int y = 0; y < height; y++) 
	{
		for (int x = 0; x < width; x++) 
		{
			bitmap->GetPixel(x, y, &pixelColor);
			bool transp = pixelColor.GetAlpha() == 0;
			outa->set_pixel(x,y, Pixel(pixelColor.GetRed(), pixelColor.GetGreen(), pixelColor.GetBlue(), transp));
		}
	}

}

BmpData::BmpData(size_t height, size_t width)
{
	reset(height, width);
}

BmpData::BmpData()
{

}

void BmpData::reset(size_t height, size_t width)
{
	std::vector<Pixel> row(width);
	m_data.clear();
	m_data.resize(height, row);
	m_height = height;
	m_width = width;
	m_size = height * width;
}

// int gdi_test()
// {
// 	BmpData d;
// 	gdi_extract("example.bmp", &d);
// 	return 0;
// }
// 
// const int asfdas = gdi_test();


const Pixel& BmpData::pixel(size_t x, size_t y) const
{
	return m_data[y][x];
}

const Pixel& BmpData::pixel(size_t num) const
{
	size_t row = num / width();
	size_t col = num % width();
	return m_data[row][col];
}

void BmpData::set_pixel(size_t x, size_t y, const Pixel& pixel)
{
	m_data[y][x] = pixel;
}
