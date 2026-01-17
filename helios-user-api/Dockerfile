#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 443

# Install Chrome dependencies and Chrome via .deb
RUN apt-get update && apt-get install -y --no-install-recommends \
    wget \
    fonts-liberation \
    libnss3 \
    libx11-xcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdbus-1-3 \
    libgdk-pixbuf2.0-0 \
    xdg-utils \
    --no-install-recommends && \
    wget --https-only --max-redirect=0 https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb && \
    apt-get install -y --no-install-recommends ./google-chrome-stable_current_amd64.deb && \
    rm google-chrome-stable_current_amd64.deb && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

ENV CHROME_BIN=/usr/bin/google-chrome

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG AK
ARG SK
COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "false"

COPY ["src/API/SunnyRewards.Helios.User.Api.csproj", "src/API/"]
COPY ["src/Infrastructure/SunnyRewards.Helios.User.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Core/SunnyRewards.Helios.User.Core.csproj", "src/Core/"]

RUN dotnet restore "src/API/SunnyRewards.Helios.User.Api.csproj"
COPY . .

WORKDIR "/src/src/API"
RUN dotnet build "SunnyRewards.Helios.User.Api.csproj" -c Release

WORKDIR /src
ARG AK
ARG SK
COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "true" "src/Core"

WORKDIR "/src/src/API"
FROM build AS publish
RUN dotnet publish "SunnyRewards.Helios.User.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT Development
ENTRYPOINT ["dotnet", "SunnyRewards.Helios.User.Api.dll"]
