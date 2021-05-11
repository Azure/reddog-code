## Running these functions in Kubernetes

### Install RabbitMQ

helm install --set auth.username=reddog --set auth.password=[password] --set replicaCount=3 --set service.type=LoadBalancer reddog-rabbit bitnami/rabbitmq

### Install KEDA in your cluster

```cli
func kubernetes install --n keda
```

OR

[Follow the instructions](https://keda.sh/docs/2.0/deploy/) to deploy KEDA.

* Confirm that KEDA installed properly:

```cli
kubectl get customresourcedefinition

NAME                     AGE
scaledobjects.keda.sh    2h
scaledjobs.keda.sh       2h
```

### Deploy this Function App to KEDA

* Login to Github Container Registry (use PAT as the password)
```cli
docker login ghcr.io
```

* Build the docker container, push to the registry, and deploy to kubernetes:
```cli
func kubernetes deploy --name rabbit-mq-fx --registry ghcr.io/cloudnativegbb/paas-vnext --polling-interval 20 --cooldown-period 300
```

OR

* You can build, push, and deploy separately

```cli
# create deployment yml
func kubernetes deploy --name corp-transfer-service --registry ghcr.io/cloudnativegbb/paas-vnext --polling-interval 20 --cooldown-period 300 --dry-run > func-deployment.yaml

docker build -t ghcr.io/cloudnativegbb/paas-vnext/corp-transfer-service:0.3 .
docker push ghcr.io/cloudnativegbb/paas-vnext/corp-transfer-service:0.3

kubectl apply -f func-deployment.yaml
```

* In Github packages, ensure that the visibility of the package `ghcr.io/cloudnativegbb/paas-vnext/rabbit-mq-fx` is public or you need to pass credentials in the deployment script.

### Add a queue message and validate the function app scales with KEDA

Initially, you should see 0 pods.

```cli
kubectl get deploy
```

* Add a message to the RabbitMQ queue (using `rabbit-mq-javascript/send.js` utility or manually).
* KEDA will detect the events and add pods.  (By default the polling interval set is 30s on the `ScaledObject` resource, but we are setting this to 20s here)

```cli
kubectl get pods -w
```

* You can validate the message was consumed by using `kubectl logs` on the activated pod. After all messages are consumed and the cooldown period has elapsed (default 300s, our setting is 60s), the last pod should scale back down to zero.

### Cleanup

```cli
kubectl delete deploy corp-transfer-service
kubectl delete ScaledObject corp-transfer-service
kubectl delete Secret corp-transfer-service
```
