FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj", "src/BuildingBlocks/HospitalMS.BuildingBlocks/"]
COPY ["src/Services/Outpatient/HospitalMS.Outpatient.Domain/HospitalMS.Outpatient.Domain.csproj", "src/Services/Outpatient/HospitalMS.Outpatient.Domain/"]
COPY ["src/Services/Outpatient/HospitalMS.Outpatient.Application/HospitalMS.Outpatient.Application.csproj", "src/Services/Outpatient/HospitalMS.Outpatient.Application/"]
COPY ["src/Services/Outpatient/HospitalMS.Outpatient.Infrastructure/HospitalMS.Outpatient.Infrastructure.csproj", "src/Services/Outpatient/HospitalMS.Outpatient.Infrastructure/"]
COPY ["src/Services/Outpatient/HospitalMS.Outpatient.API/HospitalMS.Outpatient.API.csproj", "src/Services/Outpatient/HospitalMS.Outpatient.API/"]
RUN dotnet restore "src/Services/Outpatient/HospitalMS.Outpatient.API/HospitalMS.Outpatient.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Outpatient/HospitalMS.Outpatient.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.Outpatient.API.dll"]
