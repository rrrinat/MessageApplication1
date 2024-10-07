FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MessageHistoryClient.csproj ./
RUN dotnet restore MessageHistoryClient.csproj

COPY . ./
RUN dotnet publish MessageHistoryClient.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "MessageHistoryClient.dll"]
