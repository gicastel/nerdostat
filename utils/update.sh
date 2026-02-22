#!/bin/sh
sudo systemctl stop nerdostat.service
rm deploy/config.json
cp device/config.json deploy/config.json
cp device/nerdostat.sqlite deploy/nerdostat.sqlite
sudo chmod +x deploy/Nerdostat.Device
rm -r device
mv deploy device
sudo systemctl start nerdostat.service
echo "Nerdostat device updated successfully."