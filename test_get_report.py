#!/usr/bin/env python3
"""Send GET_REPORT to a HID device to test if it causes freezing."""

import fcntl
import os
import sys

# HIDIOCGFEATURE ioctl - sends GET_REPORT (feature) to device
# _IOC(IOC_READ|IOC_WRITE, 'H', 0x07, len)
def HIDIOCGFEATURE(length):
    return (3 << 30) | (ord('H') << 8) | 0x07 | (length << 16)

# HIDIOCGINPUT - get input report
def HIDIOCGINPUT(length):
    return (3 << 30) | (ord('H') << 8) | 0x0A | (length << 16)

def test_get_report(device_path, report_id=0):
    print(f"Opening {device_path}...")
    try:
        fd = os.open(device_path, os.O_RDWR | os.O_NONBLOCK)
    except PermissionError:
        print("ERROR: Permission denied. Run with sudo.")
        return False

    buf = bytearray(64)
    buf[0] = report_id

    print(f"Sending GET_FEATURE_REPORT (report_id={report_id})...")
    try:
        fcntl.ioctl(fd, HIDIOCGFEATURE(len(buf)), buf)
        print(f"SUCCESS - Got response: {buf[:16].hex()}")
    except OSError as e:
        print(f"Feature report failed (normal for keyboards): {e}")

    buf = bytearray(64)
    buf[0] = report_id
    print(f"Sending GET_INPUT_REPORT (report_id={report_id})...")
    try:
        fcntl.ioctl(fd, HIDIOCGINPUT(len(buf)), buf)
        print(f"SUCCESS - Got response: {buf[:16].hex()}")
    except OSError as e:
        print(f"Input report failed: {e}")

    os.close(fd)
    print("Device still responding? Try typing something!")
    return True

if __name__ == "__main__":
    # Lenovo receiver hidraw devices
    devices = ["/dev/hidraw2", "/dev/hidraw3", "/dev/hidraw6", "/dev/hidraw7"]

    if len(sys.argv) > 1:
        devices = [sys.argv[1]]

    for dev in devices:
        print(f"\n{'='*50}")
        test_get_report(dev)
        print("Press Enter to continue to next interface (or Ctrl+C to stop)...")
        try:
            input()
        except KeyboardInterrupt:
            break
