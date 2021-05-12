FROM node:14.15.4-alpine

LABEL org.opencontainers.image.source='https://github.com/cloudnativegbb/paas-vnext'
LABEL org.opencontainers.image.url='https://github.com/cloudnativegbb/paas-vnext'

WORKDIR /usr/src/app
COPY . .
WORKDIR "/usr/src/app/RedDog.UI"
RUN npm ci
RUN npm run build

ENV NODE_ENV "production"
EXPOSE 8080

CMD [ "npm", "run", "container" ]
