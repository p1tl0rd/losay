FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["LoSay.csproj", "."]
RUN dotnet restore "./LoSay.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./LoSay.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./LoSay.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoSay.dll"]