# Etapa 1: compilar, com o SDK completo
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ClinicaAcolher.slnx ./
COPY src/ClinicaAcolher.Domain/ClinicaAcolher.Domain.csproj src/ClinicaAcolher.Domain/
COPY src/ClinicaAcolher.Application/ClinicaAcolher.Application.csproj src/ClinicaAcolher.Application/
COPY src/ClinicaAcolher.Infrastructure/ClinicaAcolher.Infrastructure.csproj src/ClinicaAcolher.Infrastructure/
COPY src/ClinicaAcolher.Api/ClinicaAcolher.Api.csproj src/ClinicaAcolher.Api/
RUN dotnet restore src/ClinicaAcolher.Api/ClinicaAcolher.Api.csproj

COPY src/ src/
RUN dotnet publish src/ClinicaAcolher.Api/ClinicaAcolher.Api.csproj -c Release -o /app --no-restore

# Etapa 2: rodar, só com o necessário
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicaAcolher.Api.dll"]