FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Quitmed-Scraper.WebApp/Quitmed-Scraper.WebApp.csproj", "src/Quitmed-Scraper.WebApp/"]
COPY ["src/Quitmed-scraper.Database/Quitmed-scraper.Database.csproj", "src/Quitmed-scraper.Database/"]
COPY ["src/Quitmed-Scraper.Library/Quitmed-Scraper.Library.csproj", "src/Quitmed-Scraper.Library/"]
RUN dotnet restore "src/Quitmed-Scraper.WebApp/Quitmed-Scraper.WebApp.csproj"
COPY . .
WORKDIR "/src/src/Quitmed-Scraper.WebApp"
RUN dotnet build "Quitmed-Scraper.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Quitmed-Scraper.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quitmed-Scraper.WebApp.dll"]
