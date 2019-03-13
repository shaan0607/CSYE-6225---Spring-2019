#!/bin/bash
cd /home/centos/

# use systemd to start and monitor dotnet application
systemctl enable kestrel-aspnetcoreapp.service
systemctl start kestrel-aspnetcoreapp.service
