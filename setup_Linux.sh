#!/bin/bash

####################################################################
# Just setting up stuff so that way we dont run into issues later. #
# You will need to install dotnet cli on your linux system.        #
# https://learn.microsoft.com/en-us/dotnet/core/install/linux      #
# YOU SHOULD RUN THE BUILDER_LINUX.SH FILE INSTEAD OF THIS         #
####################################################################

# Get the absolute path of the script
SCRIPT_PATH=$(realpath "$0")

# Get the parent directory of the script
PARENT_DIR=$(dirname "$SCRIPT_PATH")

## everything was wrote sleep deprived
cd $PARENT_DIR/Quasar.Server && dotnet add package System.Resources.Extensions && dotnet restore && cd ../
cd $PARENT_DIR/Quasar.Common.Tests && dotnet add package System.Resources.Extensions && dotnet restore && cd ../
cd $PARENT_DIR/Quasar.Common && dotnet add package System.Resources.Extensions && dotnet restore && cd ../
cd $PARENT_DIR/Quasar.Client && dotnet add package System.Resources.Extensions && dotnet restore && cd ../