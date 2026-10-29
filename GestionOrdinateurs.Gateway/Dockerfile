FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj GestionOrdinateurs.Gateway/
RUN dotnet restore GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj

COPY GestionOrdinateurs.Gateway/ GestionOrdinateurs.Gateway/
RUN dotnet publish GestionOrdinateurs.Gateway/GestionOrdinateurs.Gateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=5010
EXPOSE 5010
USER $APP_UID
ENTRYPOINT ["dotnet", "GestionOrdinateurs.Gateway.dll"]
