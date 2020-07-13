FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["WaterController/WaterController.csproj", "WaterController/"]
COPY ["ContextBrokerLibrary/ContextBrokerLibrary.csproj", "ContextBrokerLibrary/"]

RUN dotnet restore "WaterController/WaterController.csproj"
RUN dotnet restore "ContextBrokerLibrary/ContextBrokerLibrary.csproj"

# Copy everything else and build
COPY ["WaterController", "WaterController"]
COPY ["ContextBrokerLibrary", "ContextBrokerLibrary"]

WORKDIR /app/WaterController

RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app/WaterController
COPY --from=build-env /app/WaterController/out .

ENTRYPOINT ["dotnet", "WaterController.dll"]