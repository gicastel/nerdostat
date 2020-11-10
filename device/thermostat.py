import os
import json
import jsonpickle
from  datetime import datetime, timedelta

from gpiozero import LED, OutputDevice
import Adafruit_DHT as ada

# following lines depends on the cabling of the Raspberry - better in a config file?
dht = { "sensor" : ada.DHT22, "pin" : 4}
led = LED(17)

# my relay needs active_high = true
heater = OutputDevice(21, active_high=True, initial_value=False)

class Config(object):
    def __init__(self, program, ovUntil=None, ovSetpoint=None, ovDefault=4.0, threshold=0.2, hOnSince=None, awayTemp=13, noFrost=5):
        self.PROGRAM = program
        self.THRESHOLD = threshold
        self.OVERRIDE_UNTIL = ovUntil
        self.OVERRIDE_SETPOINT = ovSetpoint
        self.OVERRIDE_DEFAULT_DURATION = ovDefault
        self.HEATER_ON_SINCE = hOnSince
        self.AWAY_SETPOINT = awayTemp
        self.NOFROST_SETPOINT = noFrost

def refresh():
    global _config
    
    temp, hum = _ReadValues()
    setpoint = _GetCurrentSetpoint()

    diff = temp - setpoint

    if abs(diff) > _config.THRESHOLD:
        if diff < 0:
            _StartHeater()
        else:
            _StopHeater()
    
    heater_time, heater_is_active = _GetHeaterTime()

    overrideEnd = 0
    if _config.OVERRIDE_UNTIL is not None:
        overrideEnd = (_config.OVERRIDE_UNTIL - datetime.now()).total_seconds() * 1000

    # save config here
    open('data/config.json', 'w').write(jsonpickle.encode(_config))

    return temp, hum, setpoint, heater_time, heater_is_active, int(overrideEnd)

def _CreateDefaultConfig():
    p={}
    for wd in range(0,7):
        p[str(wd)] = {}
        for hour in range(0, 7):
            p[str(wd)][str(hour)] = {}
            for q in range(0, 4):
                p[str(wd)][str(hour)][str(q)] = 18
        for hour in range(7, 22):
            p[str(wd)][str(hour)] = {}
            for q in range(0, 4):
                p[str(wd)][str(hour)][str(q)] = 20
        for hour in range(22, 24):
            p[str(wd)][str(hour)] = {}
            for q in range(0, 4):
                p[str(wd)][str(hour)][str(q)] = 18
    return p

def _IsSetpointOverridden():
    global _config

    if _config.OVERRIDE_UNTIL is not None:
        if _config.OVERRIDE_UNTIL > datetime.now():
            return True
        else:
            _config.OVERRIDE_UNTIL = None
            return False
    else:
        return False

def _GetCurrentSetpoint():
    if _IsSetpointOverridden():
        return _config.OVERRIDE_SETPOINT
    else:
        wd = datetime.now().weekday()
        hour = datetime.now().hour
        quarter = int(datetime.now().minute / 15)
        return _config.PROGRAM[str(wd)][str(hour)][str(quarter)]

def overrideProgram(setpoint, hours):
    global _config

    _config.OVERRIDE_SETPOINT = setpoint
    _config.OVERRIDE_UNTIL = datetime.now() + timedelta(hours= hours if hours is not None else _config.OVERRIDE_DEFAULT_DURATION)

def away():
    global _config
    _config.OVERRIDE_SETPOINT = _config.AWAY_SETPOINT
    # at the moment the AWAY flag duration is ~forever. needs implementation
    _config.OVERRIDE_UNTIL = datetime.now() + timedelta(hours= 24*365*100)

def revertToProgram():
    global _config
    _config.OVERRIDE_SETPOINT = None
    _config.OVERRIDE_UNTIL = datetime.now() - timedelta(seconds = 10)

def _Init():
    if os.path.exists('data/config.json'):
        global _config
        cfgFile = open('data/config.json', 'r')
        cfgString = cfgFile.read()
        _config = jsonpickle.decode(cfgString)
    else:
        program = _CreateDefaultConfig()
        _config = Config(program)
        # resetting flag in case of device reboot
        _config.HEATER_ON_SINCE = None
        open('data/config.json', 'w').write(jsonpickle.encode(_config))

    return _config

# hardware related

def _ReadValues():
    hum, temp = ada.read_retry(**dht) #dht_sensor, dht_pin)
    return round(temp, 2), round(hum, 2)

def _GenerateValues():
    minute = datetime.now().minute
    baseTemp = 19.5
    baseTemp += 0.2 * int(minute/10)
    return baseTemp, 60

def _StartHeater():
    global _config
    heater.on()
    led.on()
    if _config.HEATER_ON_SINCE is None:
        _config.HEATER_ON_SINCE = datetime.now()

def _StopHeater():
    heater.off()
    led.off()

def _GetHeaterTime():
    global _config
    secs = 0
    if _config.HEATER_ON_SINCE is not None:
        now = datetime.now()
        delta = now - _config.HEATER_ON_SINCE
        _config.HEATER_ON_SINCE = now if heater.is_active else None
        secs = delta.seconds
    return secs, heater.is_active

_config = _Init()
