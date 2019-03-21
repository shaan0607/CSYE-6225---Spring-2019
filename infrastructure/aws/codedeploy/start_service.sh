#!/bin/bash
cd /home/centos/WebApp/

# use systemd to start and monitor dotnet application
sudo systemctl enable kestrel.service
sudo systemctl start kestrel.service
sudo rm -rf /home/centos/WebApp/logs/*
sudo /opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl -a fetch-config -m ec2 -c file:/home/centos/WebApp/cloudwatch-config.json -s
