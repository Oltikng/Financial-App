# Use the official image for .NET Core SDK as the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FinancialApp/FinancialApp.csproj", "FinancialApp/"]
RUN dotnet restore "FinancialApp/FinancialApp.csproj"
COPY . .
WORKDIR "/src/FinancialApp"
RUN dotnet build "FinancialApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinancialApp.csproj" -c Release -o /app/publish

# Copy the app from the build container to the final container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinancialApp.dll"]
