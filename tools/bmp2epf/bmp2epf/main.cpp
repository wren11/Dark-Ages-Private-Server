#include <assert.h>
#include "arguments.h"
#include <iostream>
#include "exceptions.h"
#include "GdiFIle.h"
#include <algorithm>
#include <filesystem>
#include "MpfFile.h"
#include <locale>


//examples of usage
//--frame *.png --frame_indices 0:2:4:2:4:2:4:2:0:2:2:2 --pallete mns010.pal  --pal_num 10 --transparent 0:0:0 MNS001.MPF
//--frame *.png --frame_indices 0:2:4:2:4:2:4:2:0:2:2:2 --pal_num 129 --transparent 0:0:0 MNS001.MPF



using namespace std;

void test1()
{
	BmpData bmp_data;
	gdi_extract(L"example_24bit.bmp", &bmp_data);

	EpfFile epf;
	epf.set_size(500, 500);

	Rect r = { 0, uint16_t( bmp_data.width()), 0,  uint16_t( bmp_data.height()) };
	epf.append_frame(r, bmp_data);

	std::vector<Pixel> pallete;
	load_pallete("eff000.pal", &pallete);

	epf.set_pallete(pallete);

	std::vector<uint8_t> out_data;
	assert(epf.create_raw_data(&out_data));

	FILE* f = fopen("example.epf", "wb");
	assert(f);
	fwrite(out_data.data(), out_data.size(), 1, f);
	fclose(f);
}


void get_filenames(const Arguments &args, vector<std::wstring> *filenames)
{
	int min_width = 0;
	int min_height = 0;

	for (size_t i = 0; i < args.input_count(); ++i)
	{

		string h = args.filename(i);
		if (h.size() && h[0] == '*')
		{
			wstring ext_template(h.begin() + 1, h.end());

			string path = ".";
			vector<std::wstring> fnames;

			for (auto & p : experimental::filesystem::v1::directory_iterator(path))
			{
				experimental::filesystem::v1::directory_entry entry = p;
				wstring fname = entry.path().filename().c_str();
				wstring ext = entry.path().extension().c_str();

				if (fname.substr(fname.size() - ext_template.size()) == ext_template)
					fnames.push_back(fname);
			}
			sort(fnames.begin(), fnames.end());
			filenames->insert(filenames->end(), fnames.begin(), fnames.end());
		}
		else
		{
			wstring fn(h.begin(), h.end());
			filenames->push_back(fn);
		}
	}
}


void process(const Arguments &args)
{
	
//	bmpextract(args.filename(), &bmp_data);

	uint16_t min_width = 0;
	uint16_t min_height = 0;

	vector<std::wstring> filenames;
	get_filenames(args, &filenames);

	EpfFile epf;


	for (size_t i = 0; i < filenames.size(); ++i)
	{
		std::wstring fname = filenames[i];
		BmpData bmp_data;
		gdi_extract(fname, &bmp_data);
		Rect r = { 0, uint16_t(bmp_data.width()) , 0, uint16_t(bmp_data.height())};

		const frame_coordinates* c = NULL;

		if (args.frame_coord().size() > i)
			c = &args.frame_coord()[i];
		else if (args.common_coord().size())
			c = &args.common_coord();

		if (c)
		{
			r.left = atoi(c->at(0).c_str());
			r.top = atoi(c->at(1).c_str());
			r.right = atoi(c->at(2).c_str());
			r.bottom = atoi(c->at(3).c_str());
		}

		epf.append_frame(r, bmp_data);
		min_width = max(min_width, r.right);
		min_height = max(min_height, r.bottom);
	}

	if (!args.height().empty() && !args.width().empty())
	{
		uint32_t width = atoi(args.width().c_str());
		uint32_t height = atoi(args.height().c_str());
		if (height > 0xFFFF)
			throw usage_exception("Invalid height");
		if (width > 0xFFFF)
			throw usage_exception("Invalid width");

		if (height < min_height)
			throw usage_exception("Image height should be not less than bottom side of any frame");

		if (width < min_width)
			throw usage_exception("Image height should be not less than bottom side of any frame");

		epf.set_size(width, height);
	}
	else
		epf.set_size(min_width, min_height);
	
	if (!args.pallete_file().empty())
	{
		std::vector<Pixel> pallete;
		load_pallete(args.pallete_file(), &pallete);
		epf.set_pallete(pallete);
	}

	if (args.transparent_color().size())
	{
		uint32_t r = atoi(args.transparent_color()[0].c_str());
		uint32_t g = atoi(args.transparent_color()[1].c_str());
		uint32_t b = atoi(args.transparent_color()[2].c_str());
		if (r > 255 || g > 255 || b > 255)
			throw usage_exception("Wrong transparent color");

		epf.set_transparent(Pixel(r, g, b));
	}

	std::vector<uint8_t> out_data;
	assert(epf.create_raw_data(&out_data));

	std::string outfile = args.outname();
	if (outfile.empty())
	{
		outfile = args.filename(0);
		outfile.resize(outfile.size() - 3);
		outfile += "epf";
	}

	FILE* f = fopen(outfile.c_str(), "wb");
	assert(f);
	fwrite(out_data.data(), out_data.size(), 1, f);
	fclose(f);

	if (args.pallete_file().empty())
	{
		string palfile = outfile;
		palfile.resize(palfile.size() - 3);
		palfile += "pal";

		vector<Pixel> pallete;
		epf.get_pallete(&pallete);
		save_pallete_to_file(palfile, pallete);
	}


	//tbl file
	vector<uint8_t> tbl_data;
	epf.get_tbl_data(&tbl_data);
	string tbl_name = outfile;
	tbl_name.resize(tbl_name.size() - 3);
	tbl_name += "tbl";

	FILE* tblf = fopen(tbl_name.c_str(), "wb");
	if (!tblf)
		throw exception((string("cant open tbl file ") + tbl_name + "for writing").c_str());
	fwrite(tbl_data.data(), tbl_data.size(), 1, tblf);
	fclose(tblf);
}


void process_mpf(const Arguments &args)
{
	uint16_t min_width = 0;
	uint16_t min_height = 0;

	vector<std::wstring> filenames;
	get_filenames(args, &filenames);

	MpfFile mpf;



	for (size_t i=0;i< filenames.size();++i)
	{
		std::wstring fname = filenames[i];
		BmpData bmp_data;
		gdi_extract(fname, &bmp_data);
		Rect r = { 0, uint16_t(bmp_data.width()), 0,  uint16_t(bmp_data.height()) };

		uint16_t x_offs = 0;
		uint16_t y_offs = 0;
		
		
		const frame_coordinates* c = NULL;

		if (args.frame_coord().size() > i)
			c = &args.frame_coord()[i];
		else if (args.common_coord().size())
			c = &args.common_coord();

		if (c)
		{
			r.left = atoi(c->at(0).c_str());
			r.top = atoi(c->at(1).c_str());
			r.right = atoi(c->at(2).c_str());
			r.bottom = atoi(c->at(3).c_str());
			x_offs = atoi(c->at(4).c_str());
			y_offs = atoi(c->at(5).c_str());
		}

		mpf.append_frame(r, x_offs, y_offs,  bmp_data);
		min_width = max(min_width, r.right);
		min_height = max(min_height, r.bottom);
	}

	if (!args.height().empty() && !args.width().empty())
	{
		uint32_t width = atoi(args.width().c_str());
		uint32_t height = atoi(args.height().c_str());
		if (height > 0xFFFF)
			throw usage_exception("Invalid height");
		if (width > 0xFFFF)
			throw usage_exception("Invalid width");

		if (height < min_height)
			throw usage_exception("Image height should be not less than bottom side of any frame");

		if (width < min_width)
			throw usage_exception("Image height should be not less than bottom side of any frame");

		mpf.set_size(width, height);
	}
	else
		mpf.set_size(min_width, min_height);

	if (!args.pallete_file().empty())
	{
		std::vector<Pixel> pallete;
		load_pallete(args.pallete_file(), &pallete);
		mpf.set_pallete(pallete);
	}

	int ind_sz = args.frame_indices().size();
	if (ind_sz)
	{
		if (ind_sz == 8)
		{
			FrameIndices indices;
			indices.walkFrameIndex = atoi(args.frame_indices()[0].c_str());
			indices.walkFrameCount = atoi(args.frame_indices()[1].c_str());
			indices._attackFrameIndex = atoi(args.frame_indices()[2].c_str());
			indices._attackFrameCount = atoi(args.frame_indices()[3].c_str());
			indices._stopFrameIndex = atoi(args.frame_indices()[4].c_str());
			indices._stopFrameCount = atoi(args.frame_indices()[5].c_str());
			indices._stopMotionFrameCount = atoi(args.frame_indices()[6].c_str());
			indices._stopMotionProbability = atoi(args.frame_indices()[7].c_str());
			mpf.set_frame_indices(indices);
		}
		else if (ind_sz == 12)
		{
			FrameIndices2 indices;
			indices.walkFrameIndex = atoi(args.frame_indices()[0].c_str());
			indices.walkFrameCount = atoi(args.frame_indices()[1].c_str());
			indices.ff_flag = 0xFFFF;
			indices._stopFrameIndex = atoi(args.frame_indices()[8].c_str());
			indices._stopFrameCount = atoi(args.frame_indices()[9].c_str());
			indices._stopMotionFrameCount = atoi(args.frame_indices()[10].c_str());
			indices._stopMotionProbability = atoi(args.frame_indices()[11].c_str());
			indices._attackFrameIndex = atoi(args.frame_indices()[2].c_str());
			indices._attackFrameCount = atoi(args.frame_indices()[3].c_str());
			indices._attack2FrameIndex = atoi(args.frame_indices()[4].c_str());
			indices._attack2FrameCount = atoi(args.frame_indices()[5].c_str());
			indices._attack3FrameIndex = atoi(args.frame_indices()[6].c_str());
			indices._attack3FrameCount = atoi(args.frame_indices()[7].c_str());
			mpf.set_frame_indices2(indices);
		}
		else
			throw std::exception("Unknown indices format");
	}
	else
		throw std::exception("Frame indices should be set for mpf file");

	
	uint32_t pallete_number = 0xFFFFFFFF;
	if (args.pallete_number().size())
	{
		pallete_number = atoi(args.pallete_number().c_str());
		mpf.set_pallete_number(pallete_number);
	}

	if (args.transparent_color().size())
	{
		uint32_t r = atoi(args.transparent_color()[0].c_str());
		uint32_t g = atoi(args.transparent_color()[1].c_str());
		uint32_t b = atoi(args.transparent_color()[2].c_str());
		if (r > 255 || g > 255 || b > 255)
			throw usage_exception("Wrong transparent color");

		mpf.set_transparent(Pixel(r, g, b));
	}

	std::vector<uint8_t> out_data;
	mpf.create_raw_data(&out_data);

	std::string outfile = args.outname();
	if (outfile.empty())
	{
		outfile = args.filename(0);
		outfile.resize(outfile.size() - 3);
		outfile += "MPF";
	}

	FILE* f = fopen(outfile.c_str(), "wb");
	if (!f)
		throw std::exception((string("Cant open ") + outfile + " for writing").c_str());
	fwrite(out_data.data(), out_data.size(), 1, f);
	fclose(f);

	if (args.pallete_file().empty())
	{
		string palfile = "";
		if (outfile.rfind("\\") > -1 || outfile.rfind("/") > -1)
			palfile = outfile.substr(std::max(outfile.rfind("\\"), outfile.rfind("/"))+1);
	
		if (pallete_number == 0xFFFFFFFF)
			usage_exception("pal_num should be set for mpf if pallete_file is not set");

		char txt[300] = "";
		sprintf(txt, "mns%03d.pal",pallete_number);
		palfile += txt;

		vector<Pixel> pallete;
		mpf.get_pallete(&pallete);
		save_pallete_to_file(palfile, pallete);
	}
}


string file_upper_ext(string fname)
{
	std::locale loc;
	fname.rfind(".");
	string ext = fname.substr(fname.rfind(".")+1);

	for (char& ch : ext)
		ch = std::toupper(ch,loc);
	return ext;
}


int main(int argc, const char** argv )
{
	
	try
	{
		Arguments args;
		args.parse(argv, argc);

		if (args.help())
		{
			args.print_usage();
			exit(0);
		}

		if (args.input_count() == 0)
			throw usage_exception("Input filename is not set");

		if (args.outname().size() > 4 && file_upper_ext(args.outname()) == "MPF")
			process_mpf(args);
		else
			process(args);



	}
	catch (const usage_exception& e)
	{
		std::cout << e.what() << endl;
		Arguments().print_usage();
		system("pause");
		return -1;
	}
	catch (const exception& e)
	{
		std::cout << e.what() << endl;
		system("pause");
		return -1;
	}

	



    return 0;
}

