#!/bin/bash
cd /home/centos/WebApp/

# use systemd to start and monitor dotnet application
sudo systemctl stop kestrel.service
sudo systemctl disable kestrel
sudo systemctl unmask kestrel.service
sudo systemctl status kestrel.service
sudo systemctl start kestrel.service
sudo systemctl enable kestrel
sudo systemctl status kestrel.service

