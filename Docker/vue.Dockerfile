FROM node:lts
RUN npm install -g http-server
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build
EXPOSE 8000
CMD [ "http-server", "dist", "-a", "0.0.0.0" ]
