FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GestionOrdinateurs.API/GestionOrdinateurs.API.csproj GestionOrdinateurs.API/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj GestionOrdinateurs.Shared/
RUN dotnet restore GestionOrdinateurs.API/GestionOrdinateurs.API.csproj

COPY GestionOrdinateurs.API/ GestionOrdinateurs.API/
COPY GestionOrdinateurs.Shared/ GestionOrdinateurs.Shared/
RUN dotnet publish GestionOrdinateurs.API/GestionOrdinateurs.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /data && chown $APP_UID:$APP_UID /data
ENV ASPNETCORE_HTTP_PORTS=5100
EXPOSE 5100
USER $APP_UID
ENTRYPOINT ["dotnet", "GestionOrdinateurs.API.dll"]
