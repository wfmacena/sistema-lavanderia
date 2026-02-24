FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY SistemaLavanderia.Web.csproj .

# Restaurar dependências
RUN dotnet restore

# Copiar todo o código fonte
COPY . .

# Publicar a aplicação
RUN dotnet publish -c Release -o /app/publish

# Imagem final
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SistemaLavanderia.Web.dll"]