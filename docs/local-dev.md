## Local development setup

GitHub Codespaces makes local development incredibly easy to get up and running.  Under the "Code" menu at the root of the repo, instead of choosing to clone the repository, switch to the Codespaces option and choose to create a "New Codespace".  Choose your desired machine size and wait for the development environment to spin up.  After your Codespaces environment is running, follow the steps below to transition to running locally.

## Overview

One of the nice things about Dapr is the ability to make the transition from executing against different backing services without ever having to change a line of code.  This is especially important if you would like to run completely locally without any reliance on cloud-based services.  The steps below will guide you through altering the Dapr configs to be able to run Daprized services completely locally for a branch location.

### Getting started

1. Before attempting to run anything, trust local certs:
`dotnet dev-certs https --trust`
2. Copy the following dapr configs from `/manifests/branch/base/components`:
  - reddog.binding.receipt.yaml
  - reddog.binding.virtualworker.yaml
  - reddog.pubsub.yaml
  - reddog.secretstore.yaml
  - reddog.state.loyalty.yaml
  - reddog.state.makeline.yaml
3. Follow the steps below to make adjustments for each configuration.  These adjustments will allow you to run completely locally.

### Adjust reddog.binding.receipt.yaml

For the receipt component, we will switch this from relying on a storage account in Azure to local storage.

Replace the dapr component configuration with the following:

```
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: reddog.binding.receipt
  namespace: reddog-retail
spec:
  type: bindings.localstorage
  version: v1
  metadata:
    - name: rootPath
      value: /tmp/receipts
scopes:
  - receipt-generation-service
```

### Adjust reddog.pubsub.yaml

For the pubsub component, we will switch this from relying on rabbitmq to utilize redis streams (that exists as a result of `dapr init`).

Replace the dapr component configuration with the following:

```
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: reddog.pubsub
  namespace: reddog-retail
spec:
  type: pubsub.redis
  version: v1
  metadata: 
    - name: redisHost
      value: dapr_redis_dapr-dev-container:6379
scopes:
  - order-service
  - make-line-service
  - loyalty-service
  - receipt-generation-service
  - accounting-service
```

### Adjust reddog.state.loyalty.yaml

For the loyalty component, we are only making a small adjustment to no longer specify a pulling of redis password from Azure Key Vault.

Replace the dapr component configuration with the following:

```
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: reddog.state.loyalty
  namespace: reddog-retail
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: dapr_redis_dapr-dev-container:6379
scopes:
  - loyalty-service
```

### Adjust reddog.state.makeline.yaml

For the makeline component, we are only making a small adjustment to no longer specify the reliance on Azure Key Vault for a password.

Replace the dapr component configuration with the following:

```
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: reddog.state.makeline
  namespace: reddog-retail
spec:
  type: state.redis
  version: v1
  metadata:
    - name: redisHost
      value: dapr_redis_dapr-dev-container:6379
scopes:
  - make-line-service
```

## Test it Out

Now that you have the dapr configs adjusted, try running a few services.  For example, you can try running the OrderService by utilizing the provided VS Code tasks and doing the following:

- Open the Command Palette
- Select `Tasks: Run Task`
- Select `Dapr OrderService`
- Switch to the "Run and Debug" screen
- From the dropdown, choose to `Debug OrderService`

You should now be able to call endpoints exposed by the OrderService.  Try posting an order to `http:localhost:5100/order`.  If successful (and other Dapr services are running) you should see the OrderService receive the posted order and then utilize the pubsub component to publish an OrderSummary message.

You can follow the steps above to begin running other services (MakeLine, Loyalty, ReceiptGeneration or VirtualWorker) and observe the new dapr configs working with their new configuration.
