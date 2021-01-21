import json
import math
import os
import socket
import sys
import time

from calculator import Vector3
from network import Network
from robot import Robot

# Create a network object
network = Network(port=8052)

# Create the robot object with reference to the network
robot = Robot(network)

# Sleep until talking with server
network.wait_for_messages()

# Update loop
try:
    while(True):
        robot.loop()

# Hook and disconnect before exiting
except KeyboardInterrupt:
    # Disconnect
    network.disconnect()

    # Exit this way because python is dumb
    try:
        sys.exit(0)
    except SystemExit:
        os._exit(0)
