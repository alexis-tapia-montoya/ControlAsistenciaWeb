# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ControlAsistenciaWeb.csproj", "./"]
RUN dotnet restore "ControlAsistenciaWeb.csproj"
COPY . .
RUN dotnet publish "ControlAsistenciaWeb.csproj" -c Release -o /app/publish

# Etapa 2: Entorno de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ControlAsistenciaWeb.dll"]