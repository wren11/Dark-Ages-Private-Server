#pragma once
#include <vector>
#include <stdint.h>
#include <map>
#include <set>
#include "GdiFIle.h"


struct Rect
{
	uint16_t left;
	uint16_t right;
	uint16_t top;
	uint16_t bottom;

	inline uint32_t height() const { return bottom - top; }
	inline uint32_t width() const { return right - left; }
	inline uint32_t square() const { return height() * width(); }

};


void load_pallete(const std::vector<uint8_t> &palfile,  std::vector<Pixel> *pallete);
void load_pallete(std::string filename, std::vector<Pixel> *pallete);
void save_pallete_to_file(std::string filename, const std::vector<Pixel> &pallete);


typedef std::vector<std::vector<Pixel>> FrameData;

class EpfFile
{
public:
	EpfFile();
	bool create_raw_data(std::vector<uint8_t>* out);

	void pack_frame(size_t i);

	void set_size(uint16_t width, uint16_t height);
	void append_frame(const Rect& pos, const BmpData &data);
	void create_pallete_from_frame_data();
	void create_tbl_data();
	void set_pallete(const std::vector<Pixel>& pallete);
	void get_pallete(std::vector<Pixel>* pallete);
	void get_tbl_data(std::vector<uint8_t> *tbl);
	void set_transparent(const Pixel& transp);

protected:

	std::vector<Rect> m_frame_coords;
	std::vector<BmpData> m_frames;
	std::vector<uint8_t> m_raw_data;
	std::map<Pixel, uint8_t> m_pixel_index;
	std::vector<Pixel> m_pallete;
	std::vector<uint16_t> m_tbl;
	Pixel m_transparent;
	bool m_is_transparent;

	template <class T>
	void push(const T& var)
	{
		m_raw_data.resize(m_raw_data.size() + sizeof(var));
		memcpy(m_raw_data.data() + m_raw_data.size() - sizeof(var), &var, sizeof(var));
	}

	void create_pixel_index();
	uint16_t m_min_width;
	uint16_t m_min_height;
	uint16_t m_width;
	uint16_t m_height;
public:
	static int test();
};
