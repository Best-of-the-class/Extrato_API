# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Extrato_API/Extrato_API.csproj", "Extrato_API/"]
RUN dotnet restore "Extrato_API/Extrato_API.csproj"

COPY . .
WORKDIR "/src/Extrato_API"
RUN dotnet publish "Extrato_API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Extrato_API.dll"]
