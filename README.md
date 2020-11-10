# nerdostat
A DIY connected thermostat using a Raspberry and Azure public cloud.

_DISCLAIMER: think about it before connecting anything to your boiler._

I have always been fond of controlling my home's temperature while away (and I really suggest you to get a Netatmo if you like the idea) - so, the day I got a Rasperry I started working on a smart (it's better to say "connected") thermostat for my family's house in the mountains.

The solution is made of 5 parts.
### /infra
The cloud infrastructure is made of: 
-	an Azure Iot Hub (free tier, less than 8k daily messages);
-	an Azure Function app (Consumption tier) with a bounded Application Insight;
-	a Storage account (V2, static site enabled).

Code definition missing at the moment.

### /device [Python]
Code for the Raspberry.
The program gets data from a DHT sensor and sends it to the Azure IoT hub, listens for any cloud-to-device message and controls the relay.
It also manages setpoints, manual overrides and a weekly program.
At the moment the diagram of the sensors connections is missing but I will add it in the future.

### /api [C#]
The Function app definition.
It manages incoming messages from the device, sending some data to Application Insight for cheap reporting (or to a PowerBI dataset) and listens for the API calls from the web app.

### /webapp [JavaScript]
Simple webapp to see the data from the sensor and to send commands to the device.

### /webapp_blazor [C#]
Rewrite of the previous app as a Blazor WASM - still WIP

### /shared [C#]
.Net shared project - JSON objects definition.
