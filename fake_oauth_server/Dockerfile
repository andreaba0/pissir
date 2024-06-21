from node:20.11-alpine

WORKDIR /app

COPY package.json /app
COPY package-lock.json /app

RUN npm install

COPY cache.js /app
COPY index.js /app

EXPOSE 8000

ENTRYPOINT ["node", "index.js"]