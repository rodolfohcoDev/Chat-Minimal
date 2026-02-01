FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y \
    libgomp1 \
    libstdc++6 \
    libc6 \
    && rm -rf /var/lib/apt/lists/*
ENV LD_LIBRARY_PATH="/app:/app/runtimes/linux-x64/native:${LD_LIBRARY_PATH}"
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/app/Chat.Minimal.Services.api/Chat.Minimal.Services.api.csproj", "src/app/Chat.Minimal.Services.api/"]
COPY ["src/app/Chat.Minimal.IAs.Services/Chat.Minimal.IAs.Services.csproj", "src/app/Chat.Minimal.IAs.Services/"]
RUN dotnet restore "src/app/Chat.Minimal.Services.api/Chat.Minimal.Services.api.csproj"
COPY . .
WORKDIR "/src/src/app/Chat.Minimal.Services.api"
RUN dotnet build "Chat.Minimal.Services.api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Chat.Minimal.Services.api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Minimal.Services.api.dll"]
