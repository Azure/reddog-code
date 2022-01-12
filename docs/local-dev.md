## Overview

One of the nice things about Dapr is the ability to make the transition from executing against different backing services without ever having to change a line of code.  This is especially important if you would like to run completely locally without any reliance on cloud-based services.  You will notice in the `/manifests/branch/local` location that a number of dapr configs have been provided.  These dapr configs will allow you to run each of the dapr-ized services in a "local" manner without relying on any cloud-based services.

With that said, there are a few short steps you will need to perform in order to set up your local development environment.  The instructions below will guide you through setting up a GitHub Codespace and subsequently running one of the Reddog dapr services.


## Create Codespace

1. Browse to the Codespaces page in the repo: https://github.com/Azure/reddog-code/codespaces
2. Select "New codespace"
3. On the "master" branch, select "Create codespace"
4. Pick a size for your Codespace environment
5. Wait for the environment to be ready

Once complete, VS Code will be running in your browser with the master branch cloned.


## Setup Reddog Environment

1. In a VS Code terminal window, run the following to trust local certs:
`dotnet dev-certs https --trust`
2. Start the Bootstrapper Dapr sidecar by doing the following:
    1. Open the Command Palette
    2. Select `Tasks: Run Task`
    3. Select `Dapr Bootstrapper`
3. Set an environment variable for the Bootstrapper's Dapr HTTP Port
```
DAPR_HTTP_PORT=5880
```
5. Switch to the Run and Debug screen and debug the Bootstrapper.  Upon completion, you should now have a "reddogdemo" database in the given SQL Server instance.

>The Accounting Service within Reddog relies on SQL Server for persistent storage.  As such, you will notice that the `.devcontainer` configuration for Codespaces (located withing the `.devcontainer` folder) points at a Docker compose file that includes a container image reference to SQL Server.  While the image will be pulled into your Codespace for you, the database itself will still need to be provisioned.  Included in the Reddog repo is an EF Core migration that can be run to provision the database.    (If you would like to connect to the SQL Server instance and verify the migration, it is easiest to connect via `sqlcmd` in the VS Code terminal.  [Installation instructions](https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility?view=sql-server-ver15))

>If desired, execute the following to connect via sqlcmd:<br> 
>sqlcmd -S reddog-code_devcontainer_db_1,1433 -U SA -P "pass@word1" -d reddogdemo<br><br>
>Execute the following to verify that Reddog tables have been created:<br>
> ```1>SELECT Name FROM sys.tables```<br>
> ```2>GO```

## Test it Out

Now that you have the dapr configs adjusted, try running a few services.  For example, you can try running the OrderService by utilizing the provided VS Code tasks and doing the following:

- Open the Command Palette
- Select `Tasks: Run Task`
- Select `Dapr OrderService`
- Switch to the "Run and Debug" screen
- From the dropdown, choose to `Debug OrderService`

You should now be able to call endpoints exposed by the OrderService.  Try posting an order to `http:localhost:5100/order` (you can use [Postman](https://www.postman.com/downloads/) or [HTTP Rest](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)).  If successful (and other Dapr services are running) you should see the OrderService receive the posted order and then utilize the pubsub component to publish an OrderSummary message.

An example order POST body is below:

```
{
    "storeId": "Redmond",
    "firstName": "John",
    "lastName": "Smith",
    "loyaltyId": "12342",
    "orderItems": [
        {
            "productId": 2,
            "quantity": 2
        }
    ]
}
```

You can follow the steps above to begin running other services (MakeLine, Loyalty, ReceiptGeneration, VirtualWorker or AccountingService) and observe as the local dapr configs allow you to run against local storage, local Redis and a local secret store.
