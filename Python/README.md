Experimental Python code for Raspberry Pi

Usage:

```
python ccd.py read
python ccd.py write
```

The "read" argument only monitors the CCD-bus and does nothing with it:

![Python CCD read](https://chryslerccdsci.files.wordpress.com/2020/07/python_ccd_read_01.png)

The "write" argument makes the Python code read the lines from the "requests.txt" file and write it to the CCD-bus one-by-one:

![Python CCD write](https://chryslerccdsci.files.wordpress.com/2020/07/python_ccd_write_01.png)