# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/node:3.0-appservice
FROM mcr.microsoft.com/azure-functions/node:3.0

# Label for GHCR
LABEL org.opencontainers.image.source='https://github.com/cloudnativegbb/paas-vnext'
LABEL org.opencontainers.image.url='https://github.com/cloudnativegbb/paas-vnext'

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY . /home/site/wwwroot

RUN cd /home/site/wwwroot && \
    npm install
