FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ARG AK
ARG SK
COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "false"

COPY ["src/API/Sunny.Benefits.Bff.Api.csproj", "src/API/"]
COPY ["src/Infrastructure/Sunny.Benefits.Bff.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Core/Sunny.Benefits.Bff.Core.csproj", "src/Core/"]

RUN dotnet restore "src/API/Sunny.Benefits.Bff.Api.csproj"
COPY . .

WORKDIR "/src/src/API"
RUN dotnet build "Sunny.Benefits.Bff.Api.csproj" -c Release

WORKDIR /src
ARG AK
ARG SK
COPY nuget.config /src/nuget.config
COPY script.sh /src/script.sh
RUN chmod +x ./script.sh
RUN ./script.sh "$AK" "$SK" "true" "src/Core"
 
WORKDIR "/src/src/API"
FROM build AS publish
RUN dotnet publish "Sunny.Benefits.Bff.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT Development
ENTRYPOINT ["dotnet", "Sunny.Benefits.Bff.Api.dll"] 
