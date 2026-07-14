FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY fcg-shared-events/ fcg-shared-events/
COPY fcg-payments-api/ fcg-payments-api/
WORKDIR /src/fcg-payments-api
RUN dotnet restore src/PaymentsAPI/PaymentsAPI.csproj
RUN dotnet publish src/PaymentsAPI/PaymentsAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentsAPI.dll"]