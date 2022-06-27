#define to_uint16(hb, lb)           (uint16_t)(((uint8_t)hb << 8) | (uint8_t)lb)
#define to_uint32(msb, hb, lb, lsb) (uint32_t)(((uint32_t)msb << 24) | ((uint32_t)hb << 16) | ((uint32_t)lb << 8) | (uint32_t)lsb)

// Set (1), clear (0) and invert (1->0; 0->1) bit in a register or variable easily
#define sbi(reg, bit) (reg) |=  (1 << (bit))
#define cbi(reg, bit) (reg) &= ~(1 << (bit))
#define ibi(reg, bit) (reg) ^=  (1 << (bit))

#define MAX(a, b) ((a) < (b) ? (b) : (a))
#define IS_BETWEEN(x, min, max) ((x >= min) && (x <= max))