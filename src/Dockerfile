FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y libgomp1 && rm -rf /var/lib/apt/lists/*
ENV LD_LIBRARY_PATH="/app:/app/runtimes/linux-x64/native:${LD_LIBRARY_PATH}"
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Chat.Minimal.Services/Chat.Minimal.Services.csproj", "src/Chat.Minimal.Services/"]
COPY ["src/Chat.Minimal.IAs.Services/Chat.Minimal.IAs.Services.csproj", "src/Chat.Minimal.IAs.Services/"]
RUN dotnet restore "src/Chat.Minimal.Services/Chat.Minimal.Services.csproj"
COPY . .
WORKDIR "/src/src/Chat.Minimal.Services"
RUN dotnet build "Chat.Minimal.Services.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Chat.Minimal.Services.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Minimal.Services.dll"]
