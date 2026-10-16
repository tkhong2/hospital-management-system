#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${BACKEND_DIR}"

echo "Creating solution HospitalMS.sln..."
dotnet new sln -n HospitalMS --force

echo "Adding BuildingBlocks..."
dotnet sln HospitalMS.sln add src/BuildingBlocks/HospitalMS.BuildingBlocks/HospitalMS.BuildingBlocks.csproj --solution-folder "BuildingBlocks"

SERVICES=("Identity" "Patient" "Catalog" "Outpatient" "Inpatient" "Pharmacy" "HealthExam")
LAYERS=("Domain" "Application" "Infrastructure" "API")

for svc in "${SERVICES[@]}"; do
    echo "Adding Service: ${svc}..."
    for layer in "${LAYERS[@]}"; do
        proj_path="src/Services/${svc}/HospitalMS.${svc}.${layer}/HospitalMS.${svc}.${layer}.csproj"
        if [ -f "$proj_path" ]; then
            dotnet sln HospitalMS.sln add "$proj_path" --solution-folder "Services/${svc}"
        else
            echo "Warning: $proj_path not found!"
        fi
    done
done

echo "Adding ApiGateway..."
dotnet sln HospitalMS.sln add src/ApiGateway/HospitalMS.ApiGateway/HospitalMS.ApiGateway.csproj --solution-folder "ApiGateway"

echo "Solution configured successfully with 30 projects!"

