FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["beautix_bisp_17005.csproj", "."]
RUN dotnet restore "./beautix_bisp_17005.csproj"
COPY . .
RUN dotnet build "beautix_bisp_17005.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "beautix_bisp_17005.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet beautix_bisp_17005.dll --urls http://+:${PORT:-8080}"]