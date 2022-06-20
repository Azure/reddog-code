## Overview

One of the nice things about Dapr is the ability to make the transition from executing against different backing services without ever having to change a line of code.  This is especially important if you would like to run completely locally without any reliance on cloud-based services.  You will notice in the `/manifests/branch/local` location that a number of dapr configs have been provided.  These dapr configs will allow you to run each of the dapr-ized services in a local manner relying on local storage, local Redis and a local secret store.

With that said, there are a few short steps you will need to perform in order to set up your local development environment.  The instructions below will guide you through setting up a GitHub Codespace and subsequently running all RedDog services.


## Create Codespace

1. Browse to the Codespaces page in the repo: https://github.com/Azure/reddog-code/codespaces
2. Select "New codespace"
3. On the "master" branch, select "Create codespace"
4. Pick a size for your Codespace environment
5. Wait for the environment to be ready

Once complete, VS Code will be running in your browser with the master branch cloned.


## Setup RedDog Environment

1. Start the Bootstrapper Dapr sidecar by doing the following:
    1. Open the Command Palette
    2. Select `Tasks: Run Task`
    3. Select `Dapr Bootstrapper`
2. In a VS Code terminal window, set an environment variable for the Bootstrapper's Dapr HTTP Port
```
DAPR_HTTP_PORT=5880
```
3. Switch to the Run and Debug screen and debug the Bootstrapper.  Upon completion, you should now have a "reddogdemo" database in the given SQL Server instance.

>The Accounting Service within RedDog relies on SQL Server for persistent storage.  As such, you will notice that the `.devcontainer` configuration for Codespaces (located withing the `.devcontainer` folder) points at a Docker compose file that includes a container image reference to SQL Server.  While the image will be pulled into your Codespace for you, the database itself will still need to be provisioned.  Included in the RedDog repo is an EF Core migration that can be run to provision the database.  This migration functionality is located within RedDog.Bootstrapper. 

>The SQL Server command-line tools have been installed for you.  If desired, execute the following to connect via sqlcmd:<br> 
>sqlcmd -S reddog-sql-server,1433 -U SA -P "pass@word1" -d reddogdemo<br><br>
>Execute the following to verify that RedDog tables have been created:<br>
> ```1>SELECT Name FROM sys.tables```<br>
> ```2>GO```

## Test it Out

Now that everything is in place, try running a few services.  For example, you can try running the OrderService by utilizing the provided VS Code tasks and doing the following:

- Open the Command Palette
- Select `Tasks: Run Task`
- Select `Dapr OrderService`
- Switch to the "Run and Debug" screen
- From the dropdown, choose to `Debug OrderService`

You should now be able to call endpoints exposed by the OrderService.  Try posting an order to `http:localhost:5100/order`.  To do so, a series of .rest files have been provided for you in the root of the RedDog solution in a folder named "rest-samples"  (Install the [HTTP Rest](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension to easily execute these requests.).  If successful (and other Dapr services are running) you should see the OrderService receive the posted order and then utilize the pubsub component to publish an OrderSummary message.

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


## Running All Services and the RedDog UI

A few helpful VS Code tasks and launch configurations have been provided to help you run all services and the RedDog UI at the same time.  To do so, follow these instructions:

### Running All Dapr-ized Services

1. Run all Dapr services by doing the following:
    - Open the Command Palette
    - Select `Tasks: Run Task`
    - Select `Dapr (All Services)`
2. Debug all services by doing the following:
    - Switch to the Run and Debug screen
    - From the dropdown, choose to `Debug All Services`
3. After all services are running, switch to the `Ports` tab within VS Code (likely located to the right of your Terminal window) and find ports 5200 and 5700.  These two ports are for the MakeLine Service and the Accounting Service, respectively.  For both of these ports, set the Visibility to Public.  This will allow the RedDog.UI to make calls to each of these services to retrieve necessary data for display purposes.

### Running the RedDog.UI
1. In the terminal window, navigate into the RedDog.UI folder
2. Perform an npm install to install necessary dependencies for the UI
```
npm install
```
3. Before running the RedDog.UI, we need to create a .env file in the root of this web app to set the base URL for the MakeLine and Accounting services.  To do so, create a file named `.env` in the RedDog.UI folder. Execute the following to easily create the file:
```
touch .env
```
4. In this file, place the following contents:
```
VUE_APP_MAKELINE_BASE_URL=http://localhost:5200
VUE_APP_ACCOUNTING_BASE_URL=http://localhost:5700
```
8. Switch to the Run and Debug Screen and, from the dropdown, choose `Debug UI` and begin debugging the UI.
9. After a few moments, you will notice port 8080 show up in the Ports window.  Set this port to public.
10. Launch the UI by clicking on the VS code pop-up in the lower-right of your screen or by clicking on the "Open in Browser" icon on the port 8080 line within the "Ports" window.

At this point, you should have all dapr services running as well as the RedDog.UI.  After a few moments, the RedDog.UI will begin showing metrics related to orders being created and worked by the VirtualCustomer and VirtualWorker services.  All of the services within RedDog will be exercised by the VirtualCustomer creating orders.  Take a peek at the various debug windows for each of the services to see data being processed in real-time.
