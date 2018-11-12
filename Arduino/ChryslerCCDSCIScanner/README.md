The .ino and .h files are used in Arduino IDE only.

The .hex files are compiled binaries. You can upload them with your preferred programmer. If you want to program the scanner later with Arduino IDE you have to use the "with_bootloader" version. The other one doesn't have bootloader section in it so only dedicated programmers can overwrite it.