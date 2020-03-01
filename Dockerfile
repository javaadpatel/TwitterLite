#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["TwitterLite.Web/TwitterLite.Web.csproj", "TwitterLite.Web/"]
#COPY ["TwitterLite.Web.csproj", "TwitterLite.Web/"]
RUN dotnet restore "TwitterLite.Web/TwitterLite.Web.csproj"
COPY . .
WORKDIR "/src/TwitterLite.Web"
RUN dotnet build "TwitterLite.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TwitterLite.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TwitterLite.Web.dll"]