# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Healthcare.Api/Healthcare.Api.csproj", "src/Healthcare.Api/"]
RUN dotnet restore "src/Healthcare.Api/Healthcare.Api.csproj"

COPY . .

RUN dotnet publish "src/Healthcare.Api/Healthcare.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/Uploads

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Healthcare.Api.dll"]

