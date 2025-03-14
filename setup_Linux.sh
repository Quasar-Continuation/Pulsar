####################################################################
# Just setting up stuff so that way we dont run into issues later. #
# You will need to install dotnet cli on your linux system.        #
# https://learn.microsoft.com/en-us/dotnet/core/install/linux      #
# YOU SHOULD RUN THE BUILDER_LINUX.SH FILE INSTEAD OF THIS         #
####################################################################

## everything was wrote sleep deprived
cd ./Quasar.Server && dotnet add package System.Resources.Extensions && dotnet restore && cd ../
cd ./Quasar.Common.Tests && dotnet add package System.Resources.Extensions && dotnet restore && cd ../
cd ./Quasar.Common && dotnet add package System.Resources.Extensions && dotnet restore && cd ../
cd ./Quasar.Client && dotnet add package System.Resources.Extensions && dotnet restore && cd ../