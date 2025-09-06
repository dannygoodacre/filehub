FROM node:22-alpine AS builder

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

FROM node:22-alpine AS runner

WORKDIR /app

COPY package*.json ./

RUN npm install --only=production && npm cache clean --force

COPY --from=builder /app/build ./build

EXPOSE 3001

ENV NODE_ENV=production

CMD ["node", "build"]
