from calculator import Vector2
from inputs import INPUTS
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
        x = INPUTS.get_axis(0)
        y = -INPUTS.get_axis(1)

        # Drives straight forward
        self.left_motor.setPower(y+x)
        self.right_motor.setPower(y-x)
