# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["EducationalTimeManagement.Api.csproj", "./"]
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create directory for SQLite database
RUN mkdir -p /app/data

COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DatabaseProvider=Sqlite
ENV ConnectionStrings__SqliteConnection="Data Source=/app/data/app.db"

EXPOSE 5000

ENTRYPOINT ["dotnet", "EducationalTimeManagement.Api.dll"]
