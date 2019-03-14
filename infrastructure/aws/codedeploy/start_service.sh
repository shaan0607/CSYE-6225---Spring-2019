#!/bin/bash
cd /home/centos/WebApp/NoteApp_Production/aws/

# use systemd to start and monitor dotnet application
dotnet ef database update
sudo systemctl enable kestrel.service
sudo systemctl start kestrel.service
