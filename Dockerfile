# ============================================================================
# Dockerfile multi-stage: compila a API e gera uma imagem enxuta de runtime.
# ============================================================================

# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto primeiro (cache de restore eficiente).
COPY BankPriorityQueueApi.sln ./
COPY src/Domain/Domain.csproj            src/Domain/
COPY src/Application/Application.csproj  src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Api/Api.csproj                  src/Api/
RUN dotnet restore src/Api/Api.csproj

# Copia o resto do código e publica em Release.
COPY . .
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# A API escuta na porta 8080 dentro do container.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]
