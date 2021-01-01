#pragma once

#include <vector>
#include <stdint.h>
#include "EfpFile.h"


#pragma pack(1)
struct FrameIndices
{
	unsigned char walkFrameIndex;
	unsigned char walkFrameCount;
	unsigned char _attackFrameIndex;
	unsigned char _attackFrameCount;
	unsigned char _stopFrameIndex;
	unsigned char _stopFrameCount;
	unsigned char _stopMotionFrameCount;
	unsigned char _stopMotionProbability;
};

struct FrameIndices2
{
	unsigned char walkFrameIndex;
	unsigned char walkFrameCount;
	unsigned short ff_flag;
	unsigned char _stopFrameIndex;
	unsigned char _stopFrameCount;
	unsigned char _stopMotionFrameCount;
	unsigned char _stopMotionProbability;
	unsigned char _attackFrameIndex;
	unsigned char _attackFrameCount;
	unsigned char _attack2FrameIndex;
	unsigned char _attack2FrameCount;
	unsigned char _attack3FrameIndex;
	unsigned char _attack3FrameCount;
};
#pragma pack()


class MpfFile: public EpfFile
{
public:
	MpfFile();
	void create_raw_data(std::vector<uint8_t>* out);
	inline void set_frame_indices(const FrameIndices& indices) { m_frame_indeces = indices; }
	inline void set_frame_indices2(const FrameIndices2& indices) {
		m_frame_indeces2 = indices; m_is_indices2 = true;}
	void append_frame(const Rect& pos, uint16_t x_offs, uint16_t y_offs, const BmpData &data);
	void set_pallete_number(uint32_t num) { m_pallete_number = num; }

protected:
	FrameIndices m_frame_indeces;
	FrameIndices2 m_frame_indeces2;
	bool m_is_indices2;
	std::vector<uint16_t> m_xoffset;
	std::vector<uint16_t> m_yoffset;
	uint32_t m_pallete_number;

public:
	static int test();




};

