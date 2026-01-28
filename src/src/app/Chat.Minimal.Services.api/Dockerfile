# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/Chat.Minimal.Services/Chat.Minimal.Services.csproj", "Chat.Minimal.Services/"]
RUN dotnet restore "Chat.Minimal.Services/Chat.Minimal.Services.csproj"

# Copy everything else and build
COPY src/Chat.Minimal.Services/. Chat.Minimal.Services/
WORKDIR "/src/Chat.Minimal.Services"
RUN dotnet build "Chat.Minimal.Services.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Chat.Minimal.Services.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Minimal.Services.dll"]
