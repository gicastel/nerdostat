
def gen():
    p={}
    for wd in range(0,7):
        p[wd] = {}
        for hour in range(0, 7):
            p[wd][hour] = {}
            for q in range(0, 4):
                p[wd][hour][q] = 18
        for hour in range(7, 22):
            p[wd][hour] = {}
            for q in range(0, 4):
                p[wd][hour][q] = 20
        for hour in range(22, 24):
            p[wd][hour] = {}
            for q in range(0, 4):
                p[wd][hour][q] = 18
    return p

import Adafruit_DHT
import gpiozero
import sys
import time


dht = Adafruit_DHT.DHT22
dhtpin = 4

def read_dht():
    hum, temp =  Adafruit_DHT.read_retry(dht, dhtpin)
    print ("temp = {}, hum = {}".format(temp, hum))


relay_pin = 21
relay = gpiozero.OutputDevice(relay_pin, active_high=False, initial_value=False)

def set_relay(status):
    if status:
        print("Setting relay: ON")
        relay.on()
    else:
        print("Setting relay: OFF")
        relay.off()


def toggle_relay():
    print("toggling relay")
    relay.toggle()


def main_loop():
    # start by turning the relay off
    set_relay(False)
    while 1:
        # then toggle the relay every second until the app closes
        toggle_relay()
        # wait a second 
        time.sleep(1)


if __name__ == "__main__":
    try:
        main_loop()
    except KeyboardInterrupt:
        # turn the relay off
        set_relay(False)
        print("\nExiting application\n")
        # exit the application
        sys.exit(0)