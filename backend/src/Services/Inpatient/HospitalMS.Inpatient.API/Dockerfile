FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj", "src/BuildingBlocks/HospitalMS.BuildingBlocks/"]
COPY ["src/Services/Inpatient/HospitalMS.Inpatient.Domain/HospitalMS.Inpatient.Domain.csproj", "src/Services/Inpatient/HospitalMS.Inpatient.Domain/"]
COPY ["src/Services/Inpatient/HospitalMS.Inpatient.Application/HospitalMS.Inpatient.Application.csproj", "src/Services/Inpatient/HospitalMS.Inpatient.Application/"]
COPY ["src/Services/Inpatient/HospitalMS.Inpatient.Infrastructure/HospitalMS.Inpatient.Infrastructure.csproj", "src/Services/Inpatient/HospitalMS.Inpatient.Infrastructure/"]
COPY ["src/Services/Inpatient/HospitalMS.Inpatient.API/HospitalMS.Inpatient.API.csproj", "src/Services/Inpatient/HospitalMS.Inpatient.API/"]
RUN dotnet restore "src/Services/Inpatient/HospitalMS.Inpatient.API/HospitalMS.Inpatient.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Inpatient/HospitalMS.Inpatient.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.Inpatient.API.dll"]
