# =====================================================================
# Roteiro 8 — imagem da API em DOIS ESTÁGIOS (multi-stage).
# Imagens base com versão fixa (10.0), nunca "latest": o mesmo Dockerfile gera a mesma imagem.
# =====================================================================

# ---------- Estágio 1: BUILD — imagem com o SDK (grande), só para compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primeiro só o .csproj + restore: esta camada fica em cache enquanto os pacotes não mudam
COPY PortalTarefas.Web/PortalTarefas.Web.csproj PortalTarefas.Web/
RUN dotnet restore PortalTarefas.Web/PortalTarefas.Web.csproj

# Depois o código, e o publish (a versão "de produção", como a dist/ do front-end)
COPY PortalTarefas.Web/ PortalTarefas.Web/
RUN dotnet publish PortalTarefas.Web/PortalTarefas.Web.csproj -c Release -o /app/publish \
    --no-restore -p:OpenApiGenerateDocuments=false

# ---------- Estágio 2: EXECUÇÃO — só o runtime do ASP.NET (pequena), sem SDK nem código-fonte ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Pasta do banco SQLite, gravável pelo usuário sem privilégios
RUN mkdir /app/data && chown $APP_UID /app/data

COPY --from=build /app/publish .

# Versão da imagem (o commit), informada no build: docker build --build-arg VERSAO=...
ARG VERSAO=local
ENV APP_VERSAO=$VERSAO

# Usuário NÃO privilegiado ("app"), que já existe nas imagens do .NET
USER $APP_UID

# Porta padrão das imagens do ASP.NET. Health Check: /health/live e /health/ready
EXPOSE 8080
ENTRYPOINT ["dotnet", "PortalTarefas.Web.dll"]