from node:16-alpine as builder

# Create app directory
WORKDIR /usr/src/app

copy package*.json ./
copy *.js ./
copy *.pem ./

RUN npm install

cmd ["node", "index.js"]