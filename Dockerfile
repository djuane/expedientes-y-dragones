# Servidor de partidas de Expedientes y Dragones
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS compilar
WORKDIR /src
COPY logica/Expedientia.Logica/ logica/Expedientia.Logica/
COPY servidor/Expedientia.Servidor/ servidor/Expedientia.Servidor/
RUN dotnet publish servidor/Expedientia.Servidor/Expedientia.Servidor.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=compilar /app .
COPY contenido/ /contenido/
ENV ASPNETCORE_URLS=http://+:8080 \
    Juego__CarpetaPreguntas=/contenido/preguntas
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Expedientia.Servidor.dll"]
