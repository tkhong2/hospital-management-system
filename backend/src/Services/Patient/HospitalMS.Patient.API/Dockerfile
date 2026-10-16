FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj", "src/BuildingBlocks/HospitalMS.BuildingBlocks/"]
COPY ["src/Services/Patient/HospitalMS.Patient.Domain/HospitalMS.Patient.Domain.csproj", "src/Services/Patient/HospitalMS.Patient.Domain/"]
COPY ["src/Services/Patient/HospitalMS.Patient.Application/HospitalMS.Patient.Application.csproj", "src/Services/Patient/HospitalMS.Patient.Application/"]
COPY ["src/Services/Patient/HospitalMS.Patient.Infrastructure/HospitalMS.Patient.Infrastructure.csproj", "src/Services/Patient/HospitalMS.Patient.Infrastructure/"]
COPY ["src/Services/Patient/HospitalMS.Patient.API/HospitalMS.Patient.API.csproj", "src/Services/Patient/HospitalMS.Patient.API/"]
RUN dotnet restore "src/Services/Patient/HospitalMS.Patient.API/HospitalMS.Patient.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Patient/HospitalMS.Patient.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.Patient.API.dll"]
