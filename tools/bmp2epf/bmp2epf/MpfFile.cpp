#include "MpfFile.h"
#include <assert.h>

MpfFile::MpfFile():m_is_indices2(false), m_pallete_number(0)
{
	memset(&m_frame_indeces, 0, sizeof(m_frame_indeces));
	memset(&m_frame_indeces2, 0, sizeof(m_frame_indeces2));
}

void MpfFile::create_raw_data(std::vector<uint8_t>* out)
{
	m_raw_data.clear();
	m_raw_data.reserve(0x10000);

	if (m_pixel_index.size() == 0)
		create_pallete_from_frame_data();

	push(uint8_t(m_frames.size() + (m_pallete_number?1:0)));
	push(uint16_t(m_width));
	push(uint16_t(m_height)); 

	uint32_t offs = 0;
	std::vector<uint32_t> pixel_data_offset;
	for (Rect& r : m_frame_coords)
	{
		uint32_t square = r.height() * r.width();
		pixel_data_offset.push_back(offs);
		offs += square;
	}
	push(offs); //pixel_data_length
	if (m_is_indices2)
		push(m_frame_indeces2);
	else
		push(m_frame_indeces);

	for (size_t i = 0; i < m_frames.size(); ++i)
	{
		push(m_frame_coords[i].left);
		push(m_frame_coords[i].top);
		push(m_frame_coords[i].right);
		push(m_frame_coords[i].bottom);
		push(m_xoffset[i]);
		push(m_yoffset[i]);
		push(pixel_data_offset[i]);
	}
	if (m_pallete_number)
	{
		for (int i = 0; i < 6; ++i)
			push(uint16_t(0xFFFF));
		push(m_pallete_number);
	}

	int data_start = m_raw_data.size();
	for (size_t i = 0; i < m_frames.size(); ++i)
	{
		assert(data_start + pixel_data_offset[i] == m_raw_data.size());
		pack_frame(i);
	}
	*out = m_raw_data;
}

void MpfFile::append_frame(const Rect& pos, uint16_t x_offs, uint16_t y_offs, const BmpData &data)
{
	EpfFile::append_frame(pos,data);
	m_xoffset.push_back(x_offs);
	m_yoffset.push_back(y_offs);
}

int MpfFile::test()
{
// 	MpfFile mpf;
// 	mpf.set_size(500, 500);
// 
// 	for (uint16_t i = 0; i < 3; ++i)
// 	{
// 		Rect r = { i * 100,i * 100 + 100,i * 100,i * 100 + 100 };
// 
// 		int h = r.height();
// 		int w = r.width();
// 		int xmid = w / 2;
// 		int ymid = h / 2;
// 
// 		FrameData fd;
// 		fd.resize(h*w);
// 
// 		Pixel black(0, 0, 0);
// 		Pixel green(0, 255, 0);
// 
// 		int num = 0;
// 		for (int x = 0; x < w; ++x)
// 		{
// 			for (int y = 0; y < h; ++y)
// 			{
// 				int s1 = (x - xmid)*(x - xmid);
// 				int s2 = (y - ymid)*(y - ymid);
// 				fd[num++] = (sqrt(s1 + s2) < (i * 10 + 10)) ? green : black;
// 			}
// 		}
// 		mpf.append_frame(r, 0, 0,fd);
// 	}
// 
// 	std::vector<Pixel> pallete;
// 	load_pallete("mns001.pal", &pallete);
// 
// 	mpf.set_pallete(pallete);
// 
// 	std::vector<uint8_t> out_data;
// 	mpf.create_raw_data(&out_data);
// 
// 	FILE* f = fopen("test.mpf", "wb");
// 	assert(f);
// 	fwrite(out_data.data(), out_data.size(), 1, f);
// 	fclose(f);
// 	return 0;
	return 0;
}

// #ifdef _DEBUG
// const int yyy = MpfFile::test();
// #endif