#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ARG AK
ARG SK
COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "false"
COPY ["src/API/SunnyRewards.Helios.Wallet.Api.csproj", "src/API/"]
COPY ["src/Infrastructure/SunnyRewards.Helios.Wallet.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Core/SunnyRewards.Helios.Wallet.Core.csproj", "src/Core/"]
RUN dotnet restore "src/API/SunnyRewards.Helios.Wallet.Api.csproj"
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "SunnyRewards.Helios.Wallet.Api.csproj" -c Release

WORKDIR /src
ARG AK
ARG SK
COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "true" "src/Core"

WORKDIR "/src/src/API"
FROM build AS publish
RUN dotnet publish "SunnyRewards.Helios.Wallet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT Development
ENTRYPOINT ["dotnet", "SunnyRewards.Helios.Wallet.Api.dll"]