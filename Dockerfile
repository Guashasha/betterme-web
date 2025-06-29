# Usa dotnet como la imagen padre
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Se cambia al directorio /app dentro de la imagen
WORKDIR /App

# Copia todos los archivos a la imagen
COPY . ./

# Restaura lo necesario
RUN dotnet restore
# Build y publica el release
RUN dotnet publish -c Release -o out

# Construye la imagen
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "DotNet.Docker.dll"]
