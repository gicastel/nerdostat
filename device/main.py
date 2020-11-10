#!/usr/bin/env python3

import time

import iothub as hub
import thermostat as thermo

INTERVAL = 5*60

def start_thermostat():
    try:
        print('Creating IoT client...')
        hub.iothub_client_init()

        while True:
            temp, hum, setpoint, heatertime, heateractive, overrideEnd = thermo.refresh()
            
            try:
                hub.sendMessage(temp, hum, setpoint, heatertime, heateractive, overrideEnd)
            except:
                pass

            time.sleep(INTERVAL)

    except KeyboardInterrupt:
        print ( "Nerdostat stopped" )

if __name__ == '__main__':
    print ( "--- Nerd-o-stat ---" )
    print ( "Press Ctrl-C to exit" )
    start_thermostat()
