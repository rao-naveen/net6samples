Simple Web API Delpoyment using minikube
1. Make sure point DOCKER HOST to internal minikube registery
```shell
this will setup DOCKER_HOST point to internal minikube reg
 eval $(minikube -p minikube docker-env)
```
2.Simple Deployement/Service file files
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: config-demo-deployment
  labels:
    app: config-demo
spec:
  replicas: 1
  selector:
    matchLabels:
      app: config-demo
  template:
    metadata:
      labels:
        app: config-demo
    spec:
      volumes:
        - name: config-data-volume
          configMap:
            name: app-config
      containers:
      - name: config-demo
        image: msdemo/usingconfigurationoption:1.0
        volumeMounts:
        - mountPath: /config
          name: config-data-volume
        ports:
        - containerPort: 80
-------------------------------------------
apiVersion: v1
kind: Service
metadata:
  name: config-demo-service
  labels:
    app: nodejs
spec:
  type: LoadBalancer
  ports:
    - port: 8091
      targetPort: 80
  selector:
    app: config-demo
--------------------------------------------------
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
data:
  fordemoappconfig.json: |
    {
      "array": [
        1,
        2,
        3
      ],
      "boolean": true,
      "number": 123,
      "object": {
        "a": "b",
        "c": "d",
        "e": "f"
      },
      "string": "Hello World"
    }

```
3. Execute minikube service command to get access to public endpoint
```shell
 minikube service config-demo-service
```

Sample deployment for testing 

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: linux-demo-deployment
  labels:
    app: linux-demo
spec:
  replicas: 1
  selector:
    matchLabels:
      app: linux-demo
  template:
    metadata:
      labels:
        app: linux-demo
    spec:
      containers:
      - name: linux-demo
        image: alpine
        command: ["printenv"]
        
```
```shell
PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
HOSTNAME=linux-demo-deployment-87ccd7cbc-8vt4w
KUBERNETES_PORT_443_TCP=tcp://10.96.0.1:443
KUBERNETES_PORT_443_TCP_PROTO=tcp
KUBERNETES_PORT_443_TCP_PORT=443
KUBERNETES_PORT_443_TCP_ADDR=10.96.0.1
KUBERNETES_SERVICE_HOST=10.96.0.1
KUBERNETES_SERVICE_PORT=443
KUBERNETES_SERVICE_PORT_HTTPS=443
KUBERNETES_PORT=tcp://10.96.0.1:443
HOME=/root
```