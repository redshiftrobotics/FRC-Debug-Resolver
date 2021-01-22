import json
import socket
import sys
import threading
import time

from calculator import Vector2


class InputNetworkVars:
    """The variables that will be received from the server"""
    def __init__(self, world_position: Vector2 = Vector2(0, 0), world_rotation: float = 0, encoder_count: Vector2 = Vector2(0, 0)):
        self.world_position = world_position
        self.world_rotation = world_rotation
        self.encoder_count = encoder_count

    @classmethod
    def from_bytes(cls, data):
        """Builds class from bytes"""
        msg = json.loads(data)

        world_position = Vector2.fromDict(msg['outputWorldPosition'])
        world_rotation = float(msg['outputWorldRotation'])
        encoder_count = Vector2.fromDict(msg['outputEncoderCount'])

        return cls(world_position, world_rotation, encoder_count)


class OutputNetworkVars:
    """The variables that will be sent to the server"""
    def __init__(self, movement: Vector2 = Vector2(0, 0), rotation: float = 0):
        self.movement = movement
        self.rotation = rotation

    def as_bytes(self):
        """Builds bytes from class"""
        # Create object to send
        data = {
            "movement": self.movement.asDict(),
            "rotation": self.rotation
        }

        # Gather it as bytes
        return json.dumps(data).encode()


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

        self.input_vars = InputNetworkVars()
        self.output_vars = OutputNetworkVars()

        # Target a thread to chattering
        self.chatter_thread = threading.Thread(target=self.chatter)

        # After initializing, try to connect
        self.connect()

    def connect(self):
        """Connects..."""
        print('connecting to %s port %s' % self.server_address)
        self.sock.connect(self.server_address)
        self.connected = True
        print('connected\n')

        # Starts receiving messages in another thread
        self.chatter_thread.start()


    def disconnect(self):
        """Close off the socket"""
        self.shutdown = True
        self.chatter_thread.join()
        self.sock.close()
        self.connected = False
        print('disconnected')

    def wait_for_messages(self):
        """Sleep until got at least one message (to avoid initialization bugs)"""
        while (not self.message_count > 1):
            time.sleep(0.01)

    def chatter(self):
        """Send and receive with server"""
        while(not self.shutdown):
            # We must be connected to send data
            if (not self.connected):
                return
            # Chatter with server
            self.push()
            self.pull()


    def push(self):
        """Push vars to server"""
        try:
            # Send the data to the server
            self.sock.send(self.output_vars.as_bytes())
        except:
            print(sys.exc_info()[0])
            pass



    def pull(self):
        """Receive vars from server"""
        try:
            # Read 1024 bytes
            message = InputNetworkVars.from_bytes(self.sock.recv(1024))

            self.input_vars = message
            self.message_count += 1
        except:
            print(sys.exc_info()[0])
            pass
