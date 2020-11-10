from azure.iot.device import IoTHubDeviceClient, Message, MethodResponse
from datetime import datetime
import thermostat as thermo
import threading
import json

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
    
_callbackSelector = {
    "ReadNow" : _callback_ReadNow,
    "SetManualSetPoint" : _callback_Setpoint,
    "ClearManualSetPoint" : _callback_ClearSetpoint,
    "SetAwayOn" : _callback_Setpoint,
    "SetAwayOff" : _callback_ClearSetpoint
}

def iothub_client_init():
    # Create an IoT Hub client
    global _client
    try:
        _client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING)
        _client.connect()
        device_method_thread = threading.Thread(target=_deviceMethodListener, args=(_client,))
        device_method_thread.daemon = True
        device_method_thread.start()
        _led.on()
    except:
        _led.off()

def _deviceMethodListener(device_client):
    while True:
        method_request = device_client.receive_method_request()
        _led.blink(on_time=0.1, off_time=0.1)
        print (
            "\nMethod callback called with:\nmethodName = {method_name}\npayload = {payload}".format(
                method_name=method_request.name,
                payload=method_request.payload
            ) , flush=True
        )

        cb = _callbackSelector.get(method_request.name, lambda _: ({"Response" : "Not Defined"}, 404))

        response_payload, response_status = cb(method_request.payload)

        method_response = MethodResponse(method_request.request_id, response_status, payload=response_payload)
        print("\nResponse:\nPayload: {pl}\nStatus: {st}".format(pl=response_payload, st=response_status), flush=True)
        device_client.send_method_response(method_response)
        _led.on()

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

    print( "Sending message: {}".format(message))
    if _client is None:
        iothub_client_init()

    try:
        _client.send_message(message)
        _led.on()
        print("Message sent.")
    except Exception as ex:
        _client.disconnect()
        _client = None
        print (str(ex))
        _led.off()
