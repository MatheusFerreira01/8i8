FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["8i8.Api/8i8.Api.csproj", "8i8.Api/"]
COPY ["8i8.Domain/8i8.Domain.csproj", "8i8.Domain/"]
COPY ["8i8.Infrastructure/8i8.Infrastructure.csproj", "8i8.Infrastructure/"]
COPY ["8i8.Application/8i8.Application.csproj", "8i8.Application/"]
COPY ["8i8.Contracts/8i8.Contracts.csproj", "8i8.Contracts/"]
COPY ["8i8.Shared/8i8.Shared.csproj", "8i8.Shared/"]
RUN dotnet restore "8i8.Api/8i8.Api.csproj"
COPY . .
WORKDIR "/src/8i8.Api"
RUN dotnet publish "8i8.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "8i8.Api.dll"]
