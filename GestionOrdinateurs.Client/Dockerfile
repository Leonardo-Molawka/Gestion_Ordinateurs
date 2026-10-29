FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj GestionOrdinateurs.Client/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj GestionOrdinateurs.Shared/
RUN dotnet restore GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj

COPY GestionOrdinateurs.Client/ GestionOrdinateurs.Client/
COPY GestionOrdinateurs.Shared/ GestionOrdinateurs.Shared/
RUN dotnet publish GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj -c Release -o /app/publish --no-restore

FROM nginx:stable-alpine AS runtime
COPY --from=build /app/publish/wwwroot/ /usr/share/nginx/html/
COPY GestionOrdinateurs.Client/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 5000
