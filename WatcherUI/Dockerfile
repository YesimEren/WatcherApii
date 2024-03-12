# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WatcherUI/WatcherUI.csproj", "WatcherUI/"]
COPY ["WatcherApı/WatcherApi.csproj", "WatcherApı/"]
RUN dotnet restore "WatcherUI/WatcherUI.csproj"
COPY . .

# wwwroot'u kopyala
COPY ["WatcherUI/wwwroot", "/src/WatcherUI/wwwroot"]

WORKDIR "/src/WatcherUI"
RUN dotnet build "WatcherUI.csproj" -c Release -o /app/publish/UI /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /src/WatcherUI/wwwroot /app/wwwroot
COPY --from=build /app/publish/UI .

# Entry point
ENTRYPOINT ["dotnet", "WatcherUI.dll"]
