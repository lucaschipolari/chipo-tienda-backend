# ── Build ────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/ChipoBackend.API/ChipoBackend.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
# Railway inyecta PORT y DATABASE_URL; el arranque los lee en Program.cs
ENTRYPOINT ["dotnet", "ChipoBackend.API.dll"]
