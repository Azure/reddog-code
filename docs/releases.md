## Release Schedule

### Milestone v1.1

Target: 11/17/2021

To do list:
* Incorporate updates for Hub/Corp deployment
    * KeyVault cert and AKS secret
    * KeyVault secrets
    * Arc GitOps dependencies/app
    * Azure SQL config
    * UI deployment in App Svc
    * APIM for Hub
* Automate Azure Function deploy (Corp Transfer Service)
    * Add RabbitMQ queues/bindings
* Fix Lima deployment on Rancher K3s
* Branch API Management
    * Add to APIM to Bicep
    * Arc Enabled APIM gateway
    * Configure branch API's
* Create function for mobile orders created at Corp (Corp -> Store)
* Connect Mobile Apps to Corp order service (via APIM)
* Slide Deck, Videos, etc. - covering the business side and some of the technical decisions we have made


### Milestone vNext

Futures:
* Complete Documentation
* Deployment scenarios
    * Container Apps
    * AKS
    * Azure Stack HCI 
* Event Grid
* Azure ML
