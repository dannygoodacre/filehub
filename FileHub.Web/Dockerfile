FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 3000
EXPOSE 3001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FileHub.Web/FileHub.Web.csproj", "FileHub.Web/"]
RUN dotnet restore "FileHub.Web/FileHub.Web.csproj"
COPY . .
WORKDIR "/src/FileHub.Web"
RUN dotnet build "FileHub.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileHub.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/data /app/files /app/certs

ENTRYPOINT ["dotnet", "FileHub.Web.dll"]
