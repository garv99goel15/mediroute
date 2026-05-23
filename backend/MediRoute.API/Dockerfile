# syntax=docker/dockerfile:1
# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MediRoute.API.csproj ./
RUN dotnet restore MediRoute.API.csproj
COPY . .
RUN dotnet publish MediRoute.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Render sets $PORT dynamically. ASP.NET Core honors ASPNETCORE_URLS.
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "MediRoute.API.dll"]
