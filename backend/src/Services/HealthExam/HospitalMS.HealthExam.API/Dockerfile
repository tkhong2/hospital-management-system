FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj", "src/BuildingBlocks/HospitalMS.BuildingBlocks/"]
COPY ["src/Services/HealthExam/HospitalMS.HealthExam.Domain/HospitalMS.HealthExam.Domain.csproj", "src/Services/HealthExam/HospitalMS.HealthExam.Domain/"]
COPY ["src/Services/HealthExam/HospitalMS.HealthExam.Application/HospitalMS.HealthExam.Application.csproj", "src/Services/HealthExam/HospitalMS.HealthExam.Application/"]
COPY ["src/Services/HealthExam/HospitalMS.HealthExam.Infrastructure/HospitalMS.HealthExam.Infrastructure.csproj", "src/Services/HealthExam/HospitalMS.HealthExam.Infrastructure/"]
COPY ["src/Services/HealthExam/HospitalMS.HealthExam.API/HospitalMS.HealthExam.API.csproj", "src/Services/HealthExam/HospitalMS.HealthExam.API/"]
RUN dotnet restore "src/Services/HealthExam/HospitalMS.HealthExam.API/HospitalMS.HealthExam.API.csproj"

COPY . .
WORKDIR "/src/src/Services/HealthExam/HospitalMS.HealthExam.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.HealthExam.API.dll"]
