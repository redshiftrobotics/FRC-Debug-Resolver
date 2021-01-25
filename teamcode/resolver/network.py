import json
import random
import socket
import sys
import threading
import time

from .calculator import Vector2, Vector3


# Network object
class Network:
    """Performs networking with Unity"""

    def __init__(self, port):
        # Socket for networking
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        # Running on localhost
        self.server_address = ('127.0.0.1', port)
        self.message_count = 0

        self.connected = False
        self.shutdown = False

        # After initializing, try to connect
        self.connect()

    def connect(self):
        """Connects..."""
        print('connecting to %s port %s' % self.server_address)
        self.sock.connect(self.server_address)
        self.connected = True
        print('connected\n')

        # Starts receiving messages in another thread
        # self.chatter_thread.start()

    def disconnect(self):
        """Close off the socket"""

        # Send disconnect packet
        data = {
            "header": "disconnect"
        }

        self.sock.send(json.dumps(data).encode())
        time.sleep(0.01)

        self.sock.close()
        self.connected = False
        print('disconnected')

    def request_variable(self, variable):
        """Ask the server to respond with a variable"""
        message = {
            "header": "request",
            "variable": variable
        }

        self.sock.send(json.dumps(message).encode())

        msg = ''
        while True:
            response = self.sock.recv(1)
            # End of message
            if response.hex() == '00':
                break
            msg += response.decode('ascii')

        return json.loads(msg)

    def update_variable(self, variable, value):
        """Sets a variable on the server"""
        message = {
            "header": "set_variable",
            "variable": variable,
            "value": value
        }

        self.sock.send(json.dumps(message).encode())

        msg = ''
        while True:
            response = self.sock.recv(1)
            # End of message
            if response.hex() == '00':
                break
            msg += response.decode('ascii')

        return json.loads(msg)

    def add_overlay_point(self, id: str, point: Vector3, radius: float, r, g, b):
        """Draws a circle on the simulator. Big brain networking code for dis one"""
        data = {
            "header": "add_overlay_point",
            "value": {
                "id": id,
                "center": point.asDict(),
                "radius": radius,
                "color": {
                    "r": r,
                    "g": g,
                    "b": b
                }
            }
        }


        self.sock.send(json.dumps(data).encode())

        msg = ''
        while True:
            response = self.sock.recv(1)
            # End of message
            if response.hex() == '00':
                break
            msg += response.decode('ascii')

        return json.loads(msg)

    def add_overlay_line(self, id: str, start: Vector3, end: Vector3, r, g, b):
        """Draws a line on the simulator. Big brain networking code for dis one"""
        data = {
            "header": "add_overlay_line",
            "value": {
                "id": id,
                "start": start.asDict(),
                "end": end.asDict(),
                "color": {
                    "r": r,
                    "g": g,
                    "b": b
                }
            }
        }


        self.sock.send(json.dumps(data).encode())

        msg = ''
        while True:
            response = self.sock.recv(1)
            # End of message
            if response.hex() == '00':
                break
            msg += response.decode('ascii')

        return json.loads(msg)

