# syntax=docker/dockerfile:1

# ---- Build stage: the full .NET SDK (compiler, NuGet). It is thrown away; only its output is kept.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore with only the project files first: Docker caches this layer until a .csproj changes,
# so editing code does not download every NuGet package again.
COPY src/ClinicBooking.Domain/ClinicBooking.Domain.csproj src/ClinicBooking.Domain/
COPY src/ClinicBooking.Application/ClinicBooking.Application.csproj src/ClinicBooking.Application/
COPY src/ClinicBooking.Infrastructure/ClinicBooking.Infrastructure.csproj src/ClinicBooking.Infrastructure/
COPY src/ClinicBooking.Api/ClinicBooking.Api.csproj src/ClinicBooking.Api/
RUN dotnet restore src/ClinicBooking.Api/ClinicBooking.Api.csproj

COPY src/ src/
RUN dotnet publish src/ClinicBooking.Api/ClinicBooking.Api.csproj -c Release -o /app --no-restore

# ---- Runtime stage: just the ASP.NET Core runtime plus the published app.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# The official .NET images ship a non-root user; the app never needs root.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicBooking.Api.dll"]
