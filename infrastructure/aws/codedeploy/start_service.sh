#!/bin/bash
cd /home/centos/WebApp/

# use systemd to start and monitor dotnet application
sudo systemctl unmask kestrel.service
sudo systemctl status kestrel.service
sudo systemctl start kestrel.service

sudo systemctl status kestrel.service

