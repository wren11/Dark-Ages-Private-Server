#pragma once
#include <stdint.h>
#include <vector>

class BitStack
{
public:
	BitStack():m_count(0),m_val(0)
	{
	}

	void push(bool bit);
	const std::vector<uint8_t>& data() const { return m_data; }


	void flush();
private:
	uint32_t m_count;
	uint8_t m_val;
	std::vector<uint8_t> m_data;

};

