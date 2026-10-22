# syntax=docker/dockerfile:1

# ---- Build stage: Node compiles the Angular app into static files. It is thrown away afterwards.
FROM node:24-alpine AS build
WORKDIR /app

# Install dependencies with only the manifests first, so this layer stays cached until they change.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---- Runtime stage: nginx serves the static files and forwards /api to the backend container.
# The "unprivileged" variant runs as a non-root user and listens on 8080.
FROM nginxinc/nginx-unprivileged:1.29-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
EXPOSE 8080
