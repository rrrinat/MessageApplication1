FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY MessageService/MessageService.csproj ./MessageService/
RUN dotnet restore ./MessageService/MessageService.csproj

COPY MessageService/ ./MessageService/
RUN dotnet publish ./MessageService/MessageService.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

ENTRYPOINT ["dotnet", "MessageService.dll"]
