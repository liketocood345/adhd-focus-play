#!/usr/bin/env python3
"""HybridBCI → Unity UDP 桥接示例。Unity 侧 transportType=udp, port=9876。"""
import json
import socket
import time

HOST = "127.0.0.1"
PORT = 9876


def demo_frame(t: float) -> dict:
    return {
        "focus": 50 + 40 * abs((t % 4) - 2) / 2,
        "blink": int(t) % 7 == 0,
        "head": "nod" if int(t) % 11 == 0 else "none",
    }


def main():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    print(f"Sending demo BCI frames to {HOST}:{PORT}")
    t0 = time.time()
    while True:
        frame = demo_frame(time.time() - t0)
        sock.sendto(json.dumps(frame).encode("utf-8"), (HOST, PORT))
        time.sleep(0.05)


if __name__ == "__main__":
    main()
