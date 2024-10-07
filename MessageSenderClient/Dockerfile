FROM mcr.microsoft.comdotnetsdk8.0 AS build
WORKDIR app

COPY .csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.comdotnetruntime8.0 AS runtime
WORKDIR app
COPY --from=build appout .

ENTRYPOINT [dotnet, MessageSenderClient.dll]
