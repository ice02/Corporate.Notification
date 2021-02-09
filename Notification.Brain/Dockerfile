#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Notification.Brain/Notification.Brain.csproj", "Notification.Brain/"]
COPY ["Notification.Data/Notification.Data.csproj", "Notification.Data/"]
RUN dotnet restore "Notification.Brain/Notification.Brain.csproj"
COPY . .
WORKDIR "/src/Notification.Brain"
RUN dotnet build "Notification.Brain.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.Brain.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Brain.dll"]