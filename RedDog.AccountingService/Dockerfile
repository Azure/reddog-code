FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RedDog.AccountingService/RedDog.AccountingService.csproj", "RedDog.AccountingService/"]
RUN dotnet restore "RedDog.AccountingService/RedDog.AccountingService.csproj"
COPY . .
WORKDIR "/src/RedDog.AccountingService"
RUN dotnet build "RedDog.AccountingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RedDog.AccountingService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedDog.AccountingService.dll"]
