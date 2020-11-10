import json

class LED:
    is_active = None
    
    def __init__(self, int):
        self.is_active = False
        return

    def on(self):
        self.is_active = True
        return

    def off(self):
        self.is_active = False
        return
    
    def blink(self):
        return

def Config(object):
    def __init__(self, j):
        self.__dict__ = json.loads(j)
