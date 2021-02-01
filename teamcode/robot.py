import math

import resolver
from resolver import Calculator, Vector2, Vector3, network


class Robot(resolver.NetworkedRobot):
    # Override functions
    def robotInit(self):
        """Called when robot powers on"""
         # Create encoders
        self.left_encoder = resolver.Encoder(0)
        self.right_encoder = resolver.Encoder(1)

        # Create motors
        self.left_motor = resolver.Motor(0)
        self.right_motor = resolver.Motor(1)

        self.driver_joystick = resolver.Joystick(0)

        self.movement = Vector2(0,0)



    def autonomousInit(self):
        """Called only at the beginning of autonomous mode"""
        self.start_position = network.request_variable("world_position")['value']
        self.start_rotation = network.request_variable("world_rotation")['value']


    def autonomousPeriodic(self):
        """Called every 20ms in autonomous mode"""
        motor_power = self.go_to_point(Vector2(0,10))

        self.left_motor.set_power(motor_power.x)
        self.right_motor.set_power(motor_power.y)


    def teleopInit(self):
        """Called only at the beginning of teleop mode"""


    def teleopPeriodic(self):
        """Called every frame"""

        # self.go_to_point(Vector2(5,5))

        # Drives straight forward
        self.left_motor.set_power(self.driver_joystick.get_stick(0).y * 1)
        self.right_motor.set_power(self.driver_joystick.get_stick(0).y * 1)

    # Some math calculations
    def go_to_point(self, point:Vector2):

        # Should get position from encoders (something the kiddos can work on)
        server_pos = network.request_variable("world_position")['value']
        server_rot = network.request_variable("world_rotation")['value']

        # Cool little thing that can draw on the application
        network.add_overlay_line("robot_trajectory", Vector2.fromDict(server_pos), point,1,0,0)

        position_delta = point - Vector2.fromDict(server_pos)

        world_rotation = (math.radians(server_rot))

        distance_to_target = math.sqrt((position_delta.x)**2 + (position_delta.y)**2)

        abs_angle_to_target = math.atan2(position_delta.x, position_delta.y)
        relative_angle_to_target = Calculator.angle_wrap(abs_angle_to_target - (world_rotation))

        move = Calculator.clip(math.pow(1 - abs(relative_angle_to_target)/math.pi, 3) * distance_to_target/2, -1.0, 1.0)

        motor_powers = Vector2(move + relative_angle_to_target, move - relative_angle_to_target)

        # if distance_to_target < 0.001:
        #     motor_powers = Vector2(0,0)

        return motor_powers


if __name__ == "__main__":
    resolver.run(Robot)
