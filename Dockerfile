# Use the official image for .NET
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["FinancialApp/FinancialApp.csproj", "FinancialApp/"]

# Restore dependencies
RUN dotnet restore "FinancialApp/FinancialApp.csproj"

# Copy the rest of the application and publish it
COPY ./FinancialApp ./FinancialApp
WORKDIR "/src/FinancialApp"
RUN dotnet publish "FinancialApp.csproj" -c Release -o /app/publish

# Set the base image to run the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinancialApp.dll"]
