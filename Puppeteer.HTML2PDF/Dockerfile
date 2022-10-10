#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Puppeteer.HTML2PDF/Puppeteer.HTML2PDF.csproj", "Puppeteer.HTML2PDF/"]
RUN dotnet restore "Puppeteer.HTML2PDF/Puppeteer.HTML2PDF.csproj"
COPY . .
WORKDIR "/src/Puppeteer.HTML2PDF"
RUN dotnet build "Puppeteer.HTML2PDF.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Puppeteer.HTML2PDF.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Puppeteer.HTML2PDF.dll"]