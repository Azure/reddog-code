FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RedDog.ReceiptGenerationService/RedDog.ReceiptGenerationService.csproj", "RedDog.ReceiptGenerationService/"]
RUN dotnet restore "RedDog.ReceiptGenerationService/RedDog.ReceiptGenerationService.csproj"
COPY . .
WORKDIR "/src/RedDog.ReceiptGenerationService"
RUN dotnet build "RedDog.ReceiptGenerationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RedDog.ReceiptGenerationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedDog.ReceiptGenerationService.dll"]
