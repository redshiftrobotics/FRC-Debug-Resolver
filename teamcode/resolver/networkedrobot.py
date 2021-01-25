import enum
import os
import sys

from .calculator import Vector2
from .modules import Encoder, Motor, network


class State(enum.Enum):
    NONE = 0
    AUTO_INITIALIZE = 1
    AUTO_LOOP = 2
    TELEOP_INITIALIZE = 3
    TELEOP_LOOP = 4


def run(robot):
    try:
        robot = robot()
    finally:
        # Disconnect
        network.disconnect()
        print(sys.exc_info()[0])

        # Exit this way because python is dumb
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)


class NetworkedRobot:
    """NetworkedRobot class that robot.py will inherit from"""

    def __init__(self):

        # Things we get from the server
        self.world_position = Vector2(0, 0)
        self.world_rotation = 0

        self.__state = State.NONE

        self.robotInit()
        self.__loop()

    def __loop(self):
        """Global state loop. Do not touch this"""

        auto_initialized = False
        teleop_initialized = False

        while True:
            self.__state = State(network.request_variable("state")['value'])

            if (self.__state is State.NONE):
                auto_initialized = False
                teleop_initialized = False

            if (self.__state is State.AUTO_INITIALIZE and not auto_initialized):
                self.autonomousInit()
                auto_initialized = True

            if (self.__state is State.AUTO_LOOP):
                self.autonomousPeriodic()

            if (self.__state is State.TELEOP_INITIALIZE and not teleop_initialized):
                self.teleopInit()
                teleop_initialized = True

            if (self.__state is State.TELEOP_LOOP):
                self.teleopPeriodic()

    def robotInit(self):
        """Called at initialization of the robot class. Override this"""

    def autonomousInit(self):
        """Called only at the beginning of autonomous mode. Override this"""

    def autonomousPeriodic(self):
        """Called every 20ms in autonomous mode. Override this"""

    def teleopInit(self):
        """Called only at the beginning of teleop mode. Override this"""

    def teleopPeriodic(self):
        """Called every frame in teleop mode. Override this"""
