FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj", "src/BuildingBlocks/HospitalMS.BuildingBlocks/"]
COPY ["src/Services/Identity/HospitalMS.Identity.Domain/HospitalMS.Identity.Domain.csproj", "src/Services/Identity/HospitalMS.Identity.Domain/"]
COPY ["src/Services/Identity/HospitalMS.Identity.Application/HospitalMS.Identity.Application.csproj", "src/Services/Identity/HospitalMS.Identity.Application/"]
COPY ["src/Services/Identity/HospitalMS.Identity.Infrastructure/HospitalMS.Identity.Infrastructure.csproj", "src/Services/Identity/HospitalMS.Identity.Infrastructure/"]
COPY ["src/Services/Identity/HospitalMS.Identity.API/HospitalMS.Identity.API.csproj", "src/Services/Identity/HospitalMS.Identity.API/"]
RUN dotnet restore "src/Services/Identity/HospitalMS.Identity.API/HospitalMS.Identity.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Identity/HospitalMS.Identity.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.Identity.API.dll"]
