FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RedDog.OrderService/RedDog.OrderService.csproj", "RedDog.OrderService/"]
RUN dotnet restore "RedDog.OrderService/RedDog.OrderService.csproj"
COPY . .
WORKDIR "/src/RedDog.OrderService"
RUN dotnet build "RedDog.OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RedDog.OrderService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedDog.OrderService.dll"]
