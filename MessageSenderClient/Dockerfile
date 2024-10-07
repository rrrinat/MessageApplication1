FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MessageSenderClient.csproj ./
RUN dotnet restore MessageSenderClient.csproj

COPY . ./
RUN dotnet publish MessageSenderClient.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "MessageSenderClient.dll"]
