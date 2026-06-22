# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
COPY ["SmartBookAPI.csproj", "./"]
RUN dotnet restore

# Copiar todo el codigo y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar la aplicacion compilada
COPY --from=build /app/publish .

# Variable de entorno para produccion
ENV ASPNETCORE_ENVIRONMENT=Production

# Exponer puerto (Railway usa la variable PORT)
EXPOSE 8080

# Comando para ejecutar la aplicacion
ENTRYPOINT ["dotnet", "SmartBookAPI.dll"]
