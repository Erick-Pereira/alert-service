FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Simcag.AlertService.Domain/Simcag.AlertService.Domain.csproj", "Simcag.AlertService.Domain/"]
COPY ["Simcag.AlertService.Application/Simcag.AlertService.Application.csproj", "Simcag.AlertService.Application/"]
COPY ["Simcag.AlertService.Infrastructure/Simcag.AlertService.Infrastructure.csproj", "Simcag.AlertService.Infrastructure/"]
COPY ["Simcag.AlertService.Api/Simcag.AlertService.Api.csproj", "Simcag.AlertService.Api/"]
COPY ["shared/shared.csproj", "shared/"]

RUN dotnet restore "Simcag.AlertService.Api/Simcag.AlertService.Api.csproj"

COPY . .
WORKDIR "/src/Simcag.AlertService.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Simcag.AlertService.Api.dll"]