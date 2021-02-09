#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 7000
EXPOSE 7001

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Notification.Watcher/Notification.AgentSmith.csproj", "Notification.Watcher/"]
COPY ["Notification.Data/Notification.Data.csproj", "Notification.Data/"]
RUN dotnet restore "Notification.Watcher/Notification.AgentSmith.csproj"
COPY . .
WORKDIR "/src/Notification.Watcher"
RUN dotnet build "Notification.AgentSmith.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.AgentSmith.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.AgentSmith.dll"]