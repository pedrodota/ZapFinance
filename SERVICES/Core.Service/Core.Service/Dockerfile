# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["SERVICES/Core.Service/Core.Service/Core.Service.csproj", "SERVICES/Core.Service/Core.Service/"]
COPY ["LIBRARIES/ProtoServer/ProtoServer/ProtoServer.csproj", "LIBRARIES/ProtoServer/ProtoServer/"]

RUN dotnet restore "SERVICES/Core.Service/Core.Service/Core.Service.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SERVICES/Core.Service/Core.Service"
RUN dotnet build "Core.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.Service.dll"]
