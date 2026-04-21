FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore
COPY ["GarbageCollection.sln", "./"]
COPY ["GarbageCollection.API/GarbageCollection.API.csproj", "GarbageCollection.API/"]
COPY ["GarbageCollection.Business/GarbageCollection.Business.csproj", "GarbageCollection.Business/"]
COPY ["GarbageCollection.Common/GarbageCollection.Common.csproj", "GarbageCollection.Common/"]
COPY ["GarbageCollection.DataAccess/GarbageCollection.DataAccess.csproj", "GarbageCollection.DataAccess/"]
RUN dotnet restore

# Copy all the source code
COPY . .

# Build and Publish
WORKDIR "/src/GarbageCollection.API"
RUN dotnet publish "GarbageCollection.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Railway dynamically provides PORT environment variable, ASP.NET Core 8 automatically listens on it)
ENTRYPOINT ["dotnet", "GarbageCollection.API.dll"]
