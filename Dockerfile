FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY backend/ backend/
RUN dotnet restore backend/src/MyFundex.Api/MyFundex.Api.csproj
RUN dotnet publish backend/src/MyFundex.Api/MyFundex.Api.csproj --configuration Release --no-restore -o /app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "MyFundex.Api.dll"]
