## Red Dog Demo - Azure Application Innovation Scenario

### Background

Microservices can be hard. But, while being exceedingly difficult to architect they have become an increasingly popular architecture pattern. As developers begin to migrate their existing monolithic codebases to a microservices system, they spend a lot of their time dealing with the inherent challenges presented by distributed applications, such as state management and service invocation.

Enter [Dapr](https://www.dapr.io) - The Distributed Application Runtime built with developers in mind. Dapr aims to solve some of these microservice-related challenges by providing consistent building blocks in the form of http/gRPC APIs that can be called natively or using one of the dapr language SDKs.

This comprehensive code repository was created as a resource for software developers who are looking to gain a deeper understanding of how to build cloud-native, distributed applications powered by dapr. The codebase can be run on your local development machine or deployed to a container hosting platform of your choosing. In an effort to help you best leverage the codebase, we have also created a series of examples showcasing how to deploy the app using key services and capabilities of the Azure platform. The deployment options we have developed thus far are detailed in the section below. 

### Deployment Options

Below are some example scenarios for deploying the application. Each scenario is in its own repo.

* [Codespaces "Local" Development](docs/local-dev.md)
* [Hybrid / Arc Deployment](https://github.com/Azure/reddog-hybrid-arc)
* [Container Apps](https://github.com/Azure/reddog-containerapps)
* AKS (coming soon)

### Architecture Diagram and Service Descriptions

The reddog application is developed with .NET and Javascript. As mentioned above, it utilizes Dapr ([Distributed Application Runtime](https://dapr.io)) so it can easily be adapted to multiple scenarios. 

![Logical Application Architecture Diagram](assets/reddog_code.png)


| Service          | Description                                                                                                 |
|------------------|-------------------------------------------------------------------------------------------------------------|                               
| AccountingService | Service used to process, store and aggregate order data, transforming customer orders into meaningful sales metrics that can be showcased via the UI |
| Bootstrapper | A service that leverages Entity Framework Core Migrations to initialize the tables within Azure SQL DB based on the data model found in Reddog.AccountingModel |
| LoyaltyService | Manages the loyalty program by modifying customer reward points based on spend |
| MakeLineService | Responsible for simulating and coordinating a 'queue' of current orders. Monitors the processing and completion of each order in the 'queue' | 
| OrderService | Basic CRUD API that is used to place and manage orders |
| ReceiptGenerationService | Archival program that generates and stores order receipts for auditing and historical purposes  |
| UI | Dashboard showcasing order/sales data related to a single hub location and/or for visibility across Hubs via the Corporate Dashboard |
| VirtualCustomers | 'Customer simulation' program that simulates customers placing orders |
| VirtualWorker | 'Worker simulation' program that simulates the completion of customer orders |
| CorporateTransferService* | Azure Function responsible for monitoring order activity via RabbitMQ i.e. order placement and order completion within the context of a specific hub location and propogating these order activities to an Azure Service Bus for Corporate Hub consumption and visibility |

*These services are specific to the Hybrid retail scenario and may not be applicable for other deployment patterns 

### Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
