FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .

COPY LibraryApp/LibraryApp.csproj ./LibraryApp/
COPY Application/Application.csproj ./Application/
COPY Domain/Domain.csproj ./Domain/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY LibraryTests/LibraryTests.csproj ./LibraryTests/

RUN dotnet restore

COPY . .

WORKDIR /src/LibraryApp
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .
COPY wait-for-db.sh /wait-for-db.sh
RUN chmod +x /wait-for-db.sh

EXPOSE 80

ENTRYPOINT ["/wait-for-db.sh", "dotnet", "LibraryApp.dll"]