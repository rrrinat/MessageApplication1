FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MessageReceiverClient.csproj ./
RUN dotnet restore MessageReceiverClient.csproj

COPY . ./
RUN dotnet publish MessageReceiverClient.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "MessageReceiverClient.dll"]
