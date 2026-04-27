# Build context: raiz do repositório
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["shared/shared.csproj", "shared/"]
COPY ["alert-service/Simcag.AlertService.Domain/Simcag.AlertService.Domain.csproj", "alert-service/Simcag.AlertService.Domain/"]
COPY ["alert-service/Simcag.AlertService.Application/Simcag.AlertService.Application.csproj", "alert-service/Simcag.AlertService.Application/"]
COPY ["alert-service/Simcag.AlertService.Infrastructure/Simcag.AlertService.Infrastructure.csproj", "alert-service/Simcag.AlertService.Infrastructure/"]
COPY ["alert-service/Simcag.AlertService.Api/Simcag.AlertService.Api.csproj", "alert-service/Simcag.AlertService.Api/"]
RUN dotnet restore "alert-service/Simcag.AlertService.Api/Simcag.AlertService.Api.csproj"
COPY shared/ shared/
COPY alert-service/ alert-service/
RUN dotnet publish "alert-service/Simcag.AlertService.Api/Simcag.AlertService.Api.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Simcag.AlertService.Api.dll"]
