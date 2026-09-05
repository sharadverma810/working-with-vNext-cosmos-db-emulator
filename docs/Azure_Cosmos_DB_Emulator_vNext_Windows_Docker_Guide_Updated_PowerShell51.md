# Azure Cosmos DB Emulator vNext on Windows with Docker

## Practical learning guide for .NET, Docker, and Azure Cosmos DB

**Last updated:** 5 September 2026

> **Purpose:** Set up the Linux-based Azure Cosmos DB Emulator vNext on
> a Windows development machine using Docker Desktop, then use it for
> learning Cosmos DB with .NET and other technologies.
>
> **Microsoft documentation:**
> https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux

------------------------------------------------------------------------

# 1. What we are building

The target local development environment is:

``` text
Windows
│
├── Docker Desktop
│     │
│     └── WSL 2
│            │
│            └── Azure Cosmos DB Emulator vNext
│
├── .NET 10
│     │
│     └── ASP.NET Core Web API
│              │
│              └── Azure Cosmos DB .NET SDK
│
└── Other learning projects
       ├── Console applications
       ├── Worker Services
       ├── Dockerized APIs
       ├── Docker Compose
       └── Integration tests
```

Later, the environment can be extended to:

``` text
Local Docker
     │
     ▼
.NET API container
     │
     ▼
Cosmos DB Emulator
```

And eventually to Azure:

``` text
Local application
      │
      │ VPN / appropriate network connectivity
      ▼
Azure VNet
      │
      ▼
Private Endpoint + Private DNS
      │
      ▼
Azure Cosmos DB
```

------------------------------------------------------------------------

# 2. Important: vNext vs the existing Windows emulator

The **Cosmos DB Emulator vNext** and the older Windows Cosmos DB
Emulator can coexist on the same machine, but they cannot both bind to
the same host ports at the same time.

The vNext Docker setup in this guide uses:

``` text
localhost:8081
localhost:8080
localhost:1234
```

The existing Windows emulator commonly uses port `8081` as well.

Therefore:

### When using vNext

``` text
Windows Cosmos Emulator     STOPPED
Docker Cosmos vNext         RUNNING
```

### When using the old emulator

``` text
Windows Cosmos Emulator     RUNNING
Docker Cosmos vNext         STOPPED
```

Stopping the old Windows emulator does **not** delete its data.

Likewise, removing the Docker vNext container does **not** delete the
named Docker volume used by this guide.

------------------------------------------------------------------------

# 3. Important limitation of vNext

The Linux-based Cosmos DB Emulator vNext currently supports the **Azure
Cosmos DB API for NoSQL** in gateway mode, with a selected subset of
Cosmos DB functionality.

It is **not a complete local copy of every Azure Cosmos DB capability**.

It is also **not the emulator to use for an existing Cosmos DB MongoDB
API workload**.

For MongoDB API learning, use the appropriate MongoDB-compatible
emulator/image and MongoDB tooling such as `MongoDB.Driver` and Compass.

Microsoft documentation:

https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux

------------------------------------------------------------------------

# Part 1 --- Verify Docker

## 4. Check Docker

Open **PowerShell**.

Run:

``` powershell
docker --version
```

Then:

``` powershell
docker info
```

This should return Docker information without an error.

Check WSL:

``` powershell
wsl --status
```

You should have WSL 2 available.

------------------------------------------------------------------------

# Part 2 --- Download the Cosmos DB Emulator vNext

## 5. Pull the official Microsoft image

Run:

``` powershell
docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest
```

This downloads the official Linux-based Cosmos DB Emulator vNext image
from Microsoft's container registry.

Verify:

``` powershell
docker images
```

Look for:

``` text
mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator
```

with tag:

``` text
vnext-latest
```

The image is large, so the download may take some time.

------------------------------------------------------------------------

# Part 3 --- Create persistent storage

## 6. Create a Docker volume

Create a named volume so emulator data can survive container
removal/recreation:

``` powershell
docker volume create cosmos-vnext-data
```

Verify:

``` powershell
docker volume ls
```

You should see:

``` text
cosmos-vnext-data
```

### Important

Do not use:

``` powershell
docker volume rm cosmos-vnext-data
```

unless you intentionally want to delete the data stored in that volume.

------------------------------------------------------------------------

# Part 4 --- Start the emulator

## 7. Start vNext in HTTPS mode

For .NET development, use HTTPS.

Run in PowerShell:

``` powershell
docker run -d `
  --name cosmos-vnext `
  -p 8081:8081 `
  -p 8080:8080 `
  -p 1234:1234 `
  -v cosmos-vnext-data:/data `
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest `
  --protocol https
```

### What the ports mean

    Host port   Container port Purpose
  ----------- ---------------- -------------------------
         8081             8081 Cosmos DB gateway
         8080             8080 Health/readiness probes
         1234             1234 Data Explorer

------------------------------------------------------------------------

# Part 5 --- Verify the container

## 8. Check Docker

Run:

``` powershell
docker ps
```

You should see a running container named:

``` text
cosmos-vnext
```

The port mappings should include the ports described above.

If the container is not running, inspect the logs:

``` powershell
docker logs cosmos-vnext
```

------------------------------------------------------------------------

# 6. What happened during our successful setup

A healthy vNext container produced log messages similar to:

``` text
PostgreSQL and pgcosmos extension are ready
Started

PostgreSQL=OK, Gateway=OK, Explorer=OK

System is now fully ready to accept requests

Now listening on: https://0.0.0.0:8081
```

Repeated messages such as:

``` text
PostgreSQL=OK, Gateway=OK, Explorer=OK
```

are a good sign.

The PostgreSQL message is normal for vNext. You do **not** need to
install PostgreSQL separately for the emulator.

The key indicators are:

``` text
PostgreSQL=OK
Gateway=OK
Explorer=OK
```

and:

``` text
System is now fully ready to accept requests
```

------------------------------------------------------------------------

# Part 6 --- Check emulator readiness

## 9. IMPORTANT: use HTTP for port 8080

The health/readiness endpoint on port `8080` is HTTP.

Use:

``` powershell
curl.exe http://localhost:8080/ready
```

Do **not** use:

``` powershell
curl.exe -k https://localhost:8080/ready
```

Using HTTPS on port 8080 can produce a Windows Schannel error such as:

``` text
curl: (35) schannel: failed to receive handshake, SSL/TLS connection failed
```

That does not necessarily mean the emulator is broken; it can simply
mean the health endpoint was accessed using the wrong protocol.

Other health endpoints:

``` powershell
curl.exe http://localhost:8080/alive
```

and:

``` powershell
curl.exe http://localhost:8080/status
```

The readiness check is the most useful one for determining whether the
emulator is ready.

------------------------------------------------------------------------

# Part 7 --- Check the Cosmos HTTPS gateway

## 10. Test port 8081

The Cosmos gateway is HTTPS because we started the emulator with:

``` text
--protocol https
```

For a simple local test:

``` powershell
curl.exe -k https://localhost:8081/
```

The `-k` option temporarily tells curl not to validate the local
certificate.

This is useful for testing only. We will properly trust the emulator
certificate for .NET development.

------------------------------------------------------------------------

# Part 8 --- Open Data Explorer

## 11. Open Data Explorer

Try:

``` text
https://localhost:1234
```

The Data Explorer allows you to:

-   Create databases
-   Create containers
-   Insert documents
-   View documents
-   Run Cosmos queries
-   Experiment with Cosmos DB locally

A typical learning structure will be:

``` text
LearningDB
│
└── Products
      ├── product-001
      ├── product-002
      └── product-003
```

If Data Explorer does not open, first verify:

``` powershell
docker ps
```

and:

``` powershell
curl.exe http://localhost:8080/ready
```

Then inspect:

``` powershell
docker logs cosmos-vnext --tail 50
```

------------------------------------------------------------------------

# Part 9 --- Install the emulator certificate

## 12. Why the certificate matters

The emulator uses HTTPS.

Because the emulator certificate is local/self-signed, Windows and development tools may need to trust it before they can make secure HTTPS connections.

> **Important:** Windows PowerShell 5.1 and PowerShell 7+ handle `SkipCertificateCheck` differently. Use the section below that matches your PowerShell version.

---

## 12.1 Check your PowerShell version

Run:

```powershell
$PSVersionTable.PSVersion
```

If the major version is:

```text
5
```

you are using **Windows PowerShell 5.1**.

If the major version is:

```text
7
```

or greater, use the **PowerShell 7+** instructions.

---

# Option A --- Windows PowerShell 5.1

## 12.2 Download the certificate using curl.exe

Windows PowerShell 5.1 does **not** support:

```powershell
Invoke-WebRequest -SkipCertificateCheck
```

Therefore, use `curl.exe`.

Run:

```powershell
curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt
```

Explanation:

```text
-k
↓
Temporarily ignore the local self-signed certificate

https://localhost:8081/_explorer/emulator.pem
↓
Download the Cosmos Emulator certificate

-o emulatorcert.crt
↓
Save the certificate as emulatorcert.crt
```

Verify that the file was downloaded:

```powershell
dir emulatorcert.crt
```

You should see:

```text
emulatorcert.crt
```

---

## 12.3 Import the certificate in Windows PowerShell 5.1

Run:

```powershell
Import-Certificate `
  -FilePath .\emulatorcert.crt `
  -CertStoreLocation Cert:\CurrentUser\Root
```

A successful result should show certificate information similar to:

```text
PSParentPath : Microsoft.PowerShell.Security\Certificate::CurrentUser\Root
Subject      : CN=localhost
```

This imports the certificate into the **Current User Trusted Root Certification Authorities** store.

---

# Option B --- PowerShell 7 or greater

## 12.4 Download the certificate

PowerShell 7+ supports `SkipCertificateCheck`.

Run:

```powershell
$parameters = @{
    Uri = 'https://localhost:8081/_explorer/emulator.pem'
    Method = 'GET'
    OutFile = 'emulatorcert.crt'
    SkipCertificateCheck = $True
}

Invoke-WebRequest @parameters
```

Verify:

```powershell
dir emulatorcert.crt
```

---

## 12.5 Import the certificate

Run:

```powershell
Import-Certificate `
  -FilePath .\emulatorcert.crt `
  -CertStoreLocation Cert:\CurrentUser\Root
```

---

# 12.6 Why Windows PowerShell 5.1 showed an error

If you run the PowerShell 7+ command in Windows PowerShell 5.1, you may see:

```text
Invoke-WebRequest : A parameter cannot be found
that matches parameter name 'SkipCertificateCheck'.
```

That is because `SkipCertificateCheck` is not available in Windows PowerShell 5.1.

For PowerShell 5.1, use:

```powershell
curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt
```

Then import the certificate:

```powershell
Import-Certificate `
  -FilePath .\emulatorcert.crt `
  -CertStoreLocation Cert:\CurrentUser\Root
```

---

# 12.7 Verify the certificate installation

Run:

```powershell
Get-ChildItem Cert:\CurrentUser\Root |
Where-Object { $_.Subject -like "*localhost*" }
```

You should see a certificate with a subject similar to:

```text
CN=localhost
```

---

# 12.8 Security note

The `-k` option in `curl.exe` is used only to download the emulator's local self-signed certificate.

After importing the certificate, your .NET application should use normal HTTPS certificate validation.

Do **not** permanently disable TLS/SSL certificate validation in your application code.

---

# Part 10 --- Check .NET

## 13. Check your .NET SDK

Run:

``` powershell
dotnet --version
```

To see all installed SDKs:

``` powershell
dotnet --list-sdks
```

For the learning project, use .NET 10 if it is installed.

------------------------------------------------------------------------

# Part 11 --- Create the first .NET API

## 14. Create a project

Create a learning folder:

``` powershell
mkdir CosmosLearning
cd CosmosLearning
```

Create an ASP.NET Core Web API:

``` powershell
dotnet new webapi -n CosmosLearning.Api
```

Move into the project:

``` powershell
cd CosmosLearning.Api
```

Install the official Cosmos DB SDK:

``` powershell
dotnet add package Microsoft.Azure.Cosmos
```

Restore packages:

``` powershell
dotnet restore
```

------------------------------------------------------------------------

# Part 12 --- First application architecture

The first application will follow:

``` text
HTTP Request
     │
     ▼
ASP.NET Core Web API
     │
     ▼
CosmosClient
     │
     ▼
Microsoft.Azure.Cosmos SDK
     │
     ▼
HTTPS
     │
     ▼
localhost:8081
     │
     ▼
Cosmos DB Emulator
```

We will build APIs such as:

``` text
POST   /products
GET    /products
GET    /products/{id}
PUT    /products/{id}
DELETE /products/{id}
```

------------------------------------------------------------------------

# Part 13 --- First database and container

## 15. Create a database

Create:

``` text
Database: LearningDB
```

Then create:

``` text
Container: Products
```

For the first learning exercise, use:

``` text
Partition key: /category
```

Example document:

``` json
{
  "id": "p001",
  "name": "Laptop",
  "category": "electronics",
  "price": 75000
}
```

------------------------------------------------------------------------

# Part 14 --- Local connection settings

## 16. Emulator connection

The emulator exposes its Cosmos gateway at:

``` text
https://localhost:8081/
```

For local development, use the emulator's well-known account key or
configure a key explicitly according to the current Microsoft
documentation.

Keep the connection string in application configuration rather than
hard-coding it into source code.

Conceptually:

``` text
AccountEndpoint=https://localhost:8081/;
AccountKey=<emulator-account-key>;
```

We will configure this properly in the .NET project.

------------------------------------------------------------------------

# Part 15 --- Learning roadmap

Work through the exercises in this order.

## Exercise 1 --- CRUD

Learn:

``` text
Create
Read
Update
Delete
```

------------------------------------------------------------------------

## Exercise 2 --- Cosmos SQL queries

Example:

``` sql
SELECT *
FROM c
WHERE c.category = "electronics"
```

Learn how Cosmos DB SQL queries differ from relational SQL.

------------------------------------------------------------------------

## Exercise 3 --- Partition keys

Understand:

``` text
Partition Key
      │
      ▼
/category
```

Learn:

-   Logical partitions
-   Physical partitions
-   Partition-key selection
-   Hot partitions
-   Cross-partition queries

------------------------------------------------------------------------

## Exercise 4 --- Point reads vs queries

Compare:

``` text
Point Read
    vs
Query
```

Learn why a point read can be more efficient when you know both the item
ID and partition key.

------------------------------------------------------------------------

## Exercise 5 --- Pagination

Build:

``` text
GET /products?pageSize=10
```

Use Cosmos continuation tokens.

------------------------------------------------------------------------

## Exercise 6 --- Optimistic concurrency

Learn:

``` text
ETag
If-Match
```

Build an API that prevents accidental overwrites.

------------------------------------------------------------------------

## Exercise 7 --- TTL

Configure documents to expire automatically.

Potential uses:

``` text
Temporary sessions
Cache-like data
Short-lived events
```

------------------------------------------------------------------------

## Exercise 8 --- Change Feed

Build:

``` text
Cosmos DB
    │
    ▼
Change Feed
    │
    ▼
.NET Worker Service
    │
    ▼
Process event
```

------------------------------------------------------------------------

## Exercise 9 --- Dockerize the .NET API

Eventually run:

``` text
Docker
│
├── Cosmos Emulator
│
└── .NET API
```

Learn:

-   Dockerfiles
-   Container networking
-   Environment variables
-   Health checks
-   Container-to-container communication

------------------------------------------------------------------------

## Exercise 10 --- Docker Compose

Create a Compose environment such as:

``` text
docker-compose
│
├── api
│
├── worker
└── cosmos
```

Then start it with:

``` powershell
docker compose up
```

------------------------------------------------------------------------

## Exercise 11 --- Integration testing

Use the emulator for automated integration tests.

Target:

``` text
Developer machine
        │
        ▼
Cosmos Emulator
        │
        ▼
Integration Tests
```

Later:

``` text
CI/CD pipeline
        │
        ▼
Cosmos Emulator container
        │
        ▼
Automated tests
```

------------------------------------------------------------------------

# Part 16 --- Move from local Cosmos to Azure

Once the local application works, use the same application against a
real Azure Cosmos DB account.

## Stage 1 --- Local

``` text
.NET API
   │
   ▼
Cosmos Emulator
```

## Stage 2 --- Azure

``` text
.NET API
   │
   ▼
Azure Cosmos DB
```

## Stage 3 --- Secure Azure networking

``` text
.NET application
        │
        ▼
     Azure VNet
        │
        ▼
 Private Endpoint
        │
        ▼
   Private DNS
        │
        ▼
 Azure Cosmos DB
```

A local Docker container does **not** automatically become part of an
Azure VNet.

If a local application needs to reach a Cosmos DB private endpoint,
appropriate network connectivity into Azure is required. We will cover
this separately using the correct VPN/Point-to-Site and DNS
architecture.

------------------------------------------------------------------------

# Part 17 --- NoSQL vs MongoDB API

If you are continuing your existing MongoDB API work, keep that
environment separate.

## vNext

``` text
Cosmos DB Emulator vNext
        │
        └── API for NoSQL
```

Typical .NET client:

``` text
Microsoft.Azure.Cosmos
```

## MongoDB API

``` text
Cosmos DB
        │
        └── API for MongoDB
                 │
                 ├── MongoDB.Driver
                 └── MongoDB Compass
```

Do not assume that an existing MongoDB API database/data set can simply
be opened by the NoSQL vNext emulator.

------------------------------------------------------------------------

# Part 18 --- Useful Docker commands

## Start

``` powershell
docker start cosmos-vnext
```

## Stop

``` powershell
docker stop cosmos-vnext
```

## Status

``` powershell
docker ps
```

## All containers

``` powershell
docker ps -a
```

## Logs

``` powershell
docker logs cosmos-vnext
```

## Last 50 log lines

``` powershell
docker logs cosmos-vnext --tail 50
```

## Follow logs

``` powershell
docker logs -f cosmos-vnext
```

Press `Ctrl+C` to stop following logs. This does not stop the container.

## Restart

``` powershell
docker restart cosmos-vnext
```

## Remove the container

``` powershell
docker rm -f cosmos-vnext
```

Removing the container does **not** remove:

``` text
cosmos-vnext-data
```

## List volumes

``` powershell
docker volume ls
```

------------------------------------------------------------------------

# Part 19 --- Troubleshooting

## Problem 1 --- Port 8081 is unavailable

Typical error:

``` text
ports are not available
listen tcp 0.0.0.0:8081
```

Most likely another application is already using port 8081.

Check:

``` powershell
netstat -ano | findstr :8081
```

If you use the old Windows Cosmos Emulator, stop it before starting
vNext.

Alternatively, we can deliberately map a different host port, for
example:

``` text
Host 18081 → Container 8081
```

using:

``` powershell
-p 18081:8081
```

Then the host-side Cosmos endpoint would be:

``` text
https://localhost:18081/
```

Do not change the container's internal port unless Microsoft
documentation specifically requires it.

------------------------------------------------------------------------

## Problem 2 --- Container name already exists

Typical error:

``` text
Conflict. The container name "/cosmos-vnext"
is already in use
```

Check:

``` powershell
docker ps -a
```

If the existing `cosmos-vnext` container is a failed/old container and
you intentionally want to recreate it:

``` powershell
docker rm cosmos-vnext
```

Then run the `docker run` command again.

This does not remove the named volume:

``` text
cosmos-vnext-data
```

------------------------------------------------------------------------

## Problem 3 --- Health check gives a Schannel SSL error

If you run:

``` powershell
curl.exe -k https://localhost:8080/ready
```

and get:

``` text
curl: (35) schannel:
failed to receive handshake,
SSL/TLS connection failed
```

use HTTP instead:

``` powershell
curl.exe http://localhost:8080/ready
```

Port 8080 is the health/readiness endpoint.

For the Cosmos gateway on 8081, use HTTPS:

``` powershell
curl.exe -k https://localhost:8081/
```

------------------------------------------------------------------------

## Problem 4 --- Data Explorer does not open

Check:

``` powershell
docker ps
```

Then:

``` powershell
curl.exe http://localhost:8080/ready
```

Then:

``` powershell
docker logs cosmos-vnext --tail 50
```

Look for:

``` text
PostgreSQL=OK, Gateway=OK, Explorer=OK
```

and:

``` text
System is now fully ready to accept requests
```

------------------------------------------------------------------------

## Problem 5 --- Certificate errors in .NET

Make sure the emulator certificate was downloaded and imported into:

``` text
Cert:\CurrentUser\Root
```

Do not permanently disable certificate validation in the application.

------------------------------------------------------------------------

## Problem 6 --- Data seems to disappear

Make sure the container was started with:

``` text
-v cosmos-vnext-data:/data
```

Check:

``` powershell
docker volume ls
```

The named volume should exist:

``` text
cosmos-vnext-data
```

Do not remove the volume unless you intentionally want to remove the
stored emulator data.

------------------------------------------------------------------------

# Part 20 --- Recommended learning sequence

Follow this order:

``` text
01. Docker basics
       ↓
02. Cosmos Emulator vNext
       ↓
03. Data Explorer
       ↓
04. Cosmos DB concepts
       ↓
05. .NET Cosmos SDK
       ↓
06. CRUD
       ↓
07. Queries
       ↓
08. Partitioning
       ↓
09. Pagination
       ↓
10. ETags / concurrency
       ↓
11. TTL
       ↓
12. Change Feed
       ↓
13. .NET Worker
       ↓
14. Dockerize .NET
       ↓
15. Docker Compose
       ↓
16. Integration tests
       ↓
17. Azure Cosmos DB
       ↓
18. Private Endpoint
       ↓
19. Private DNS
       ↓
20. Azure VNet + local connectivity
```

------------------------------------------------------------------------

# Part 21 --- Current successful setup checkpoint

At this point, the known-good setup is:

``` text
Windows
│
├── Existing Windows Cosmos Emulator
│       └── STOPPED while vNext uses 8081
│
└── Docker Desktop
        │
        └── cosmos-vnext
                │
                ├── PostgreSQL / pgcosmos  ✅
                ├── Gateway                ✅
                ├── Explorer               ✅
                ├── HTTPS                  ✅
                │
                └── localhost:8081
```

A healthy log contains:

``` text
PostgreSQL=OK, Gateway=OK, Explorer=OK
```

and:

``` text
Now listening on: https://0.0.0.0:8081
```

The next practical milestone is:

``` text
Docker Cosmos vNext
        ↓
Data Explorer
        ↓
Certificate trust
        ↓
.NET 10 Web API
        ↓
Cosmos SDK
        ↓
CRUD
```

Do not move to Azure VNet/Private Endpoint until the local .NET
application is successfully reading and writing data to the emulator.

------------------------------------------------------------------------

# Microsoft references

-   Azure Cosmos DB Emulator vNext:
    https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux

-   Develop locally with the emulator:
    https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator

-   Azure Cosmos DB Emulator:
    https://learn.microsoft.com/en-us/azure/cosmos-db/emulator

------------------------------------------------------------------------

## Final note

The emulator is a **development and testing environment**, not a
production database.

Always validate production-specific behavior against the real Azure
Cosmos DB service before deployment.

For vNext, check Microsoft's current feature-support and limitations
documentation before relying on a particular feature in an application.
