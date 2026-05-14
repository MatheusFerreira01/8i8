# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

# Copia todos os .csproj antes para aproveitar cache do restore
COPY ["8i8.Api/8i8.Api.csproj",                       "8i8.Api/"]
COPY ["8i8.Application/8i8.Application.csproj",       "8i8.Application/"]
COPY ["8i8.Contracts/8i8.Contracts.csproj",           "8i8.Contracts/"]
COPY ["8i8.Domain/8i8.Domain.csproj",                 "8i8.Domain/"]
COPY ["8i8.Infrastructure/8i8.Infrastructure.csproj", "8i8.Infrastructure/"]
COPY ["8i8.Shared/8i8.Shared.csproj",                 "8i8.Shared/"]

RUN dotnet restore "./8i8.Api/8i8.Api.csproj"

COPY . .

WORKDIR "/src/8i8.Api"

RUN dotnet build "./8i8.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug

RUN dotnet publish "./8i8.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "8i8.Api.dll"]
