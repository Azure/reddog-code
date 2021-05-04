FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RedDog.VirtualCustomers/RedDog.VirtualCustomers.csproj", "RedDog.VirtualCustomers/"]
RUN dotnet restore "RedDog.VirtualCustomers/RedDog.VirtualCustomers.csproj"
COPY . .
WORKDIR "/src/RedDog.VirtualCustomers"
RUN dotnet build "RedDog.VirtualCustomers.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RedDog.VirtualCustomers.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedDog.VirtualCustomers.dll"]
