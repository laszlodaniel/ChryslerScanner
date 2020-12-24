#include "common.h"

uint8_t ack[1] = { 0x00 }; // acknowledge payload array
uint8_t err[1] = { 0xFF }; // error payload array

uint8_t hw_version[2];
uint8_t hw_date[8];
uint8_t assembly_date[8];

uint32_t current_millis = 0;
uint8_t current_timestamp[4]; // current time is stored here when "update_timestamp" is called

bus ccd, pcm, tcm;
