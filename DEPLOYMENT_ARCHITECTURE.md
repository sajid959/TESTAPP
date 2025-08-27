# DSAGrind Platform - Deployment Architecture Documentation

## 📋 Document Information
- **Version**: 1.0  
- **Created**: 2024
- **System**: DSAGrind Competitive Programming Platform
- **Focus**: Production-Ready Deployment Architecture
- **Infrastructure**: Cloud-Native with Multi-Region Support

---

## 🎯 Deployment Overview

### Infrastructure Philosophy
DSAGrind follows a **Cloud-Native Deployment Strategy** with emphasis on:
- **High Availability**: 99.9% uptime SLA with automated failover
- **Horizontal Scalability**: Auto-scaling based on demand
- **Geographic Distribution**: Multi-region deployment for global performance
- **Infrastructure as Code**: Declarative infrastructure management
- **Continuous Deployment**: Automated CI/CD pipelines
- **Monitoring & Observability**: Comprehensive system monitoring

### Deployment Environments
```
Environment Strategy:
├── Development (Local & Replit)
├── Staging (Pre-production testing)  
├── Production (Live system)
├── Canary (A/B testing environment)
└── Disaster Recovery (DR site)
```

---

## 🏗️ Cloud Infrastructure Architecture

### Multi-Cloud Strategy

#### Primary Cloud Provider: AWS
```
AWS Infrastructure Components:
├── Compute Services
│   ├── EKS (Elastic Kubernetes Service)
│   ├── EC2 (Elastic Compute Cloud)
│   ├── Fargate (Serverless containers)
│   └── Lambda (Serverless functions)
├── Storage Services
│   ├── RDS (Relational Database Service)
│   ├── DocumentDB (MongoDB-compatible)
│   ├── ElastiCache (Redis)
│   └── S3 (Object Storage)
├── Networking
│   ├── VPC (Virtual Private Cloud)
│   ├── CloudFront (CDN)
│   ├── Application Load Balancer
│   └── Route 53 (DNS)
├── Security
│   ├── IAM (Identity Access Management)
│   ├── KMS (Key Management Service)
│   ├── Secrets Manager
│   └── WAF (Web Application Firewall)
└── Monitoring
    ├── CloudWatch (Monitoring)
    ├── X-Ray (Distributed tracing)
    └── CloudTrail (Audit logging)
```

#### Secondary Provider: Azure (DR/Backup)
```
Azure Backup Infrastructure:
├── AKS (Azure Kubernetes Service)
├── Cosmos DB (Multi-model database)
├── Azure Cache for Redis
├── Azure Container Registry
└── Azure Monitor
```

### Kubernetes Cluster Architecture

#### Production EKS Cluster Configuration
```yaml
# eks-cluster.yaml
apiVersion: eksctl.io/v1alpha5
kind: ClusterConfig

metadata:
  name: dsagrind-prod
  region: us-east-1
  version: "1.28"

vpc:
  cidr: "10.0.0.0/16"
  nat:
    gateway: HighlyAvailable
  clusterEndpoints:
    publicAccess: false
    privateAccess: true

nodeGroups:
  - name: system-nodes
    instanceType: m5.large
    minSize: 2
    maxSize: 10
    desiredCapacity: 3
    availabilityZones: ["us-east-1a", "us-east-1b", "us-east-1c"]
    labels:
      node-type: system
    taints:
      - key: node-type
        value: system
        effect: NoSchedule
    iam:
      attachPolicyARNs:
        - arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy
        - arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy
        - arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly

  - name: application-nodes
    instanceType: c5.xlarge
    minSize: 3
    maxSize: 50
    desiredCapacity: 6
    availabilityZones: ["us-east-1a", "us-east-1b", "us-east-1c"]
    labels:
      node-type: application
    spot: false
    iam:
      attachPolicyARNs:
        - arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy
        - arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy
        - arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly

  - name: code-execution-nodes
    instanceType: m5.2xlarge
    minSize: 2
    maxSize: 20
    desiredCapacity: 4
    availabilityZones: ["us-east-1a", "us-east-1b", "us-east-1c"]
    labels:
      node-type: code-execution
      workload: high-security
    taints:
      - key: workload
        value: code-execution
        effect: NoSchedule
    spot: true
    spotInstanceTypes: ["m5.2xlarge", "m5a.2xlarge", "m4.2xlarge"]

addons:
  - name: vpc-cni
    version: latest
  - name: coredns
    version: latest
  - name: kube-proxy
    version: latest
  - name: aws-ebs-csi-driver
    version: latest
  - name: aws-load-balancer-controller
    version: latest

managedNodeGroups:
  - name: managed-nodes
    instanceTypes: ["m5.large", "m5.xlarge"]
    minSize: 1
    maxSize: 10
    desiredCapacity: 2
    volumeSize: 50
    ssh:
      allow: false
    tags:
      Environment: production
      Project: dsagrind
```

---

## 🚀 Container Orchestration

### Kubernetes Deployment Manifests

#### Gateway API Deployment
```yaml
# gateway-api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: dsagrind-gateway-api
  namespace: dsagrind-prod
  labels:
    app: gateway-api
    version: v1.0.0
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: gateway-api
  template:
    metadata:
      labels:
        app: gateway-api
        version: v1.0.0
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "8080"
        prometheus.io/path: "/metrics"
    spec:
      serviceAccountName: gateway-api-service-account
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        fsGroup: 2000
      containers:
      - name: gateway-api
        image: dsagrind/gateway-api:1.0.0
        imagePullPolicy: Always
        ports:
        - containerPort: 5000
          name: http
        - containerPort: 8080
          name: metrics
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:5000"
        envFrom:
        - secretRef:
            name: dsagrind-secrets
        - configMapRef:
            name: dsagrind-config
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        volumeMounts:
        - name: logs
          mountPath: /app/logs
        - name: temp
          mountPath: /tmp
      volumes:
      - name: logs
        emptyDir: {}
      - name: temp
        emptyDir:
          sizeLimit: 1Gi
      nodeSelector:
        node-type: application
      tolerations:
      - key: node-type
        operator: Equal
        value: application
        effect: NoSchedule
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: app
                  operator: In
                  values:
                  - gateway-api
              topologyKey: kubernetes.io/hostname

---
apiVersion: v1
kind: Service
metadata:
  name: gateway-api-service
  namespace: dsagrind-prod
  labels:
    app: gateway-api
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 5000
    protocol: TCP
    name: http
  selector:
    app: gateway-api

---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: gateway-api-ingress
  namespace: dsagrind-prod
  annotations:
    kubernetes.io/ingress.class: alb
    alb.ingress.kubernetes.io/scheme: internet-facing
    alb.ingress.kubernetes.io/target-type: ip
    alb.ingress.kubernetes.io/certificate-arn: arn:aws:acm:us-east-1:123456789012:certificate/12345678-1234-1234-1234-123456789012
    alb.ingress.kubernetes.io/ssl-redirect: '443'
    alb.ingress.kubernetes.io/healthcheck-path: /health
    alb.ingress.kubernetes.io/healthcheck-interval-seconds: '30'
    alb.ingress.kubernetes.io/healthy-threshold-count: '2'
    alb.ingress.kubernetes.io/unhealthy-threshold-count: '3'
spec:
  rules:
  - host: api.dsagrind.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: gateway-api-service
            port:
              number: 80
  tls:
  - hosts:
    - api.dsagrind.com
```

#### Microservices Deployment (Auth API Example)
```yaml
# auth-api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: dsagrind-auth-api
  namespace: dsagrind-prod
spec:
  replicas: 2
  selector:
    matchLabels:
      app: auth-api
  template:
    metadata:
      labels:
        app: auth-api
        version: v1.0.0
    spec:
      serviceAccountName: auth-api-service-account
      containers:
      - name: auth-api
        image: dsagrind/auth-api:1.0.0
        ports:
        - containerPort: 8001
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: database-secrets
              key: mongodb-connection-string
        - name: JWT__SecretKey
          valueFrom:
            secretKeyRef:
              name: auth-secrets
              key: jwt-secret-key
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "250m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8001
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8001
          initialDelaySeconds: 10
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: auth-api-service
  namespace: dsagrind-prod
spec:
  selector:
    app: auth-api
  ports:
  - port: 80
    targetPort: 8001
```

#### Code Execution Service (High Security)
```yaml
# code-execution-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: dsagrind-submissions-api
  namespace: dsagrind-prod
spec:
  replicas: 3
  selector:
    matchLabels:
      app: submissions-api
  template:
    metadata:
      labels:
        app: submissions-api
        version: v1.0.0
    spec:
      serviceAccountName: submissions-api-service-account
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        seccompProfile:
          type: RuntimeDefault
      containers:
      - name: submissions-api
        image: dsagrind/submissions-api:1.0.0
        ports:
        - containerPort: 8003
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: Docker__Endpoint
          value: "unix:///var/run/docker.sock"
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "1000m"
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop:
            - ALL
        volumeMounts:
        - name: docker-sock
          mountPath: /var/run/docker.sock
          readOnly: true
        - name: tmp
          mountPath: /tmp
        - name: app-tmp
          mountPath: /app/tmp
      volumes:
      - name: docker-sock
        hostPath:
          path: /var/run/docker.sock
          type: Socket
      - name: tmp
        emptyDir: {}
      - name: app-tmp
        emptyDir:
          sizeLimit: 1Gi
      nodeSelector:
        node-type: code-execution
      tolerations:
      - key: workload
        operator: Equal
        value: code-execution
        effect: NoSchedule
      affinity:
        nodeAffinity:
          requiredDuringSchedulingIgnoredDuringExecution:
            nodeSelectorTerms:
            - matchExpressions:
              - key: workload
                operator: In
                values:
                - high-security
```

---

## 🗄️ Database Deployment Strategy

### MongoDB Atlas Cluster Configuration
```yaml
# mongodb-atlas-cluster.tf
resource "mongodbatlas_cluster" "dsagrind_prod" {
  project_id   = var.mongodb_project_id
  name         = "dsagrind-prod-cluster"
  
  cluster_type = "REPLICASET"
  
  provider_settings {
    provider_name     = "AWS"
    instance_size_name = "M30"
    region_name        = "US_EAST_1"
  }
  
  replication_specs {
    num_shards = 1
    regions_config {
      region_name     = "US_EAST_1"
      electable_nodes = 3
      priority        = 7
      read_only_nodes = 0
    }
    regions_config {
      region_name     = "US_WEST_2"
      electable_nodes = 2
      priority        = 6
      read_only_nodes = 1
    }
  }
  
  backup_enabled               = true
  auto_scaling_disk_gb_enabled = true
  mongo_db_major_version       = "7.0"
  
  advanced_configuration {
    fail_index_key_too_long              = false
    javascript_enabled                   = false
    minimum_enabled_tls_protocol         = "TLS1_2"
    no_table_scan                       = true
    oplog_size_mb                       = 2048
    sample_size_bi_connector            = 5000
    sample_refresh_interval_bi_connector = 300
  }
  
  tags = {
    Environment = "production"
    Project     = "dsagrind"
    Backup      = "required"
  }
}

# Database Access Configuration
resource "mongodbatlas_database_user" "dsagrind_app_user" {
  username           = "dsagrind-app"
  password           = var.mongodb_app_password
  project_id         = var.mongodb_project_id
  auth_database_name = "admin"
  
  roles {
    role_name     = "readWrite"
    database_name = "dsagrind_prod"
  }
  
  roles {
    role_name     = "read"
    database_name = "dsagrind_analytics"
  }
  
  scopes {
    name = mongodbatlas_cluster.dsagrind_prod.name
    type = "CLUSTER"
  }
}

resource "mongodbatlas_project_ip_access_list" "eks_cluster" {
  project_id = var.mongodb_project_id
  cidr_block = "10.0.0.0/16"
  comment    = "EKS Cluster CIDR"
}
```

### Redis Cluster Setup
```yaml
# redis-cluster.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: redis-config
  namespace: dsagrind-prod
data:
  redis.conf: |
    maxmemory 2gb
    maxmemory-policy allkeys-lru
    save 900 1
    save 300 10
    save 60 10000
    rdbcompression yes
    rdbchecksum yes
    stop-writes-on-bgsave-error yes
    tcp-keepalive 300
    timeout 0
    tcp-backlog 511
    databases 16
    
---
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: redis-cluster
  namespace: dsagrind-prod
spec:
  serviceName: redis-cluster
  replicas: 3
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
      - name: redis
        image: redis:7.2-alpine
        command:
        - redis-server
        - /etc/redis/redis.conf
        - --cluster-enabled yes
        - --cluster-config-file nodes.conf
        - --cluster-node-timeout 5000
        - --appendonly yes
        ports:
        - containerPort: 6379
          name: client
        - containerPort: 16379
          name: gossip
        volumeMounts:
        - name: redis-config
          mountPath: /etc/redis
        - name: redis-data
          mountPath: /data
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "2Gi"
            cpu: "500m"
      volumes:
      - name: redis-config
        configMap:
          name: redis-config
  volumeClaimTemplates:
  - metadata:
      name: redis-data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 10Gi
      storageClassName: gp3
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow
```yaml
# .github/workflows/deploy-production.yml
name: Deploy to Production

on:
  push:
    branches: [main]
    paths:
    - 'backend/**'
    - 'client/**'
    - '.github/workflows/**'

env:
  AWS_REGION: us-east-1
  EKS_CLUSTER_NAME: dsagrind-prod
  ECR_REPOSITORY_PREFIX: dsagrind

jobs:
  security-scan:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'fs'
        scan-ref: '.'
        format: 'sarif'
        output: 'trivy-results.sarif'
        
    - name: Upload Trivy scan results
      uses: github/codeql-action/upload-sarif@v2
      if: always()
      with:
        sarif_file: 'trivy-results.sarif'

  test:
    runs-on: ubuntu-latest
    needs: security-scan
    services:
      mongodb:
        image: mongo:7.0
        ports:
        - 27017:27017
      redis:
        image: redis:7.2
        ports:
        - 6379:6379
        
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
        cache: 'npm'
        cache-dependency-path: client/package-lock.json
        
    - name: Restore .NET dependencies
      run: dotnet restore backend/DSAGrind.sln
      
    - name: Install Node.js dependencies
      run: npm ci
      working-directory: client
      
    - name: Run .NET tests
      run: dotnet test backend/DSAGrind.sln --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Run frontend tests
      run: npm test -- --coverage --watchAll=false
      working-directory: client
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        files: ./backend/coverage.xml,./client/coverage/lcov.info

  build-and-push:
    runs-on: ubuntu-latest
    needs: test
    strategy:
      matrix:
        service:
        - name: gateway-api
          path: backend/src/Services/DSAGrind.Gateway.API
        - name: auth-api
          path: backend/src/Services/DSAGrind.Auth.API
        - name: problems-api
          path: backend/src/Services/DSAGrind.Problems.API
        - name: submissions-api
          path: backend/src/Services/DSAGrind.Submissions.API
        - name: payments-api
          path: backend/src/Services/DSAGrind.Payments.API
        - name: search-api
          path: backend/src/Services/DSAGrind.Search.API
        - name: ai-api
          path: backend/src/Services/DSAGrind.AI.API
        - name: admin-api
          path: backend/src/Services/DSAGrind.Admin.API
        - name: frontend
          path: client
          
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v2
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ env.AWS_REGION }}
        
    - name: Login to Amazon ECR
      id: login-ecr
      uses: aws-actions/amazon-ecr-login@v1
      
    - name: Build and push Docker image
      env:
        ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
        ECR_REPOSITORY: ${{ env.ECR_REPOSITORY_PREFIX }}/${{ matrix.service.name }}
        IMAGE_TAG: ${{ github.sha }}
      run: |
        docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG -f ${{ matrix.service.path }}/Dockerfile .
        docker tag $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG $ECR_REGISTRY/$ECR_REPOSITORY:latest
        docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
        docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest

  deploy:
    runs-on: ubuntu-latest
    needs: build-and-push
    environment: production
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v2
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ env.AWS_REGION }}
        
    - name: Update kubeconfig
      run: aws eks update-kubeconfig --region ${{ env.AWS_REGION }} --name ${{ env.EKS_CLUSTER_NAME }}
      
    - name: Deploy to Kubernetes
      run: |
        # Update image tags in deployment manifests
        sed -i "s|dsagrind/gateway-api:.*|dsagrind/gateway-api:${{ github.sha }}|g" k8s/production/gateway-api-deployment.yaml
        sed -i "s|dsagrind/auth-api:.*|dsagrind/auth-api:${{ github.sha }}|g" k8s/production/auth-api-deployment.yaml
        # ... repeat for all services
        
        # Apply deployments
        kubectl apply -f k8s/production/
        
        # Wait for rollout to complete
        kubectl rollout status deployment/dsagrind-gateway-api -n dsagrind-prod --timeout=600s
        kubectl rollout status deployment/dsagrind-auth-api -n dsagrind-prod --timeout=600s
        # ... repeat for all services
        
    - name: Run smoke tests
      run: |
        # Wait for services to be ready
        kubectl wait --for=condition=ready pod -l app=gateway-api -n dsagrind-prod --timeout=300s
        
        # Get service endpoint
        GATEWAY_URL=$(kubectl get ingress gateway-api-ingress -n dsagrind-prod -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')
        
        # Run basic health checks
        curl -f https://$GATEWAY_URL/health || exit 1
        curl -f https://$GATEWAY_URL/health/ready || exit 1
        
    - name: Notify deployment status
      if: always()
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### ArgoCD GitOps Configuration
```yaml
# argocd/applications/dsagrind-prod.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: dsagrind-prod
  namespace: argocd
  finalizers:
    - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/dsagrind/platform
    targetRevision: main
    path: k8s/production
  destination:
    server: https://kubernetes.default.svc
    namespace: dsagrind-prod
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
      allowEmpty: false
    syncOptions:
    - CreateNamespace=true
    - PrunePropagationPolicy=foreground
    - PruneLast=true
    retry:
      limit: 5
      backoff:
        duration: 5s
        factor: 2
        maxDuration: 3m
  revisionHistoryLimit: 10
```

---

## 📊 Monitoring & Observability

### Prometheus Monitoring Setup
```yaml
# monitoring/prometheus-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-config
  namespace: monitoring
data:
  prometheus.yml: |
    global:
      scrape_interval: 15s
      evaluation_interval: 15s
      
    rule_files:
    - "dsagrind_rules.yml"
    
    alerting:
      alertmanagers:
      - static_configs:
        - targets:
          - alertmanager:9093
          
    scrape_configs:
    - job_name: 'kubernetes-pods'
      kubernetes_sd_configs:
      - role: pod
      relabel_configs:
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
        action: replace
        target_label: __metrics_path__
        regex: (.+)
      - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
        action: replace
        regex: ([^:]+)(?::\d+)?;(\d+)
        replacement: $1:$2
        target_label: __address__
        
    - job_name: 'dsagrind-gateway'
      static_configs:
      - targets: ['gateway-api-service.dsagrind-prod:80']
      metrics_path: /metrics
      scrape_interval: 10s
      
    - job_name: 'dsagrind-microservices'
      kubernetes_sd_configs:
      - role: service
        namespaces:
          names:
          - dsagrind-prod
      relabel_configs:
      - source_labels: [__meta_kubernetes_service_label_app]
        regex: '.*-api'
        action: keep

  dsagrind_rules.yml: |
    groups:
    - name: dsagrind.rules
      rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.1
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate detected"
          description: "Error rate is above 10% for 5 minutes"
          
      - alert: HighResponseTime
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 2
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "High response time detected"
          description: "95th percentile response time is above 2 seconds"
          
      - alert: PodCrashLooping
        expr: rate(kube_pod_container_status_restarts_total[15m]) > 0
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Pod is crash looping"
          description: "Pod {{ $labels.pod }} in namespace {{ $labels.namespace }} is crash looping"
          
      - alert: DatabaseConnectionIssues
        expr: mongodb_connections_current / mongodb_connections_available > 0.8
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Database connection pool nearly exhausted"
          description: "MongoDB connection usage is above 80%"
```

### Grafana Dashboards
```json
{
  "dashboard": {
    "title": "DSAGrind Platform Overview",
    "tags": ["dsagrind", "production"],
    "timezone": "UTC",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "sum(rate(http_requests_total[5m])) by (service)",
            "legendFormat": "{{service}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket[5m])) by (le, service))",
            "legendFormat": "95th percentile {{service}}"
          }
        ]
      },
      {
        "title": "Error Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "sum(rate(http_requests_total{status=~\"5..\"}[5m])) by (service) / sum(rate(http_requests_total[5m])) by (service)",
            "legendFormat": "Error rate {{service}}"
          }
        ]
      },
      {
        "title": "Database Performance",
        "type": "graph",
        "targets": [
          {
            "expr": "mongodb_op_counters_total",
            "legendFormat": "{{type}} operations"
          }
        ]
      },
      {
        "title": "Code Execution Metrics",
        "type": "graph",
        "targets": [
          {
            "expr": "sum(rate(code_executions_total[5m])) by (language)",
            "legendFormat": "{{language}} executions/sec"
          }
        ]
      },
      {
        "title": "Active Users",
        "type": "stat",
        "targets": [
          {
            "expr": "sum(active_users_total)",
            "legendFormat": "Active Users"
          }
        ]
      }
    ]
  }
}
```

### ELK Stack Logging
```yaml
# logging/elasticsearch.yaml
apiVersion: elasticsearch.k8s.elastic.co/v1
kind: Elasticsearch
metadata:
  name: dsagrind-elasticsearch
  namespace: logging
spec:
  version: 8.11.0
  nodeSets:
  - name: masters
    count: 3
    config:
      node.roles: ["master"]
      xpack.security.enabled: true
    podTemplate:
      spec:
        containers:
        - name: elasticsearch
          resources:
            requests:
              memory: 2Gi
              cpu: 1000m
            limits:
              memory: 4Gi
              cpu: 2000m
    volumeClaimTemplates:
    - metadata:
        name: elasticsearch-data
      spec:
        accessModes:
        - ReadWriteOnce
        resources:
          requests:
            storage: 100Gi
        storageClassName: gp3
  - name: data
    count: 3
    config:
      node.roles: ["data", "ingest"]
    podTemplate:
      spec:
        containers:
        - name: elasticsearch
          resources:
            requests:
              memory: 4Gi
              cpu: 1000m
            limits:
              memory: 8Gi
              cpu: 2000m
    volumeClaimTemplates:
    - metadata:
        name: elasticsearch-data
      spec:
        accessModes:
        - ReadWriteOnce
        resources:
          requests:
            storage: 500Gi
        storageClassName: gp3

---
apiVersion: kibana.k8s.elastic.co/v1
kind: Kibana
metadata:
  name: dsagrind-kibana
  namespace: logging
spec:
  version: 8.11.0
  count: 2
  elasticsearchRef:
    name: dsagrind-elasticsearch
  podTemplate:
    spec:
      containers:
      - name: kibana
        resources:
          requests:
            memory: 1Gi
            cpu: 500m
          limits:
            memory: 2Gi
            cpu: 1000m

---
apiVersion: beat.k8s.elastic.co/v1beta1
kind: Beat
metadata:
  name: dsagrind-filebeat
  namespace: logging
spec:
  type: filebeat
  version: 8.11.0
  elasticsearchRef:
    name: dsagrind-elasticsearch
  config:
    filebeat.inputs:
    - type: container
      paths:
      - /var/log/containers/*dsagrind*.log
      processors:
      - add_kubernetes_metadata:
          host: ${NODE_NAME}
          matchers:
          - logs_path:
              logs_path: "/var/log/containers/"
    output.elasticsearch:
      hosts: ["dsagrind-elasticsearch-es-http:9200"]
      index: "dsagrind-logs-%{+yyyy.MM.dd}"
  daemonSet:
    podTemplate:
      spec:
        serviceAccountName: filebeat
        terminationGracePeriodSeconds: 30
        hostNetwork: true
        dnsPolicy: ClusterFirstWithHostNet
        containers:
        - name: filebeat
          securityContext:
            runAsUser: 0
          volumeMounts:
          - name: varlogcontainers
            mountPath: /var/log/containers
            readOnly: true
          - name: varlogpods
            mountPath: /var/log/pods
            readOnly: true
          - name: varlibdockercontainers
            mountPath: /var/lib/docker/containers
            readOnly: true
        volumes:
        - name: varlogcontainers
          hostPath:
            path: /var/log/containers
        - name: varlogpods
          hostPath:
            path: /var/log/pods
        - name: varlibdockercontainers
          hostPath:
            path: /var/lib/docker/containers
```

---

## 🔄 Disaster Recovery & Backup

### Backup Strategy
```yaml
# backup/mongodb-backup.yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: mongodb-backup
  namespace: dsagrind-prod
spec:
  schedule: "0 2 * * *"  # Daily at 2 AM UTC
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: mongodb-backup
            image: mongo:7.0
            command:
            - /bin/bash
            - -c
            - |
              TIMESTAMP=$(date +%Y%m%d_%H%M%S)
              BACKUP_NAME="dsagrind_backup_${TIMESTAMP}"
              
              # Create backup
              mongodump --uri="${MONGODB_URI}" --out="/backup/${BACKUP_NAME}"
              
              # Compress backup
              tar -czf "/backup/${BACKUP_NAME}.tar.gz" -C "/backup" "${BACKUP_NAME}"
              
              # Upload to S3
              aws s3 cp "/backup/${BACKUP_NAME}.tar.gz" "s3://${BACKUP_BUCKET}/mongodb/${BACKUP_NAME}.tar.gz"
              
              # Cleanup local files
              rm -rf "/backup/${BACKUP_NAME}"
              rm -f "/backup/${BACKUP_NAME}.tar.gz"
              
              # Cleanup old backups (keep 30 days)
              aws s3 ls "s3://${BACKUP_BUCKET}/mongodb/" | while read -r line; do
                createDate=$(echo $line | awk '{print $1" "$2}')
                createDate=$(date -d "$createDate" +%s)
                cutoffDate=$(date -d "30 days ago" +%s)
                if [[ $createDate -lt $cutoffDate ]]; then
                  fileName=$(echo $line | awk '{print $4}')
                  if [[ $fileName != "" ]]; then
                    aws s3 rm "s3://${BACKUP_BUCKET}/mongodb/${fileName}"
                  fi
                fi
              done
            env:
            - name: MONGODB_URI
              valueFrom:
                secretKeyRef:
                  name: database-secrets
                  key: mongodb-uri
            - name: BACKUP_BUCKET
              value: "dsagrind-backups"
            - name: AWS_REGION
              value: "us-east-1"
            volumeMounts:
            - name: backup-storage
              mountPath: /backup
          volumes:
          - name: backup-storage
            emptyDir:
              sizeLimit: 10Gi
          restartPolicy: OnFailure

---
# Velero backup for Kubernetes resources
apiVersion: velero.io/v1
kind: Schedule
metadata:
  name: dsagrind-daily-backup
  namespace: velero
spec:
  schedule: "0 1 * * *"  # Daily at 1 AM UTC
  template:
    includedNamespaces:
    - dsagrind-prod
    - monitoring
    - logging
    excludedResources:
    - events
    - events.events.k8s.io
    storageLocation: aws-s3
    ttl: 720h  # 30 days
    snapshotVolumes: true
```

### Disaster Recovery Plan
```yaml
# dr/disaster-recovery-plan.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: disaster-recovery-plan
  namespace: dsagrind-prod
data:
  recovery-procedures.md: |
    # DSAGrind Disaster Recovery Procedures
    
    ## RTO/RPO Targets
    - Recovery Time Objective (RTO): 4 hours
    - Recovery Point Objective (RPO): 1 hour
    
    ## Disaster Scenarios
    
    ### 1. Primary Region Failure (us-east-1)
    
    **Detection:**
    - Multiple availability zones down
    - Database cluster unreachable
    - Application services failing health checks
    
    **Recovery Steps:**
    1. Activate secondary region (us-west-2)
    2. Update DNS to point to secondary region
    3. Restore database from latest backup
    4. Deploy application services to secondary region
    5. Verify all services are functional
    
    **Commands:**
    ```bash
    # Switch to DR region
    aws eks update-kubeconfig --region us-west-2 --name dsagrind-dr
    
    # Deploy applications
    kubectl apply -f k8s/disaster-recovery/
    
    # Update Route53 records
    aws route53 change-resource-record-sets --hosted-zone-id Z123456789 --change-batch file://dr-dns-changes.json
    ```
    
    ### 2. Database Corruption/Loss
    
    **Recovery Steps:**
    1. Stop all write operations
    2. Identify last known good backup
    3. Restore from backup to new cluster
    4. Update connection strings
    5. Restart applications
    6. Verify data integrity
    
    ### 3. Kubernetes Cluster Failure
    
    **Recovery Steps:**
    1. Create new EKS cluster
    2. Restore from Velero backup
    3. Update load balancer targets
    4. Verify all pods are running
    5. Test application functionality
    
    ## Recovery Verification Checklist
    - [ ] All microservices are running
    - [ ] Database connectivity restored
    - [ ] Frontend application accessible
    - [ ] User authentication working
    - [ ] Code execution functionality working
    - [ ] Payment processing functional
    - [ ] Monitoring and alerting active
    - [ ] SSL certificates valid
    - [ ] DNS resolution correct
```

This comprehensive deployment architecture ensures DSAGrind can be deployed reliably in production with high availability, scalability, and disaster recovery capabilities while maintaining security and performance standards.