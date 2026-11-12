# Etapa 1: compilar o Angular
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npx ng build

# Etapa 2: servir os arquivos com nginx, sem privilégios
FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/clinica-acolher-web/browser /usr/share/nginx/html
EXPOSE 8080