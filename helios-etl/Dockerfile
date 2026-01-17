# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build stage using the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ARG AK
ARG SK

COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "false"

# Copy project files
COPY ["src/App/SunnyRewards.Helios.ETL.App.csproj", "src/App/"]
COPY ["src/Infrastructure/SunnyRewards.Helios.ETL.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Core/SunnyRewards.Helios.ETL.Core.csproj", "src/Core/"]
COPY ["src/Common/SunnyRewards.Helios.ETL.Common.csproj", "src/Common/"]

# Restore dependencies
RUN dotnet restore "src/App/SunnyRewards.Helios.ETL.App.csproj"

# Copy full source code
COPY . .

# Build the application
WORKDIR "/src/src/App"
RUN dotnet build "SunnyRewards.Helios.ETL.App.csproj" -c Release --no-restore

# Additional script execution
WORKDIR /src
COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "true" "src/Core"

WORKDIR "/src/src/App"

# Publish the application
FROM build AS publish
RUN dotnet publish "SunnyRewards.Helios.ETL.App.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage - Use ASP.NET Core runtime instead of .NET runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variable
ENV ASPNETCORE_ENVIRONMENT={{RUNTIME_ENVIRONMENT}}

# Run the application
CMD ["dotnet", "SunnyRewards.Helios.ETL.App.dll"]
