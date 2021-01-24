#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Notification.Cockpit/Notification.Cockpit.csproj", "Notification.Cockpit/"]
RUN dotnet restore "Notification.Cockpit/Notification.Cockpit.csproj"
COPY . .
WORKDIR "/src/Notification.Cockpit"
RUN dotnet build "Notification.Cockpit.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.Cockpit.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Cockpit.dll"]