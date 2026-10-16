FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj", "src/BuildingBlocks/HospitalMS.BuildingBlocks/"]
COPY ["src/Services/Pharmacy/HospitalMS.Pharmacy.Domain/HospitalMS.Pharmacy.Domain.csproj", "src/Services/Pharmacy/HospitalMS.Pharmacy.Domain/"]
COPY ["src/Services/Pharmacy/HospitalMS.Pharmacy.Application/HospitalMS.Pharmacy.Application.csproj", "src/Services/Pharmacy/HospitalMS.Pharmacy.Application/"]
COPY ["src/Services/Pharmacy/HospitalMS.Pharmacy.Infrastructure/HospitalMS.Pharmacy.Infrastructure.csproj", "src/Services/Pharmacy/HospitalMS.Pharmacy.Infrastructure/"]
COPY ["src/Services/Pharmacy/HospitalMS.Pharmacy.API/HospitalMS.Pharmacy.API.csproj", "src/Services/Pharmacy/HospitalMS.Pharmacy.API/"]
RUN dotnet restore "src/Services/Pharmacy/HospitalMS.Pharmacy.API/HospitalMS.Pharmacy.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Pharmacy/HospitalMS.Pharmacy.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.Pharmacy.API.dll"]
