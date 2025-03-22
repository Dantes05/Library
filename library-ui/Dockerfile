
FROM node:18 AS build
WORKDIR /app


COPY library-ui/package*.json ./


RUN npm install


COPY library-ui/ .


RUN npm run build


FROM nginx:alpine
WORKDIR /usr/share/nginx/html


COPY --from=build /app/build .


EXPOSE 80


CMD ["nginx", "-g", "daemon off;"]