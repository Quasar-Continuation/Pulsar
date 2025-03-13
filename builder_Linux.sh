###############################################
# Need dotnet to be installed on linux system #
###############################################

# If youre building this and you get the following error: 
# error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.
# Then disable any proxies/vpns youre using then build.

dotnet clean && bash ./setup_Linux.sh && dotnet build Quasar.sln