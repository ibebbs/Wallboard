FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Wallboard/Wallboard.csproj", "Wallboard/"]
RUN dotnet restore "Wallboard/Wallboard.csproj"
COPY . .
WORKDIR "/src/Wallboard"
RUN dotnet build "Wallboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wallboard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wallboard.dll"]