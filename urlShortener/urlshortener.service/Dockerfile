FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
# Stage 1 - Publish the api
# Restore dependencies
COPY urlshortener.service.csproj .
RUN dotnet restore
# Copy all other files and publish the api
COPY . .
RUN dotnet publish -c Release -o ./publish

# Stage 2 - Host the api 
EXPOSE 80
ENTRYPOINT [ "dotnet", "./publish/urlshortener.service.dll" ]