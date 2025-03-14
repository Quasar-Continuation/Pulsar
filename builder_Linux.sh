#!/bin/bash

#############################################################
#                                                           #
# ONLY RUN THIS IF YOU WANT TO COMPILE THE SERVER FOR LINUX #
#                 NEED TO INSTALL WINE!                     #
#############################################################

# Get the absolute path of the script
SCRIPT_PATH=$(realpath "$0")

# Get the parent directory of the script
PARENT_DIR=$(dirname "$SCRIPT_PATH")

dotnet clean && dotnet publish $PARENT_DIR/Quasar.sln -c Release -r linux-x64

# bash ./setup_Linux.sh #You dont need to run this however it ***might*** resolve some random issues.