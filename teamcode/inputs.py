import time

import pygame

from calculator import Calculator

pygame.init()


class KeyboardInput:
    """Class for getting keyboard input from pygame"""

    def __init__(self, weight):
        self.weight = weight
        self.axis = [0,0]

        self.last_time = time.time() * 1000
        self.delta_time = 0

    def update(self):
        """Updates the axis"""
        new_time = (time.time() * 1000)

        self.delta_time = new_time - self.last_time
        self.last_time = new_time

        events = pygame.event.get()


        keys = pygame.key.get_pressed()

        horizontal = keys[pygame.K_RIGHT] - keys[pygame.K_LEFT]
        vertical = keys[pygame.K_DOWN] - keys[pygame.K_UP]

        self.axis[0] = Calculator.lerp(self.axis[0], horizontal, self.weight * self.delta_time)
        self.axis[1] = Calculator.lerp(self.axis[1], vertical, self.weight * self.delta_time)



    def get_axis(self, index):
        """Returns the value of an axis"""
        return self.axis[index]


global INPUTS
INPUTS = KeyboardInput(5)
