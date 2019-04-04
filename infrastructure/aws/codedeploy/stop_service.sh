#!/bin/bash

# stop dotnet application
sudo systemctl stop kestrel.service
sudo systemctl disable kestrel
