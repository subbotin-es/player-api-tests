FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PlayerApi/PlayerApi.csproj PlayerApi/
RUN dotnet restore PlayerApi/PlayerApi.csproj

COPY . .
WORKDIR /src/PlayerApi
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "PlayerApi.dll"]
