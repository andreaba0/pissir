from node:20.11-alpine

WORKDIR /app

COPY package.json /app
COPY package-lock.json /app

COPY index.js /app

RUN npm install

EXPOSE 8000

CMD ["node", "index.js"]