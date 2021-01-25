import math


class Vector3:
    def __init__(self, x, y, z=0):
        """Construct 3D Vector with x, y, and z components"""
        self.x = x
        self.y = y
        self.z = z

    def magnitude(self):
        """Returns the magnitude of the vector"""
        return math.sqrt(self.x**2 + self.y**2 + self.z**2)

    def normalize(self):
        """Normalizes vector over magnitude"""
        mag = self.magnitude()

        x = self.x / mag
        y = self.y / mag
        z = self.z / mag

        return Vector3(x, y, z)

    def asDict(self):
        """Returns the vector as a dictionary"""
        data = {
            "x": self.x,
            "y": self.y,
            "z": self.z
        }
        return data

    @classmethod
    def fromDict(cls, data):
        """Creates a vector from a dictionary"""

        x = data['x']
        y = data['y']
        z = data['z']

        return Vector3(x, y, z)

    def __getitem__(self, index):
        return [self.x, self.y, self.z][index]

    def __setitem__(self, index, data):
        if (index is 0):
            self.x = data
        elif (index is 1):
            self.y = data
        elif (index is 2):
            self.z = data

    def __add__(self, other):
        x = self.x + other.x
        y = self.y + other.y
        z = self.z + other.z

        return Vector3(x, y, z)

    def __div__(self, other):
        return Vector3(self.x/other, self.y/other, self.z/other)

    def __mul__(self, other):
        return Vector3(self.x*other.x, self.y*other.y, self.z*other.z)

    def __sub__(self, other):
        x = self.x - other.x
        y = self.y - other.y
        z = self.z - other.z

        return Vector3(x, y, z)

    def __str__(self):
        return '({:.2f},{:.2f},{:.2f})'.format(self.x, self.y, self.z)


class Vector2:
    def __init__(self, x, y):
        """Construct 2D Vector with x and y components"""
        self.x = x
        self.y = y

    def magnitude(self):
        """Returns the magnitude of the vector"""
        return math.sqrt(self.x * self.x + self.y * self.y)

    def normalize(self):
        """Normalizes vector over magnitude"""
        x = self.x / (abs(self.x) + abs(self.y))
        y = self.y / (abs(self.x) + abs(self.y))

        return Vector2(x, y)

    def asList(self):
        """Returns the vector as a list"""
        data = [
            self.x,
            self.y,
        ]
        return data

    @classmethod
    def fromDict(cls, data):
        """Creates a vector from a dictionary"""
        x = data['x']
        y = data['y']

        return Vector2(x, y)

    def asDict(self):
        """Returns the vector as a dictionary"""
        data = {
            "x": self.x,
            "y": self.y,
        }

        return data

    def transpose(self):
        """Returns the transposed vector"""
        return Vector2(self.y, self.x)

    def __getitem__(self, index):
        return [self.x, self.y][index]

    def __setitem__(self, index, data):
        if (index is 0):
            self.x = data
        elif (index is 1):
            self.y = data

    def __add__(self, other):
        x = self.x + other.x
        y = self.y + other.y

        return Vector2(x, y)

    def __div__(self, other):
        return Vector2(self.x/other, self.y/other)

    def __mul__(self, other):
        return Vector2(self.x*other.x, self.y*other.y)

    def __sub__(self, other):
        x = self.x - other.x
        y = self.y - other.y

        return Vector2(x, y)

    def __str__(self):
        return '({:.2f},{:.2f})'.format(self.x, self.y)


class Calculator:
    """Random Math that I didn't know where else to put"""
    @staticmethod
    def angle_wrap(angle):
        """Wraps an angle (radians) around PI"""
        while (angle < -math.pi):
            angle += 2 * math.pi
        while (angle > math.pi):
            angle -= 2 * math.pi

        return angle

    @staticmethod
    def clip(value, minimum, maximum):
        """Because python doesn't have this..."""
        return max(maximum, min(minimum, value))

    @staticmethod
    def lerp(a, b, t):
        delta = a - b

        if (abs(delta) < 0.01):
            a = b
        else:
            a = a - delta / 100 * t

        return a
