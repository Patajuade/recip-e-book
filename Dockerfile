# One image: the API also serves the Angular build, so both share one origin (#60).
# Build from the repository root: docker build -t recip-e-book .

# 1. The Angular production build.
FROM node:24 AS web
WORKDIR /source
ENV NX_DAEMON=false
COPY package.json package-lock.json ./
RUN npm ci
# Only the frontend: without the .csproj files, Nx's .NET plugin finds no project and needs no
# .NET SDK in this stage.
COPY nx.json tsconfig.base.json transloco.config.ts ./
COPY apps/web apps/web
COPY libs libs
RUN npx nx build web --configuration=production

# 2. The API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /source
COPY global.json ./
COPY apps/api/RecipEBook.Api.csproj apps/api/
RUN dotnet restore apps/api/RecipEBook.Api.csproj
COPY apps/api apps/api
RUN dotnet publish apps/api/RecipEBook.Api.csproj -c Release -o /app --no-restore

# 3. The runtime image: ASP.NET Core only, no SDK and no Node. It listens on port 8080.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=api /app ./
COPY --from=web /source/dist/apps/web/browser ./wwwroot
# The image runs behind a proxy that ends HTTPS (the Container Apps ingress) and forwards plain
# HTTP with X-Forwarded-Proto and X-Forwarded-For. This makes ASP.NET Core read them: the request
# counts as HTTPS again (both cookies are Secure) and keeps the client's address.
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
# The non-root user of the .NET images.
USER app
ENTRYPOINT ["dotnet", "RecipEBook.Api.dll"]
