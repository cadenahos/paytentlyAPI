FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PaytentlyAPI/PaytentlyTestGateway.csproj", "./"]
RUN dotnet restore "PaytentlyTestGateway.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PaytentlyAPI/PaytentlyTestGateway.csproj" -c Release -o /app/build

# Test stage
FROM build AS test
WORKDIR /src
COPY ["PaytentlyAPI.Tests/", "./PaytentlyAPI.Tests/"]
RUN dotnet test "PaytentlyAPI.Tests/PaytentlyAPI.Tests.csproj" -c Release

FROM build AS publish
RUN dotnet publish "PaytentlyAPI/PaytentlyTestGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaytentlyTestGateway.dll"] 