#!/bin/bash
cd /home/centos/WebApp/

# use systemd to start and monitor dotnet application
sudo systemctl enable kestrel.service
sudo systemctl start kestrel.service
