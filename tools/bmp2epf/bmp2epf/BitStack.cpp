#include "BitStack.h"



void BitStack::push(bool bit)
{
	uint8_t v = static_cast<uint32_t>(bit);
	m_val |= v << m_count;
	if (++m_count == 8)
		flush();
}

void BitStack::flush()
{
	if (m_count)
	{
		m_data.push_back(m_val);
		m_val = 0;
		m_count = 0;
	}
}
