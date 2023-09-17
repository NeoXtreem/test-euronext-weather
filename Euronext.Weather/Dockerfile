FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Euronext.Weather/Euronext.Weather.csproj", "Euronext.Weather/"]
RUN dotnet restore "Euronext.Weather/Euronext.Weather.csproj"
COPY . .
WORKDIR "/src/Euronext.Weather"
RUN dotnet build "Euronext.Weather.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Euronext.Weather.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Euronext.Weather.dll"]