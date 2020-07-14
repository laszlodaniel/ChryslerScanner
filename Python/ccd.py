import serial
import time
import argparse


com = serial.Serial()
# com.port = 'COM3'
com.port = '/dev/ttyUSB0'
com.baudrate = 250000
com.timeout = 2
com.dtr = None  # Disable auto-reset upon USB-connection
com.open()


def send_requests():
    time.sleep(4)  # wait for reset
    with open('requests.txt', "r") as f:
        for line in f:
            request = bytes.fromhex(line.replace('\n', ''))
            packet_length = 2 + len(request)
            packet_length_hb = (packet_length >> 8) & 0xFF
            packet_length_lb = packet_length & 0xFF
            packet = bytearray([0x3D, packet_length_hb, packet_length_lb, 0x16, 0x02])
            packet.extend(request)

            checksum = 0
            i = 1
            while i < len(packet):
                checksum += packet[i]
                i += 1

            checksum = (checksum & 0xFF).to_bytes(1, 'little')
            packet.extend(checksum)

            com.write(packet)

            echo = False
            timeout = time.time() + 2
            while not echo:
                if time.time() > timeout:
                    break
                if com.inWaiting():
                    data = None
                    try:
                        data = com.read(com.inWaiting())
                    except TypeError:
                        pass
                    if len(data) > 9:
                        if data[9] == 0xF2:
                            echo = True
                            response = data[9:(len(data)-1)]
                            print(" ".join(["{:02x}".format(x) for x in request]).upper())
                            print(" ".join(["{:02x}".format(x) for x in response]).upper())
                            with open('codes.txt', "a") as codes:
                                codes.write(" ".join(["{:02x}".format(x) for x in request]).upper() + "\n")
                                codes.write(" ".join(["{:02x}".format(x) for x in response]).upper() + "\n\n")
            if not echo:
                print(" ".join(["{:02x}".format(x) for x in request]).upper() + ": timeout")
    com.close()


def read_ccd_bus():
    while True:
        if com.inWaiting():
            data = None
            try:
                data = com.read(com.inWaiting())
            except TypeError:
                pass
            if data[3] == 0x47 and len(data) > 9:
                message = data[9:(len(data)-1)]
                print(" ".join(["{:02x}".format(x) for x in message]).upper())
                with open('messages.txt', "a") as messages:
                    messages.write(" ".join(["{:02x}".format(x) for x in message]).upper() + "\n")
    com.close()


parser = argparse.ArgumentParser(description='CCD-bus reader script')
parser.add_argument('operation')
args = vars(parser.parse_args())

if args['operation'] == 'write':
    send_requests()

if args['operation'] == 'read':
    read_ccd_bus()
