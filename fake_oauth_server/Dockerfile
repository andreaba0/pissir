from node:20.11-alpine

WORKDIR /app

COPY package.json /app
COPY package-lock.json /app

COPY cache.js /app
COPY index.js /app

RUN npm install

EXPOSE 8000

ENTRYPOINT ["node", "index.js"]