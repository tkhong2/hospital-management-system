FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj", "src/BuildingBlocks/HospitalMS.BuildingBlocks/"]
COPY ["src/Services/Catalog/HospitalMS.Catalog.Domain/HospitalMS.Catalog.Domain.csproj", "src/Services/Catalog/HospitalMS.Catalog.Domain/"]
COPY ["src/Services/Catalog/HospitalMS.Catalog.Application/HospitalMS.Catalog.Application.csproj", "src/Services/Catalog/HospitalMS.Catalog.Application/"]
COPY ["src/Services/Catalog/HospitalMS.Catalog.Infrastructure/HospitalMS.Catalog.Infrastructure.csproj", "src/Services/Catalog/HospitalMS.Catalog.Infrastructure/"]
COPY ["src/Services/Catalog/HospitalMS.Catalog.API/HospitalMS.Catalog.API.csproj", "src/Services/Catalog/HospitalMS.Catalog.API/"]
RUN dotnet restore "src/Services/Catalog/HospitalMS.Catalog.API/HospitalMS.Catalog.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Catalog/HospitalMS.Catalog.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.Catalog.API.dll"]
