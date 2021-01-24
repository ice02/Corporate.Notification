FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "Notification.Engine.csproj" -c Release -o /out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final
WORKDIR /app

COPY --from=builder /out .

ENTRYPOINT ["dotnet", "Notification.Engine.dll"]
EXPOSE 80
EXPOSE 443
