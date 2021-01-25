from .network import Network

global network
network = Network(8052)

def get_id(obj):
    root = obj.__class__.__name__
    index = obj.index

    return str(root + str(index)).lower()


class Encoder:
    def __init__(self, index, K=1):
        """Initializes an encoder object with network reference"""
        # K is the distance of one rotation divided by the ticks of one rotation
        self.K = K
        self.index = index
        self.id = get_id(self)

    def set_distance_per_pulse(self, K):
        """Sets K, so you don't have to do it manually"""
        self.K = K

    def get(self):
        """Returns encoder value"""
        return network.request_variable(self.id).value

    def get_distance(self):
        """Returns distance using K"""
        return self.get() * self.K


class Motor:
    def __init__(self, index):
        """Initializes an motor object with network reference"""
        self.index = index
        self.id = get_id(self)

    def set_power(self, power: float):
        """Sets the motor's power"""
        network.update_variable(self.id, power)
