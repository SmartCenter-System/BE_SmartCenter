FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SmartCenter.Api/SmartCenter.Api.csproj", "SmartCenter.Api/"]
COPY ["SmartCenter.Service/SmartCenter.Service.csproj", "SmartCenter.Service/"]
COPY ["SmartCenter.Repository/SmartCenter.Repository.csproj", "SmartCenter.Repository/"]
RUN dotnet restore "SmartCenter.Api/SmartCenter.Api.csproj"
COPY . .
WORKDIR "/src/SmartCenter.Api"
RUN dotnet build "./SmartCenter.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SmartCenter.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartCenter.Api.dll"]
