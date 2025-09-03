# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopiér csproj og restore først (hurtigere Docker cache)
COPY ForHinanden.Api/ForHinanden.Api.csproj ForHinanden.Api/
RUN dotnet restore ForHinanden.Api/ForHinanden.Api.csproj

# Kopiér resten og publish
COPY . .
RUN dotnet publish ForHinanden.Api/ForHinanden.Api.csproj -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render sætter PORT; vi binder til 0.0.0.0:<PORT>
ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Kør app
ENTRYPOINT ["dotnet", "ForHinanden.Api.dll"]
