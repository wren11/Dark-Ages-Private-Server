#pragma once
#include <string>
#include <vector>

struct Pixel
{
	uint8_t R;
	uint8_t G;
	uint8_t B;
	bool transparent;

	Pixel() :R(0), G(0), B(0) {}
	Pixel(uint8_t red, uint8_t green, uint8_t blue, bool transp = false) :
		R(red), G(green), B(blue), transparent(transp) {}

	bool operator == (const Pixel& p) const;
	bool operator < (const Pixel& p) const;
};


class BmpData 
{
	std::vector<std::vector<Pixel>> m_data;
	size_t m_height;
	size_t m_width;
	size_t m_size;
public:
	BmpData();
	BmpData(size_t height, size_t width);
	void reset(size_t height, size_t width);
	inline size_t height() const { return m_height; };
	inline size_t width() const { return m_width; };
	const Pixel& pixel(size_t x, size_t y) const;
	const Pixel& pixel(size_t num) const;
	void set_pixel(size_t x, size_t y, const Pixel& pixel);
	inline size_t size() const { return m_size; }
};


void gdi_extract(std::wstring fileName, BmpData* outa);