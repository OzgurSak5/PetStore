FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PetStore.Domain/PetStore.Domain.csproj PetStore.Domain/
COPY PetStore.Infrastructure/PetStore.Infrastructure.csproj PetStore.Infrastructure/
COPY PetStore.Api/PetStore.Api.csproj PetStore.Api/
RUN dotnet restore PetStore.Api/PetStore.Api.csproj

COPY . .
RUN dotnet publish PetStore.Api/PetStore.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PetStore.Api.dll"]