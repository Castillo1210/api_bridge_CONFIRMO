FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Confirmo.Api/Confirmo.Api.csproj", "src/Confirmo.Api/"]
COPY ["src/Confirmo.Tests/Confirmo.Tests.csproj", "src/Confirmo.Tests/"]
RUN dotnet restore "src/Confirmo.Api/Confirmo.Api.csproj"
COPY . .
WORKDIR "/src/src/Confirmo.Api"
RUN dotnet publish "Confirmo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Confirmo.Api.dll"]