# Multi-stage — contexto: pasta alert-service (monorepo) ou repo standalone.

FROM mcr.microsoft.com/dotnet/sdk:10.0-bookworm-slim AS builder

WORKDIR /src

ARG BUILD_CONFIGURATION=Release
ARG TOKEN
ARG NUGET_GITHUB_OWNER

RUN if [ -n "${TOKEN:-}" ] && [ -n "${NUGET_GITHUB_OWNER:-}" ]; then \
    dotnet nuget add source "https://nuget.pkg.github.com/${NUGET_GITHUB_OWNER}/index.json" \
      --name github \
      --username "${NUGET_GITHUB_OWNER}" \
      --password "${TOKEN}" \
      --store-password-in-clear-text; \
  fi

COPY ["Simcag.AlertService.Api/Simcag.AlertService.Api.csproj", "Simcag.AlertService.Api/"]
COPY ["Simcag.AlertService.Application/Simcag.AlertService.Application.csproj", "Simcag.AlertService.Application/"]
COPY ["Simcag.AlertService.Domain/Simcag.AlertService.Domain.csproj", "Simcag.AlertService.Domain/"]
COPY ["Simcag.AlertService.Infrastructure/Simcag.AlertService.Infrastructure.csproj", "Simcag.AlertService.Infrastructure/"]

RUN dotnet restore "Simcag.AlertService.Api/Simcag.AlertService.Api.csproj"

COPY ["Simcag.AlertService.Api/", "Simcag.AlertService.Api/"]
COPY ["Simcag.AlertService.Application/", "Simcag.AlertService.Application/"]
COPY ["Simcag.AlertService.Domain/", "Simcag.AlertService.Domain/"]
COPY ["Simcag.AlertService.Infrastructure/", "Simcag.AlertService.Infrastructure/"]

RUN dotnet publish "Simcag.AlertService.Api/Simcag.AlertService.Api.csproj" \
    --configuration "${BUILD_CONFIGURATION}" \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-bookworm-slim AS runtime

WORKDIR /app

RUN groupadd -r appgroup && useradd -r -g appgroup appuser

COPY --from=builder /app/publish .

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && chown -R appuser:appgroup /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

USER appuser

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD sh -c 'p="${ASPNETCORE_HTTP_PORTS:-8080}"; p="${p%%,*}"; curl -fsS "http://localhost:${p}/health" >/dev/null || exit 1'

ENTRYPOINT ["dotnet", "Simcag.AlertService.Api.dll"]
