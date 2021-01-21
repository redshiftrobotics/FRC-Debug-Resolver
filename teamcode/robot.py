import json
import math
import socket
import sys
import threading
import time

from calculator import Calculator, Vector2, Vector3
from modules import Encoder, Motor
from network import Network


class Robot:
    def __init__(self, network: Network):
        # Pull reference to the network object
        self.network = network
        
        # Things we get from the server
        self.world_position = Vector2(0,0)
        self.world_rotation = 0

        # Create encoders
        self.left_encoder = Encoder(0, self.network)
        self.right_encoder = Encoder(1, self.network)

        # Create motors
        self.left_motor = Motor(0, self.network)
        self.right_motor = Motor(1, self.network)

    def loop(self):
        """Called every frame"""
        # Drives straight forward
        self.left_motor.setPower(1)
        self.right_motor.setPower(1)
        

    # No worko
    def go_to_point(self, point, move_speed, angle, turn_speed):
        distance_to_target = math.sqrt((point.x-self.world_position.x)**2 + (point.y-self.world_position.y)**2)

        abs_angle_to_target = math.atan2(point.y - self.world_position.y, point.x - self.world_position.x)

        relative_angle_to_target = Calculator.AngleWrap(abs_angle_to_target - (math.radians(self.world_rotation) - math.radians(90)))

        relative_to_target = Vector3(
            math.cos(abs_angle_to_target) * distance_to_target,
            math.sin(abs_angle_to_target) * distance_to_target
        ) 

        movement_power = relative_to_target.normalize()
        relative_turn_angle = relative_angle_to_target + angle

        self.movement = movement_power * Vector3(move_speed, move_speed)
        self.rotation = Vector3(0, relative_turn_angle/math.radians(30) * turn_speed)

        if (distance_to_target < 0.15):
            self.movement = Vector3(0,0)
            self.rotation = Vector3(0,0)
