#!/usr/bin/env bash
set -e

# Railway may not automatically detect the .NET app root, so this script
# explicitly builds and starts the ASP.NET Core API from the PlayerApi folder.
cd PlayerApi

dotnet restore

dotnet publish -c Release -o published

cd published

dotnet PlayerApi.dll
