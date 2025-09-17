FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Instalar Chromium y Chromedriver
RUN apt-get update && apt-get install -y \
    chromium \
    chromium-driver \
    fonts-liberation \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

RUN apt-get update && apt-get install -y \
    chromium \
    chromium-driver \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Puedes dejarlo root (más sencillo para Chrome en Docker)
# USER appuser

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NetSpotifyDownloaderApi/NetSpotifyDownloaderApi.csproj", "NetSpotifyDownloaderApi/"]
COPY ["NetSpotifyDownloaderCore/NetSpotifyDownloaderCore.csproj", "NetSpotifyDownloaderCore/"]
RUN dotnet restore "./NetSpotifyDownloaderApi/NetSpotifyDownloaderApi.csproj"
COPY . .
WORKDIR "/src/NetSpotifyDownloaderApi"
RUN dotnet build "./NetSpotifyDownloaderApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NetSpotifyDownloaderApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetSpotifyDownloaderApi.dll"]
