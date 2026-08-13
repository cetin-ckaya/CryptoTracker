# 1. Build Aşaması (.NET SDK ile derleme)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve restore et
COPY *.csproj ./
RUN dotnet restore

# Tüm kodları kopyala ve publish et
COPY . .
RUN dotnet publish -c Release -o /app/out

# 2. Çalıştırma Aşaması (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Render'ın kullandığı port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CryptoTracker.dll"]