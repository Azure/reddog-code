FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RedDog.VirtualWorker/RedDog.VirtualWorker.csproj", "RedDog.VirtualWorker/"]
RUN dotnet restore "RedDog.VirtualWorker/RedDog.VirtualWorker.csproj"
COPY . .
WORKDIR "/src/RedDog.VirtualWorker"
RUN dotnet build "RedDog.VirtualWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RedDog.VirtualWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedDog.VirtualWorker.dll"]
