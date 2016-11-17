FROM microsoft/aspnetcore:1.0.1
ENTRYPOINT ["dotnet", "JabbR-Core.dll"]
ARG source=.
WORKDIR /app
EXPOSE 80
COPY $source .

ARG CONTAINER_PORT=5000
ARG SERVER_URLS=http://*:$CONTAINER_PORT
ENV SERVER.URLS $SERVER_URLS
EXPOSE $CONTAINER_PORT


ENTRYPOINT ["dnx", "-p", "project.json", "web"]