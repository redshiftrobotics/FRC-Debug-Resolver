from network import Network


class Encoder:
    def __init__(self, index, network: Network, K=1):
        """Initializes an encoder object with network reference"""
        self.index = index
        self.network = network

        # K is the distance of one rotation divided by the ticks of one rotation
        self.K = K

    def setDistancePerPulse(self, K):
        """Sets K, so you don't have to do it manually"""
        self.K = K
    
    def get(self):
        """Returns encoder value"""
        return self.network.input_vars.encoder_count.asList()[self.index]

    def getDistance(self):
        """Returns distance using K"""
        return self.get() * self.K


class Motor:
    def __init__(self, index, network: Network):
        """Initializes an motor object with network reference"""
        self.index = index
        self.network = network
    
    def setPower(self, power: float):
        """Sets the motor's power"""
        self.network.output_vars.movement[self.index] = power


    
    
