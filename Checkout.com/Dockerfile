#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["./Checkout.com.csproj", "Checkout.com/"]
RUN dotnet restore "Checkout.com/Checkout.com.csproj"
COPY . .
WORKDIR "/src/Checkout.com"
RUN dotnet build "Checkout.com.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Checkout.com.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Checkout.com.dll"]