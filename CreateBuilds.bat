
REM Generates self contained builds (Side by side - SxS) for the various supported .NET Core platforms.
REM For additional RIDs (Runtime Identifiers) you can generate see https://docs.microsoft.com/en-us/dotnet/articles/core/rid-catalog
REM Those would get added into the runtimes{} element in the project.json

cd ./src/Jabbr-Core
pwd

ECHO win7-x64
dotnet publish --configuration Release  --framework netcoreapp1.0 --runtime  win7-x64 --output bin/Release/Publish/win7-x64

ECHO Creating ubuntu.14.04-x64
dotnet publish --configuration Release  --framework netcoreapp1.0 --runtime  ubuntu.14.04-x64 --output bin/Release/Publish/ubuntu.14.04-x64
