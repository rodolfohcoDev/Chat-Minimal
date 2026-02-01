# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Chat.Minimal.Services.api/Chat.Minimal.Services.api.csproj", "Chat.Minimal.Services.api/"]
COPY ["Chat.Minimal.IAs.Services/Chat.Minimal.IAs.Services.csproj", "Chat.Minimal.IAs.Services/"]
RUN dotnet restore "Chat.Minimal.Services.api/Chat.Minimal.Services.api.csproj"

# Copy everything else and build
COPY Chat.Minimal.Services.api/. Chat.Minimal.Services.api/
COPY Chat.Minimal.IAs.Services/. Chat.Minimal.IAs.Services/
WORKDIR "/src/Chat.Minimal.Services.api"
RUN dotnet build "Chat.Minimal.Services.api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Chat.Minimal.Services.api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Minimal.Services.api.dll"]
