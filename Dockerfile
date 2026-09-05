# Multi-stage build for .NET 10 Web Application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-level props and project files for layer caching
COPY ["Directory.Build.props", "./"]
COPY ["src/BusinessManagement.Web/BusinessManagement.Web.csproj", "src/BusinessManagement.Web/"]
COPY ["src/BusinessManagement.Application/BusinessManagement.Application.csproj", "src/BusinessManagement.Application/"]
COPY ["src/BusinessManagement.Domain/BusinessManagement.Domain.csproj", "src/BusinessManagement.Domain/"]
COPY ["src/BusinessManagement.Infrastructure/BusinessManagement.Infrastructure.csproj", "src/BusinessManagement.Infrastructure/"]
COPY ["src/BusinessManagement.Persistence/BusinessManagement.Persistence.csproj", "src/BusinessManagement.Persistence/"]
COPY ["src/BusinessManagement.Shared/BusinessManagement.Shared.csproj", "src/BusinessManagement.Shared/"]

# Restore packages
RUN dotnet restore "src/BusinessManagement.Web/BusinessManagement.Web.csproj"

# Copy the rest of the application files
COPY . .

# Build and publish release output
WORKDIR "/src/src/BusinessManagement.Web"
RUN dotnet publish "BusinessManagement.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BusinessManagement.Web.dll"]
