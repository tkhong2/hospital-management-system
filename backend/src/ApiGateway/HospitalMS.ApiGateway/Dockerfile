FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/ApiGateway/HospitalMS.ApiGateway/HospitalMS.ApiGateway.csproj", "src/ApiGateway/HospitalMS.ApiGateway/"]
RUN dotnet restore "src/ApiGateway/HospitalMS.ApiGateway/HospitalMS.ApiGateway.csproj"

COPY . .
WORKDIR "/src/src/ApiGateway/HospitalMS.ApiGateway"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HospitalMS.ApiGateway.dll"]
