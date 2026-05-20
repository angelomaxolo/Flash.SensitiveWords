# Production Deployment Strategy for Flash.SensitiveWords

## Overview

This document outlines a comprehensive production deployment strategy for the Flash.SensitiveWords API, covering infrastructure, CI/CD, containerization, scaling, and operational excellence.

---

## 1. Infrastructure Architecture

### Cloud Platform: Azure (Microsoft Ecosystem)
Given that this is a .NET Core application, Azure is the optimal choice:
- Native .NET support
- Seamless integration with SQL Server
- App Insights already configured
- Strong security and compliance certifications

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CDN (Azure CDN)                         │
│                    (Cache Static Assets)                        │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                   Application Gateway / Load Balancer           │
│              (WAF, SSL Termination, Route Rules)                │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Azure Container Registry                      │
│                  (Store Docker Images)                          │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│              Azure Kubernetes Service (AKS)                      │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │  Pod 1: API      │  │  Pod 2: API      │  │  Pod 3: API  │  │
│  │  Container       │  │  Container       │  │  Container   │  │
│  └──────────────────┘  └──────────────────┘  └──────────────┘  │
│                                                                  │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │  Pod 4: API      │  │  Pod 5: API      │  (Auto-scaling)   │
│  │  Container       │  │  Container       │                    │
│  └──────────────────┘  └──────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘
         ↓                                         ↓
┌──────────────────┐                    ┌──────────────────────┐
│  SQL Server      │                    │  Redis Cache         │
│  Primary         │←──Replication────→ │  (Distributed)       │
│  (Read/Write)    │                    │                      │
└──────────────────┘                    └──────────────────────┘
         ↓
┌──────────────────┐
│  Backup Storage  │
│  (Geo-Redundant) │
└──────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                    Monitoring & Observability                    │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐     │
│  │ App Insights │  │ Azure Monitor │  │ Log Analytics      │     │
│  └──────────────┘  └──────────────┘  └────────────────────┘     │
└──────────────────────────────────────────────────────────────────┘
```

---

## 2. Containerization

### Docker Image Strategy

**Multi-stage build (production-optimized):**

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY ["Flash.SensitiveWords.slnx", "."]
COPY ["src/", "./src/"]

# Restore NuGet packages
RUN dotnet restore "Flash.SensitiveWords.slnx"

# Build release
RUN dotnet build "Flash.SensitiveWords.slnx" -c Release --no-restore

# Publish
RUN dotnet publish "src/Flash.SensitiveWords.API/Flash.SensitiveWords.API.csproj" -c Release -o /app/publish --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Install curl for health checks
RUN apk add --no-cache curl

WORKDIR /app

# Copy from build stage
COPY --from=build /app/publish .

# Non-root user for security
RUN addgroup -g 1001 dotnet && \
    adduser -u 1001 -G dotnet -s /sbin/nologin -D dotnet && \
    chown -R dotnet:dotnet /app

USER dotnet

EXPOSE 8080

ENTRYPOINT ["dotnet", "Flash.SensitiveWords.API.dll"]

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health/ready || exit 1
```

**Key optimizations:**
- Alpine Linux base (smaller image, ~130MB vs 400MB+)
- Non-root user execution for security
- Health check configuration
- Layer caching for faster builds

### Container Registry

```bash
# Push to Azure Container Registry
az acr build --registry myRegistry \
  --image sensitive-words-api:latest \
  --image sensitive-words-api:v1.0.0 .
```

---

## 3. Kubernetes Deployment on AKS

### Kubernetes Manifests

**Deployment Configuration:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sensitive-words-api
  namespace: production
  labels:
    app: sensitive-words-api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: sensitive-words-api
  template:
    metadata:
      labels:
        app: sensitive-words-api
        version: v1
    spec:
      # Pod disruption budget for high availability
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
                  - sensitive-words-api
              topologyKey: kubernetes.io/hostname
      
      # Service account with minimal permissions
      serviceAccountName: sensitive-words-api
      securityContext:
        runAsNonRoot: true
        runAsUser: 1001
      
      containers:
      - name: api
        image: myRegistry.azurecr.io/sensitive-words-api:latest
        imagePullPolicy: Always
        
        # Port configuration
        ports:
        - name: http
          containerPort: 8080
          protocol: TCP
        
        # Environment variables from ConfigMap and Secrets
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-connection-secret
              key: connection-string
        - name: ApiSettings__ApiKey
          valueFrom:
            secretKeyRef:
              name: api-key-secret
              key: api-key
        - name: ApplicationInsights__ConnectionString
          valueFrom:
            secretKeyRef:
              name: app-insights-secret
              key: connection-string
        
        # Resource limits and requests
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        
        # Liveness probe - restart if unhealthy
        livenessProbe:
          httpGet:
            path: /health/live
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        # Readiness probe - remove from load balancer if not ready
        readinessProbe:
          httpGet:
            path: /health/ready
            port: http
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 2
        
        # Security context
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop:
            - ALL
        
        # Volume mounts for temp files
        volumeMounts:
        - name: tmp
          mountPath: /tmp
      
      volumes:
      - name: tmp
        emptyDir: {}
---
apiVersion: v1
kind: Service
metadata:
  name: sensitive-words-api
  namespace: production
spec:
  type: ClusterIP
  selector:
    app: sensitive-words-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: http
    name: http
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: sensitive-words-api-hpa
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: sensitive-words-api
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
      - type: Percent
        value: 100
        periodSeconds: 30
      - type: Pods
        value: 2
        periodSeconds: 30
      selectPolicy: Max
---
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: sensitive-words-api-pdb
  namespace: production
spec:
  minAvailable: 2
  selector:
    matchLabels:
      app: sensitive-words-api
```

### Ingress Configuration (API Gateway):

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: sensitive-words-ingress
  namespace: production
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/rate-limit: "100"
    nginx.ingress.kubernetes.io/rate-limit-window: "1m"
spec:
  ingressClassName: azure-application-gateway
  tls:
  - hosts:
    - api.sensitive-words.com
    secretName: api-tls-cert
  rules:
  - host: api.sensitive-words.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: sensitive-words-api
            port:
              number: 80
```

---

## 4. Database Deployment Strategy

### SQL Server on Azure

**Setup:**
```bash
# Create SQL Server
az sql server create \
  --name sensitive-words-server \
  --resource-group myResourceGroup \
  --location eastus \
  --admin-user sqladmin \
  --admin-password P@ssw0rd123!

# Create database
az sql db create \
  --resource-group myResourceGroup \
  --server sensitive-words-server \
  --name SensitiveWordsDb \
  --service-objective S1
```

### Database Migration Strategy

**Entity Framework Core migrations in CI/CD:**

```csharp
// In Program.cs - Auto-migrate on startup (careful in production)
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SensitiveWordsDbContext>();
        
        // List pending migrations
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        
        if (pendingMigrations.Any())
        {
            // Log the migration
            logger.LogInformation($"Applying migrations: {string.Join(", ", pendingMigrations)}");
            
            // Apply migrations
            await db.Database.MigrateAsync();
            
            logger.LogInformation("Database migrations completed");
        }
    }
}
```

**Better approach: Separate migration job:**

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: db-migration-job
  namespace: production
spec:
  template:
    spec:
      containers:
      - name: migrator
        image: myRegistry.azurecr.io/sensitive-words-api:latest
        env:
        - name: RUN_MIGRATIONS_ONLY
          value: "true"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-connection-secret
              key: connection-string
      restartPolicy: Never
  backoffLimit: 3
```

### Backup Strategy

```bash
# Automated daily backup
az sql db backup create \
  --resource-group myResourceGroup \
  --server sensitive-words-server \
  --database SensitiveWordsDb

# Geo-replicated storage
az sql db replicate \
  --resource-group myResourceGroup \
  --server sensitive-words-server \
  --name SensitiveWordsDb \
  --secondary-server replica-server \
  --failover-policy Automatic
```

---

## 5. CI/CD Pipeline (GitHub Actions)

```yaml
name: Build, Test & Deploy

on:
  push:
    branches:
      - main
      - develop
  pull_request:
    branches:
      - main

env:
  REGISTRY: myRegistry.azurecr.io
  IMAGE_NAME: sensitive-words-api

jobs:
  # Job 1: Build and Test
  build:
    runs-on: ubuntu-latest
    outputs:
      image-tag: ${{ steps.meta.outputs.tags }}
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore Flash.SensitiveWords.slnx
    
    - name: Build
      run: dotnet build --configuration Release --no-restore Flash.SensitiveWords.slnx
    
    - name: Run unit tests
      run: dotnet test --configuration Release --no-build --verbosity normal
    
    - name: Code quality - SonarQube
      uses: SonarSource/sonarcloud-github-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
    
    - name: SonarQube quality gate check
      run: |
        # Check if quality gate passed
        if [ "${{ steps.sonarcloud.outputs.qualityGateStatus }}" != "OK" ]; then
          echo "Quality gate failed"
          exit 1
        fi
    
    - name: Login to Azure Container Registry
      uses: azure/docker-login@v1
      with:
        login-server: ${{ env.REGISTRY }}
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}
    
    - name: Extract metadata
      id: meta
      run: |
        BRANCH=${{ github.ref_name }}
        COMMIT_SHA=${{ github.sha }}
        BUILD_NUMBER=${{ github.run_number }}
        
        if [ "$BRANCH" = "main" ]; then
          TAG="latest"
        else
          TAG="dev-$BUILD_NUMBER"
        fi
        
        echo "tags=${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${TAG},${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${COMMIT_SHA:0:7}" >> $GITHUB_OUTPUT
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v4
      with:
        context: .
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        cache-from: type=gha
        cache-to: type=gha,mode=max

  # Job 2: Deploy to Staging
  deploy-staging:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    steps:
    - uses: actions/checkout@v3
    
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Deploy to AKS - Staging
      run: |
        az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
        kubectl set image deployment/sensitive-words-api \
          api=${{ needs.build.outputs.image-tag }} \
          -n staging --record
        kubectl rollout status deployment/sensitive-words-api -n staging

  # Job 3: Deploy to Production (Manual Approval)
  deploy-production:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment:
      name: production
      url: https://api.sensitive-words.com
    steps:
    - uses: actions/checkout@v3
    
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Run database migrations
      run: |
        az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
        kubectl apply -f k8s/db-migration-job.yaml -n production
        kubectl wait --for=condition=complete job/db-migration-job -n production --timeout=300s
    
    - name: Blue-Green Deployment
      run: |
        # Deploy new version (Green)
        kubectl set image deployment/sensitive-words-api-green \
          api=${{ needs.build.outputs.image-tag }} \
          -n production --record
        kubectl rollout status deployment/sensitive-words-api-green -n production
        
        # Run smoke tests
        ./scripts/smoke-tests.sh https://green.api.sensitive-words.com
        
        # Switch traffic to green (via ingress update)
        kubectl patch ingress sensitive-words-ingress -n production --type='json' \
          -p='[{"op": "replace", "path": "/spec/rules/0/http/paths/0/backend/service/name", "value":"sensitive-words-api-green"}]'
        
        # Monitor for errors (5 minutes)
        sleep 300
        
        # If no errors, decommission blue
        kubectl set image deployment/sensitive-words-api-blue \
          api=pause -n production
```

---

## 6. Configuration Management & Secrets Security

### Overview: Secrets Flow

```
┌──────────────────────────────────────────────────────────────────────┐
│                      Azure Key Vault                                 │
│          (Centralized Secret Store - Source of Truth)                │
│  - Database Connection Strings                                       │
│  - API Keys                                                          │
│  - Application Insights Keys                                         │
│  - TLS Certificates                                                  │
└──────────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────────┐
│              GitHub Actions Workflow                                  │
│  1. Authenticate to Azure                                            │
│  2. Retrieve secrets from Key Vault                                  │
│  3. Create/Update Kubernetes Secrets                                 │
│  4. Deploy to AKS                                                    │
└──────────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────────┐
│           Kubernetes Secrets (In-Cluster)                            │
│  db-connection-secret                                                │
│  api-key-secret                                                      │
│  app-insights-secret                                                 │
└──────────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────────┐
│        Pod Environment Variable Injection                            │
│  ┌────────────────────────────────────┐                             │
│  │  Running Container                 │                             │
│  │  - ConnectionStrings__DefaultConnection (injected)               │
│  │  - ApiSettings__ApiKey (injected)                                │
│  │  - ApplicationInsights__ConnectionString (injected)              │
│  └────────────────────────────────────┘                             │
└──────────────────────────────────────────────────────────────────────┘
```

### Azure Key Vault Setup

Create and configure Azure Key Vault to store all sensitive credentials:

```bash
# Create Azure Key Vault
az keyvault create \
  --name myKeyVault \
  --resource-group myResourceGroup \
  --location eastus \
  --enabled-for-secret-storage true \
  --enabled-for-deployment true

# Store database connection string
az keyvault secret set \
  --vault-name myKeyVault \
  --name "db-connection-string" \
  --value "Server=sensitive-words-server.database.windows.net;Database=SensitiveWordsDb;User Id=sqladmin;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"

# Store API Key
az keyvault secret set \
  --vault-name myKeyVault \
  --name "api-key" \
  --value "88a2DoxF7oqNBfHiy4RSzMNvkufAV3jg"

# Store Application Insights connection string
az keyvault secret set \
  --vault-name myKeyVault \
  --name "app-insights-connection-string" \
  --value "InstrumentationKey=xxx;..."

# Grant AKS managed identity access to Key Vault
az keyvault set-policy \
  --name myKeyVault \
  --object-id <AKS-MANAGED-IDENTITY-ID> \
  --secret-permissions get list
```

### GitHub Actions Secrets Configuration

Store GitHub Actions secrets that authenticate deployments:

```bash
# In GitHub Repository Settings → Secrets and variables → Actions
# Create the following secrets:

# AZURE_CREDENTIALS - Service Principal for Azure authentication
# Format: JSON from: az ad sp create-for-rbac --sdk-auth
# {
#   "clientId": "...",
#   "clientSecret": "...",
#   "subscriptionId": "...",
#   "tenantId": "..."
# }

# ACR_USERNAME - Azure Container Registry username
# ACR_PASSWORD - Azure Container Registry password
# ACR_LOGIN_SERVER - Container registry login server (myRegistry.azurecr.io)

# SONAR_TOKEN - SonarQube/SonarCloud token for code quality
# GITHUB_TOKEN - Already available, auto-generated by GitHub
```

**Retrieve and set AZURE_CREDENTIALS:**

```bash
# Create service principal with appropriate permissions
az ad sp create-for-rbac \
  --name "github-actions-sp" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/myResourceGroup \
  --sdk-auth

# Copy the JSON output and set as AZURE_CREDENTIALS secret in GitHub
```

### GitHub Actions: Fetch Secrets from Key Vault

**Enhanced CI/CD pipeline with Key Vault integration:**

```yaml
name: Build, Test & Deploy with Key Vault

on:
  push:
    branches:
      - main
      - develop

env:
  REGISTRY: myRegistry.azurecr.io
  IMAGE_NAME: sensitive-words-api
  KEY_VAULT_NAME: myKeyVault

jobs:
  build:
    runs-on: ubuntu-latest
    outputs:
      image-tag: ${{ steps.meta.outputs.tags }}
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore and build
      run: |
        dotnet restore Flash.SensitiveWords.slnx
        dotnet build --configuration Release --no-restore Flash.SensitiveWords.slnx
    
    - name: Run tests
      run: dotnet test --configuration Release --no-build

  deploy-staging:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    steps:
    - uses: actions/checkout@v3
    
    # Step 1: Authenticate to Azure
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    # Step 2: Retrieve secrets from Azure Key Vault
    - name: Retrieve secrets from Key Vault
      id: keyvault
      uses: azure/cli@v1
      with:
        inlineScript: |
          DB_CONNECTION=$(az keyvault secret show --name "db-connection-string" --vault-name ${{ env.KEY_VAULT_NAME }} --query value -o tsv)
          API_KEY=$(az keyvault secret show --name "api-key" --vault-name ${{ env.KEY_VAULT_NAME }} --query value -o tsv)
          APP_INSIGHTS=$(az keyvault secret show --name "app-insights-connection-string" --vault-name ${{ env.KEY_VAULT_NAME }} --query value -o tsv)
          
          # Output to GitHub Actions environment (masked for security)
          echo "::add-mask::$DB_CONNECTION"
          echo "::add-mask::$API_KEY"
          echo "::add-mask::$APP_INSIGHTS"
          
          echo "DB_CONNECTION=$DB_CONNECTION" >> $GITHUB_ENV
          echo "API_KEY=$API_KEY" >> $GITHUB_ENV
          echo "APP_INSIGHTS=$APP_INSIGHTS" >> $GITHUB_ENV
    
    # Step 3: Create/Update Kubernetes Secrets
    - name: Create Kubernetes Secrets in Staging
      run: |
        az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
        
        # Delete existing secrets (if any) to avoid conflicts
        kubectl delete secret db-connection-secret --namespace=staging --ignore-not-found=true
        kubectl delete secret api-key-secret --namespace=staging --ignore-not-found=true
        kubectl delete secret app-insights-secret --namespace=staging --ignore-not-found=true
        
        # Create new secrets with values from Key Vault
        kubectl create secret generic db-connection-secret \
          --from-literal=connection-string="${{ env.DB_CONNECTION }}" \
          --namespace=staging
        
        kubectl create secret generic api-key-secret \
          --from-literal=api-key="${{ env.API_KEY }}" \
          --namespace=staging
        
        kubectl create secret generic app-insights-secret \
          --from-literal=connection-string="${{ env.APP_INSIGHTS }}" \
          --namespace=staging
        
        # Verify secrets created
        kubectl get secrets --namespace=staging
    
    # Step 4: Deploy to Staging
    - name: Deploy to AKS Staging
      run: |
        az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
        kubectl set image deployment/sensitive-words-api \
          api=${{ needs.build.outputs.image-tag }} \
          -n staging --record
        kubectl rollout status deployment/sensitive-words-api -n staging

  deploy-production:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment:
      name: production
      url: https://api.sensitive-words.com
    steps:
    - uses: actions/checkout@v3
    
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Retrieve secrets from Key Vault
      id: keyvault
      uses: azure/cli@v1
      with:
        inlineScript: |
          DB_CONNECTION=$(az keyvault secret show --name "db-connection-string" --vault-name ${{ env.KEY_VAULT_NAME }} --query value -o tsv)
          API_KEY=$(az keyvault secret show --name "api-key" --vault-name ${{ env.KEY_VAULT_NAME }} --query value -o tsv)
          APP_INSIGHTS=$(az keyvault secret show --name "app-insights-connection-string" --vault-name ${{ env.KEY_VAULT_NAME }} --query value -o tsv)
          
          echo "::add-mask::$DB_CONNECTION"
          echo "::add-mask::$API_KEY"
          echo "::add-mask::$APP_INSIGHTS"
          
          echo "DB_CONNECTION=$DB_CONNECTION" >> $GITHUB_ENV
          echo "API_KEY=$API_KEY" >> $GITHUB_ENV
          echo "APP_INSIGHTS=$APP_INSIGHTS" >> $GITHUB_ENV
    
    - name: Create Kubernetes Secrets in Production
      run: |
        az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
        
        # Delete existing secrets
        kubectl delete secret db-connection-secret --namespace=production --ignore-not-found=true
        kubectl delete secret api-key-secret --namespace=production --ignore-not-found=true
        kubectl delete secret app-insights-secret --namespace=production --ignore-not-found=true
        
        # Create new secrets
        kubectl create secret generic db-connection-secret \
          --from-literal=connection-string="${{ env.DB_CONNECTION }}" \
          --namespace=production
        
        kubectl create secret generic api-key-secret \
          --from-literal=api-key="${{ env.API_KEY }}" \
          --namespace=production
        
        kubectl create secret generic app-insights-secret \
          --from-literal=connection-string="${{ env.APP_INSIGHTS }}" \
          --namespace=production
    
    - name: Database Migration
      run: |
        az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
        kubectl apply -f k8s/db-migration-job.yaml -n production
        kubectl wait --for=condition=complete job/db-migration-job -n production --timeout=300s
    
    - name: Deploy to Production
      run: |
        az aks get-credentials --resource-group myResourceGroup --name myAKSCluster
        kubectl set image deployment/sensitive-words-api \
          api=${{ needs.build.outputs.image-tag }} \
          -n production --record
        kubectl rollout status deployment/sensitive-words-api -n production
```

### Kubernetes Secrets: Pod Credential Injection

**How pods receive secrets as environment variables:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sensitive-words-api
  namespace: production
spec:
  template:
    spec:
      containers:
      - name: api
        image: myRegistry.azurecr.io/sensitive-words-api:latest
        
        # Environment variables injected from Kubernetes Secrets
        env:
        # From ConfigMap (non-sensitive)
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        
        # From Kubernetes Secret: db-connection-secret
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-connection-secret        # Secret name
              key: connection-string             # Key within secret
        
        # From Kubernetes Secret: api-key-secret
        - name: ApiSettings__ApiKey
          valueFrom:
            secretKeyRef:
              name: api-key-secret
              key: api-key
        
        # From Kubernetes Secret: app-insights-secret
        - name: ApplicationInsights__ConnectionString
          valueFrom:
            secretKeyRef:
              name: app-insights-secret
              key: connection-string
        
        # Optional: Volume mount for secrets (file-based)
        volumeMounts:
        - name: secrets-volume
          mountPath: /var/run/secrets/app
          readOnly: true
      
      # Define volumes for secret files
      volumes:
      - name: secrets-volume
        secret:
          secretName: api-secrets
          defaultMode: 0400  # Read-only for owner
```

### Secret Rotation Strategy

Implement automated secret rotation to enhance security:

```bash
# Rotate API Key in Key Vault
az keyvault secret set \
  --vault-name myKeyVault \
  --name "api-key" \
  --value "NewRotatedKeyValue123456789..."

# Next GitHub Actions deployment will automatically fetch new key
# Kubernetes secrets will be updated on next deployment
```

**Scheduled rotation job (optional):**

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: secret-rotation-notifier
  namespace: production
spec:
  schedule: "0 2 * * 0"  # Weekly, Sunday 2 AM
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: rotator
            image: mcr.microsoft.com/azure-cli:latest
            command:
            - /bin/bash
            - -c
            - |
              az login --service-principal -u $AZURE_CLIENT_ID -p $AZURE_CLIENT_SECRET --tenant $AZURE_TENANT_ID
              
              # Check if secrets were recently rotated
              LAST_ROTATION=$(az keyvault secret show --name "api-key" --vault-name myKeyVault --query properties.updated -o tsv)
              
              # Send notification to ops team
              echo "Last secret rotation: $LAST_ROTATION"
          restartPolicy: OnFailure
```

### ConfigMap for Non-Sensitive Configuration

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: api-config
  namespace: production
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:8080"
  Logging__LogLevel__Default: "Information"
  Logging__LogLevel__Microsoft.AspNetCore: "Warning"
```

### Security Best Practices for Secrets

1. **Never commit secrets to git** - Use `.gitignore` for `appsettings.json`
2. **Use Key Vault as source of truth** - Single source for all secrets
3. **Minimal access permissions** - AKS managed identity only gets "get" and "list" on Key Vault
4. **Audit secret access** - Enable Key Vault audit logging
5. **Rotate regularly** - Implement 90-day rotation policy
6. **Encrypt at rest** - Key Vault and Kubernetes secrets are encrypted
7. **Encrypt in transit** - TLS for all secret transfers
8. **No logs of secrets** - GitHub Actions masks secret values in logs
9. **Pod security policies** - Only authorized pods can mount secrets
10. **RBAC on Key Vault** - Limit who can modify secrets

---

## 7. Monitoring & Observability

### Application Insights Integration

```csharp
// Already configured in appsettings.json
// Add custom instrumentation
builder.Services.AddApplicationInsightsTelemetry();

// Custom metrics
var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();

// Track custom events
telemetryClient.TrackEvent("SensitiveWordFiltered", 
    properties: new Dictionary<string, string>
    {
        { "WordCount", words.Count.ToString() },
        { "MessageLength", message.Length.ToString() }
    },
    metrics: new Dictionary<string, double>
    {
        { "FilterLatencyMs", stopwatch.ElapsedMilliseconds }
    });

// Track exceptions
try 
{
    // operation
}
catch (Exception ex)
{
    telemetryClient.TrackException(ex);
    throw;
}
```

### Azure Monitor Alerts

```bash
# CPU utilization alert
az monitor metrics alert create \
  --name "CPU Alert" \
  --resource-group myResourceGroup \
  --scopes /subscriptions/xxx/resourceGroups/myResourceGroup/providers/Microsoft.Compute/virtualMachines/myVM \
  --condition "avg Percentage CPU > 80" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --action create_action --action-group-id /subscriptions/xxx/resourceGroups/myResourceGroup/providers/microsoft.insights/actionGroups/myActionGroup

# Error rate alert
az monitor metrics alert create \
  --name "Error Rate Alert" \
  --resource-group myResourceGroup \
  --condition "total Failed Requests > 100" \
  --window-size 5m \
  --evaluation-frequency 1m
```

### Log Analytics Queries

```kusto
// Find high-latency requests
requests
| where duration > 1000
| summarize count() by name, bin(timestamp, 5m)
| order by timestamp desc

// Track API key validation failures
traces
| where message contains "Invalid API key"
| summarize count() by tostring(customDimensions.ClientIp), bin(timestamp, 1h)
```

---

## 8. Security Best Practices

### Network Security

```yaml
# Network Policy - Restrict pod communication
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: api-network-policy
  namespace: production
spec:
  podSelector:
    matchLabels:
      app: sensitive-words-api
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 8080
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 1433  # SQL Server
    - protocol: TCP
      port: 6379  # Redis
    - protocol: TCP
      port: 443   # HTTPS outbound
```

### SSL/TLS Certificates

```bash
# Let's Encrypt integration with cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Create cluster issuer
kubectl apply -f - <<EOF
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@sensitive-words.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

### API Key Rotation Strategy

```csharp
// Support multiple active API keys
public class ApiKeyValidator
{
    private readonly IApiKeyService _apiKeyService;
    
    public async Task<bool> ValidateAsync(string providedKey)
    {
        var validKeys = await _apiKeyService.GetActiveKeysAsync();
        return validKeys.Contains(providedKey);
    }
}

// Scheduled job to retire old keys
public class ApiKeyRotationJob : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Deactivate keys older than 90 days
            await _apiKeyService.DeactivateExpiredKeysAsync(TimeSpan.FromDays(90));
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

---

## 9. Disaster Recovery & Business Continuity

### Backup Strategy

```yaml
# Daily backup job
apiVersion: batch/v1
kind: CronJob
metadata:
  name: db-backup-job
  namespace: production
spec:
  schedule: "0 2 * * *"  # 2 AM daily
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: backup
            image: mcr.microsoft.com/azure-cli:latest
            env:
            - name: AZURE_CREDENTIALS
              valueFrom:
                secretKeyRef:
                  name: azure-creds-secret
                  key: credentials
            command:
            - /bin/sh
            - -c
            - |
              az sql db backup create \
                --resource-group myResourceGroup \
                --server sensitive-words-server \
                --database SensitiveWordsDb
          restartPolicy: OnFailure
```

### Recovery Time Objectives (RTO) & Recovery Point Objectives (RPO)

| Scenario | RTO | RPO | Strategy |
|----------|-----|-----|----------|
| Pod failure | < 1 min | < 1 sec | Auto-restart via liveness probe |
| Node failure | < 5 min | < 1 sec | Multi-zone deployment, auto-reschedule |
| Database failure | < 10 min | < 5 min | Automated failover to secondary replica |
| Region failure | < 30 min | < 1 hour | Geo-replicated database backup |
| Data corruption | < 4 hours | < 1 hour | Point-in-time restore from backup |

### Rollback Strategy

```bash
# Quick rollback to previous version
kubectl rollout undo deployment/sensitive-words-api -n production

# Verify previous version
kubectl rollout history deployment/sensitive-words-api -n production

# Rollback to specific revision
kubectl rollout undo deployment/sensitive-words-api --to-revision=2 -n production
```

---

## 10. Scaling Strategy

### Horizontal Scaling (HPA)

Already configured in Kubernetes manifests above:
- Min replicas: 3
- Max replicas: 10
- Scale up at 70% CPU or 80% memory
- Scale down gradually to avoid cascading failures

### Vertical Scaling

Monitor resource usage and adjust limits:

```bash
# Check resource usage
kubectl top pods -n production

# Update resource requests/limits
kubectl set resources deployment sensitive-words-api \
  --limits=cpu=500m,memory=512Mi \
  --requests=cpu=250m,memory=256Mi \
  -n production
```

### Caching Strategy for Scale

```yaml
# Redis deployment for distributed caching
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis-cache
  namespace: production
spec:
  replicas: 1
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
        image: redis:7-alpine
        ports:
        - containerPort: 6379
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

---

## 11. Deployment Checklist

### Pre-Deployment
- [ ] Code review and approval
- [ ] All tests passing (unit, integration, security)
- [ ] Code quality gate passed
- [ ] Security scanning completed (SAST/DAST)
- [ ] Performance baseline established
- [ ] Database schema migrations validated
- [ ] Configuration verified in staging

### Deployment
- [ ] Database migration job runs successfully
- [ ] Container image pushed to registry
- [ ] Kubernetes manifests applied
- [ ] Pods are healthy and running
- [ ] Load balancer configured
- [ ] TLS certificate valid
- [ ] DNS pointing to load balancer

### Post-Deployment
- [ ] Smoke tests passing
- [ ] Error rates normal
- [ ] Response times within SLA
- [ ] Database connectivity healthy
- [ ] Monitoring alerts active
- [ ] Logs flowing to Log Analytics
- [ ] Team notified of deployment

---

## 12. Operational Runbooks

### Incident Response

**High Error Rate Response:**
```bash
# 1. Check logs
kubectl logs -l app=sensitive-words-api -n production --tail=200

# 2. Check pod health
kubectl get pods -n production
kubectl describe pods -n production

# 3. Check resource usage
kubectl top pods -n production

# 4. Rollback if necessary
kubectl rollout undo deployment/sensitive-words-api -n production

# 5. Notify team and document
```

**Database Connection Issues:**
```bash
# 1. Verify connection string in secrets
kubectl get secret db-connection-secret -n production -o yaml

# 2. Check database status
az sql server show --name sensitive-words-server --resource-group myResourceGroup

# 3. Check network connectivity from pod
kubectl exec -it <pod-name> -n production -- curl -v telnet://sensitive-words-server.database.windows.net:1433

# 4. Review firewall rules
az sql server firewall-rule list --server sensitive-words-server --resource-group myResourceGroup
```

---

## Conclusion

This deployment strategy ensures:
- **High Availability**: Multi-zone deployment, auto-scaling, health checks
- **Security**: TLS encryption, secret management, network policies, non-root containers
- **Observability**: Application Insights, log aggregation, custom metrics
- **Reliability**: Automated backups, disaster recovery, rollback capabilities
- **Scalability**: Horizontal and vertical scaling, caching, database replication
- **Maintainability**: Infrastructure as Code (IaC), CI/CD automation, clear runbooks
