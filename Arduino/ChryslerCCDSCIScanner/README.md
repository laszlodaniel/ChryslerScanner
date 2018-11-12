The .ino and .h files are used in Arduino IDE only.

The .hex files are compiled binaries. You can upload them with your preferred programmer through the ISP-10 connector.
If you want to program the scanner later through the USB connector with Arduino IDE then you have to burn the "with_bootloader" version.
The other one doesn't have bootloader section in it so only dedicated programmers can overwrite it.
One small advantage over the bootloader-version is that the program doesn't stand still for a moment after power up (waiting for possible re-programming) so the setup() function executes almost immediately.

After assembling the PCB for the first time the USB programming won't work because the ATmega2560 microcontroller isn't programmed with bootloader. Its flash memory is completely empty. First step is always to use a dedicated programmer and upload the "with_bootloader" .hex file through the ISP-10 connector. USB programming will work afterwards.