FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de solução e projetos
COPY SistemaLavanderia.Core/SistemaLavanderia.Core.csproj SistemaLavanderia.Core/
COPY SistemaLavanderia.Infrastructure/SistemaLavanderia.Infrastructure.csproj SistemaLavanderia.Infrastructure/
COPY SistemaLavanderia.Web/SistemaLavanderia.Web.csproj SistemaLavanderia.Web/

# Restaurar dependências
RUN dotnet restore SistemaLavanderia.Web/SistemaLavanderia.Web.csproj

# Copiar todo o código fonte
COPY SistemaLavanderia.Core/ SistemaLavanderia.Core/
COPY SistemaLavanderia.Infrastructure/ SistemaLavanderia.Infrastructure/
COPY SistemaLavanderia.Web/ SistemaLavanderia.Web/

# Publicar a aplicação
RUN dotnet publish SistemaLavanderia.Web/SistemaLavanderia.Web.csproj -c Release -o /app/publish

# Imagem final
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SistemaLavanderia.Web.dll"]