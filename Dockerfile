FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ALL project files and restore
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build SPECIFIC project
COPY . .
RUN dotnet publish TheRocksNew.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "TheRocksNew.API.dll"]