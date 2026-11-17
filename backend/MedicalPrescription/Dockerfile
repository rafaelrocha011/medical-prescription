# Build da API .NET 9; contexto de build = raiz do repositorio.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY .config/dotnet-tools.json ./.config/dotnet-tools.json
COPY backend/MedicalPrescription/ ./backend/MedicalPrescription/
RUN dotnet restore backend/MedicalPrescription/MedicalPrescription.Api/MedicalPrescription.Api.csproj
RUN dotnet publish backend/MedicalPrescription/MedicalPrescription.Api/MedicalPrescription.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

# ContÃªiner descartavel para executar as migrations sem afetar a API.
FROM build AS migrations
RUN dotnet tool restore
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "backend/MedicalPrescription/MedicalPrescription.Infrastructure/MedicalPrescription.Infrastructure.csproj", "--startup-project", "backend/MedicalPrescription/MedicalPrescription.Api/MedicalPrescription.Api.csproj", "--configuration", "Release", "--no-build"]

# Imagem final sem SDK nem ferramentas de desenvolvimento.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=40s --retries=5 CMD curl -fsS 'http://127.0.0.1:8080/api/prescricoes?pagina=1&tamanhoPagina=1' > /dev/null || exit 1
ENTRYPOINT ["dotnet", "MedicalPrescription.Api.dll"]
