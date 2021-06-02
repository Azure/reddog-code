FROM node:14.15.4-alpine

LABEL org.opencontainers.image.source='https://github.com/cloudnativegbb/paas-vnext'
LABEL org.opencontainers.image.url='https://github.com/cloudnativegbb/paas-vnext'

WORKDIR /usr/src/app
COPY . .
WORKDIR "/usr/src/app/RedDog.UI"
RUN npm ci
RUN npm run build

ENV NODE_ENV "production"
ENV VUE_APP_IS_CORP true
ENV VUE_APP_STORE_ID "Austin"
ENV VUE_APP_SITE_TYPE "Pharmacia"
ENV VUE_APP_SITE_TITLE "Contoso :: Pharmacy & Bodega"
ENV VUE_APP_MAKELINE_BASE_URL "http://austin.makeline.brianredmond.io"
ENV VUE_APP_ACCOUNTING_BASE_URL "http://austin.accounting.brianredmond.io"

EXPOSE 8080

CMD [ "npm", "run", "container" ]
