#include "EfpFile.h"
#include "BitStack.h"
#include <assert.h>
#include <stdio.h>
#include <algorithm>
#include "exceptions.h"


EpfFile::EpfFile():m_min_width(0),m_min_height(0),
m_width(0),m_height(0), m_is_transparent(false), m_transparent(255,255,255)
{
}

bool EpfFile::create_raw_data(std::vector<uint8_t>* out)
{
	if (m_min_width > m_width || m_min_height > m_height)
		return false;

	m_raw_data.clear();
	m_raw_data.reserve(0x10000);

	if (m_pixel_index.size() == 0)
		create_pallete_from_frame_data();

	push(uint16_t(m_frames.size()));

	push(m_width);   
	push(m_height);  
	push(uint16_t(0));

	std::vector<uint32_t> pixel_data_offset;
	std::vector<uint32_t> stencil_data_offset;

	uint32_t offs = 0;
	for (Rect& r : m_frame_coords)
	{
		uint32_t square = r.height() * r.width();
		pixel_data_offset.push_back(offs);
		offs += square;
		stencil_data_offset.push_back(offs);
		offs += (square+3) / 4; //TODO
	}
	push(offs); //pixel_data_length

	for (size_t i = 0; i < m_frames.size(); ++i)
	{
		assert(12 + pixel_data_offset[i] == m_raw_data.size());
		pack_frame(i);
		assert(12 + stencil_data_offset[i] == m_raw_data.size());
	
		BitStack b;

		size_t sq = m_frame_coords[i].square();
		for (size_t i=0; i < sq; ++i )
		{
			b.push(false); //TODO
			b.push(false); //TODO
		}
		b.flush();
		m_raw_data.insert(m_raw_data.end(), b.data().begin(), b.data().end()); // stencill data
	}

	for (size_t i = 0; i < m_frame_coords.size(); ++i)
	{
		push(m_frame_coords[i].top);
		push(m_frame_coords[i].left);
		push(m_frame_coords[i].bottom);
		push(m_frame_coords[i].right);
		push(pixel_data_offset[i]);
		push(stencil_data_offset[i]);
	}

	
	//terminator frame
	push(uint16_t(0));
	push(uint16_t(0));
	push(uint16_t(0));
	push(uint16_t(0));
	push(offs);
	push(uint32_t(0));
	

	create_tbl_data();

	*out = m_raw_data;
	return true;
}

void EpfFile::pack_frame(size_t i)
{
	BmpData& frame = m_frames[i];
	for (int y = m_frame_coords[i].top; y < m_frame_coords[i].bottom; ++y)
	{
		for (int x = m_frame_coords[i].left; x < m_frame_coords[i].right; ++x)
		{
			Pixel pixel = frame.pixel(x, y);
			if ((m_is_transparent && m_transparent == pixel) || pixel.transparent)
			{
				push(uint8_t(0));
				continue;
			}

			auto itr = m_pixel_index.find(pixel);
			if (itr == m_pixel_index.end())
				throw std::exception("There is no color in the pallete");
			push(itr->second);

		}
	}
}

void EpfFile::set_size(uint16_t width, uint16_t height)
{
	m_width = width;
	m_height = height;
}

void EpfFile::append_frame(const Rect& pos, const BmpData &data)
{
	m_min_width = std::max(m_min_width,pos.right);
	m_min_height = std::max(m_min_height, pos.bottom);

	if (pos.right > data.width())
	{
		char t[256] = "";
		sprintf(t, "Wrong frame coordinates: frame right %d image width %d", int(pos.right), int(data.width()));
		throw usage_exception(t);
	}

	if (pos.bottom > data.height())
	{
		char t[256] = "";
		sprintf(t, "Wrong frame coordinates: frame bottom %d image height %d", int(pos.bottom), int(data.height()));
		throw usage_exception(t);
	}
	
	m_frame_coords.push_back(pos);
	m_frames.push_back(data);
}

void EpfFile::create_pixel_index()
{
	for (size_t i = 1; i < m_pallete.size(); ++i)
	{
		Pixel p = m_pallete[i];
		if (m_pixel_index.find(p) == m_pixel_index.end() )
			m_pixel_index[p] = static_cast<uint8_t>(i);
	}
}

void EpfFile::create_pallete_from_frame_data()
{
	std::set<Pixel> new_pallete;
	m_pixel_index.clear();

	for (BmpData& fr : m_frames)
	{
		for (size_t i = 0; i < fr.size(); ++i)
			new_pallete.insert(fr.pixel(i));
	}
	m_pallete.clear();
	m_pallete.push_back(Pixel(0, 0, 0));
	m_pallete.insert(m_pallete.end(), new_pallete.begin(), new_pallete.end());

	if (m_pallete.size() > 256)
		throw std::exception("pallete size is bigger than 256 colors");


	create_pixel_index();
	
	m_pallete.resize(256, Pixel(255, 255, 255));
}

void EpfFile::create_tbl_data()
{
	m_tbl.clear();
	for (size_t i = 0; i < m_frames.size(); ++i)
	{
		m_tbl.push_back(0x37); //TODO
		m_tbl.push_back(0x46); //TODO
	}
		
}

void EpfFile::set_pallete(const std::vector<Pixel>& pallete)
{
	m_pallete = pallete;
	create_pixel_index();
}

void EpfFile::get_pallete(std::vector<Pixel>* pallete)
{
	*pallete = m_pallete;
}


void EpfFile::get_tbl_data(std::vector<uint8_t> *tbl)
{
	tbl->resize(m_tbl.size() * 2);
	memcpy(tbl->data(), m_tbl.data(), tbl->size());
}

void EpfFile::set_transparent(const Pixel& transp)
{
	m_transparent = transp; 
	m_is_transparent = true;
}

int EpfFile::test()
{
// 	EpfFile epf;
// 	epf.set_size(500, 500);
// 
// 	for (int i = 0; i < 3; ++i)
// 	{
// 		Rect r = { uint16_t(i*100),uint16_t(i * 100+ 100),uint16_t(i * 100),uint16_t(i * 100 + 100) };
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
// 				fd[num++] = (sqrt(s1 + s2) < (i*10+10)) ? green : black;
// 			}
// 		}
// 		epf.append_frame(r, fd);
// 	}
// 
// 	std::vector<Pixel> pallete;
// 	load_pallete("eff000.pal", &pallete);
// 
// 	epf.set_pallete(pallete);
// 
// 	std::vector<uint8_t> out_data;
// 	assert(epf.create_raw_data(&out_data));
// 
// 	FILE* f = fopen("test.epf", "wb");
// 	assert(f);
// 	fwrite(out_data.data(), out_data.size(), 1, f);
// 	fclose(f);
 	return 0;
}

bool Pixel::operator<(const Pixel& p) const
{
	if (transparent != p.transparent)
		return transparent == false;
	if (R != p.R)
		return R < p.R;
	if (G != p.G)
		return G < p.G;
	return B < p.B;
}

bool Pixel::operator==(const Pixel& p) const
{
	return R == p.R && G == p.G && B == p.B && transparent == p.transparent;
}

#ifdef _DEBUG
//const int EpfFile_test = EpfFile::test();
#endif

void load_pallete(const std::vector<uint8_t> &palfile, std::vector<Pixel> *pallete)
{
	if (palfile.size() == 0x418)
	{
		for (size_t i = 0x18; i < palfile.size(); i += 4)
			pallete->push_back(Pixel(palfile[i], palfile[i + 1], palfile[i + 2]));
		return;
	}

	assert(!(palfile.size() % 3));
	for (size_t i = 0; i < palfile.size(); i += 3)
		pallete->push_back(Pixel(palfile[i], palfile[i + 1], palfile[i + 2]));
}

void load_pallete(std::string filename, std::vector<Pixel> *pallete)
{

	FILE* palf = fopen(filename.c_str(), "rb");
	if (!palf)
		throw std::exception((std::string("Cant open pallete file ") + filename).c_str());

	struct stat s;
	fstat(_fileno(palf), &s);
	std::vector<uint8_t> pal_data(s.st_size);
	fread(pal_data.data(), s.st_size, 1, palf);
	fclose(palf);
	load_pallete(pal_data, pallete);
	
}

void save_pallete_to_file(std::string filename, const std::vector<Pixel> &pallete)
{
	FILE* palf = fopen(filename.c_str(), "wb");
	if (!palf)
		throw std::exception((std::string("Cant open ") + filename + " to save pallete").c_str());
	assert(pallete.size() == 256);

	for (Pixel p : pallete)
	{
		fwrite(&p.R, sizeof(p.R), 1, palf);
		fwrite(&p.G, sizeof(p.R), 1, palf);
		fwrite(&p.B, sizeof(p.R), 1, palf);
	}
	fclose(palf);
}

