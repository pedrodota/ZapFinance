# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["GATEWAYS/MobileAggregator/MobileAggregator/MobileAggregator.csproj", "GATEWAYS/MobileAggregator/MobileAggregator/"]
COPY ["LIBRARIES/ProtoServer/ProtoServer/ProtoServer.csproj", "LIBRARIES/ProtoServer/ProtoServer/"]

RUN dotnet restore "GATEWAYS/MobileAggregator/MobileAggregator/MobileAggregator.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/GATEWAYS/MobileAggregator/MobileAggregator"
RUN dotnet build "MobileAggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MobileAggregator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MobileAggregator.dll"]
