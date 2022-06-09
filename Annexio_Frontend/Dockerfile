FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8081
EXPOSE 453

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Annexio_Frontend/Annexio_Frontend.csproj", "Annexio_Frontend/"]
RUN dotnet restore "Annexio_Frontend/Annexio_Frontend.csproj"
COPY . .
WORKDIR "/src/Annexio_Frontend"
RUN dotnet build "Annexio_Frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Annexio_Frontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Annexio_Frontend.dll"]
