FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the main project file first and restore
COPY TheRocksNew.API.csproj .
RUN dotnet restore TheRocksNew.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish TheRocksNew.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "TheRocksNew.API.dll"]