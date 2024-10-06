FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5136

ENV ASPNETCORE_URLS=http://+:5136

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["UserAuthenticationWebApi2.csproj", "./"]
RUN dotnet restore "UserAuthenticationWebApi2.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "UserAuthenticationWebApi2.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "UserAuthenticationWebApi2.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserAuthenticationWebApi2.dll"]
