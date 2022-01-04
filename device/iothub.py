from azure.iot.device import IoTHubDeviceClient, Message, MethodResponse
from datetime import datetime

import jsonpickle
import thermostat as thermo

from gpiozero import LED

# led for monitoring azure connection
_led = LED(18)

# insert here IoT hub connection string
CONNECTION_STRING = ""

_client = None

def _callback_ReadNow(_):
    return _generateMessageJson(*thermo.refresh()), 200

def _callback_Setpoint(pl):
    setpoint = pl.get("setpoint")
    duration = pl.get("hours", None)
    thermo.overrideProgram(setpoint, duration)
    return _generateMessageJson(*thermo.refresh()), 200

def _callback_SetAwayOn(_):
    thermo.away()
    return _generateMessageJson(*thermo.refresh()), 200

def _callback_ClearSetpoint(_):
    thermo.revertToProgram()
    return _generateMessageJson(*thermo.refresh()), 200

def _callback_GetProgram(_):
    program = thermo._config.PROGRAM
    return jsonpickle.encode(program), 200

def _callback_SetProgram(pl):
    program = pl.get("Program")
    thermo._config.PROGRAM = program
    return jsonpickle.encode(program), 200

_callbackSelector = {
    "ReadNow" : _callback_ReadNow,
    "SetManualSetPoint" : _callback_Setpoint,
    "ClearManualSetPoint" : _callback_ClearSetpoint,
    "SetAwayOn" : _callback_Setpoint,
    "SetAwayOff" : _callback_ClearSetpoint,
    
    "GetProgram" : _callback_GetProgram,
    "SetProgram" : _callback_SetProgram
}

def iothub_client_init():
    # Create an IoT Hub client
    global _client
    _client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING, auto_connect=True)
    try:

        def device_method_handler(method_request):
            _led.blink(on_time=0.1, off_time=0.1)
            print (
                "\nMethod callback called with:\nmethodName = {method_name}\npayload = {payload}".format(
                    method_name=method_request.name,
                    payload=method_request.payload
                ) , flush=True
            )

            cb = _callbackSelector.get(method_request.name, lambda _: ({"Response" : "Not Defined"}, 404))

            response_payload, response_status = cb(method_request.payload)

            method_response = MethodResponse.create_from_method_request(method_request, response_status, payload=response_payload)
            print("\nResponse:\nPayload: {pl}\nStatus: {st}".format(pl=response_payload, st=response_status), flush=True)
            _client.send_method_response(method_response)
            _led.on()

        _client.on_method_request_received = device_method_handler

        _client.connect()
        _led.on()
    except:
        _client.shutdown()
        _client = None
        _led.off()

MSG_TXT = """{{"Timestamp" : "{timestamp}", 
               "Temperature": {temperature}, 
               "Humidity": {humidity}, 
               "CurrentSetpoint" : {setpoint}, 
               "HeaterOn" : {heatertime}, 
               "IsHeaterOn" : {heateractive},
               "OverrideEnd" : {overrideEnd} }}"""

def _generateMessageJson(temperature, humidity, setpoint, heatertime, heateractive, overrideEnd):
    msg_txt_formatted = MSG_TXT.format(
        timestamp = datetime.now().strftime('%Y-%m-%dT%H:%M:%S.%f'), 
        temperature = temperature, 
        humidity = humidity, 
        setpoint = setpoint,
        heatertime = heatertime,
        heateractive = str(heateractive).lower(),
        overrideEnd = overrideEnd
        )
    return msg_txt_formatted

def sendMessage(temperature, humidity, setpoint, heatertime, heateractive, overrideEnd):
    global _client
    _led.blink(on_time=0.1, off_time=0.1)

    msg_txt = _generateMessageJson(temperature, humidity, setpoint, heatertime, heateractive, overrideEnd)
    message = Message(msg_txt)

    message.custom_properties['testDevice'] = 'true'

    print( "Sending message: {}".format(message))
    if _client is None:
        iothub_client_init()

    try:
        _client.send_message(message)
        _led.on()
        print("Message sent.")
    except Exception as ex:
        _client.shutdown()
        _client = None
        print (str(ex))
        _led.off()
