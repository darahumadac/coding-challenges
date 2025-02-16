# Stage 1 - Build the frontend
FROM node:23-alpine AS build
# Set the working directory
WORKDIR /app
# Add layer to Install dependencies
COPY package.json .
RUN npm install
# Copy the rest of the files and build the frontend
COPY . .
RUN npm run build

FROM nginx:1.27.4-alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
# use the entrypoint and cmd provided by nginx image