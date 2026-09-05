# Azure Cosmos DB vNext Emulator on Windows

## Complete HTTPS Certificate Setup Guide for Docker Desktop

> **Goal:** Configure the Azure Cosmos DB vNext Emulator running in
> Docker Desktop on Windows so that Windows, `curl.exe`, and .NET
> applications trust its HTTPS certificate correctly.

This guide is based on a real setup and covers the exact issue where:

``` text
curl: (60) schannel: SEC_E_UNTRUSTED_ROOT
The certificate chain was issued by an authority that is not trusted.
```

and .NET reports:

``` text
The SSL connection could not be established, see inner exception.
```

------------------------------------------------------------------------

# 1. Prerequisites

You need:

-   Windows 10 or Windows 11
-   Docker Desktop running
-   Linux containers enabled in Docker Desktop
-   PowerShell
-   `curl.exe`
-   .NET SDK if using .NET

------------------------------------------------------------------------

# 2. The Most Important Thing to Understand

The Cosmos DB vNext Emulator certificate endpoint can return a **PEM
bundle containing two certificates**.

Conceptually:

``` text
Cosmos DB vNext Emulator
        |
        +-- Certificate 1: Server / Leaf certificate
        |
        +-- Certificate 2: Root / CA certificate
```

A common mistake is to import the first certificate into Windows Trusted
Root.

That can still produce:

``` text
SEC_E_UNTRUSTED_ROOT
```

The reliable process is:

1.  Download the complete PEM bundle.
2.  Extract the individual certificates.
3.  Save the server and root certificates separately.
4.  Import the **root/CA certificate** into Windows Trusted Root.
5.  Verify HTTPS **without `-k`**.

------------------------------------------------------------------------

# 3. Start the Cosmos DB vNext Emulator

Example Docker command:

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

Check the container:

``` powershell
docker ps
```

You should see the `cosmos-vnext` container running.

------------------------------------------------------------------------

# 4. Wait Until the Emulator Is Ready

Run:

``` powershell
docker logs cosmos-vnext --tail 50
```

Wait for messages similar to:

``` text
PostgreSQL=OK, Gateway=OK, Explorer=OK
```

and:

``` text
System is now fully ready to accept requests
```

and:

``` text
Now listening on: https://0.0.0.0:8081
```

Do not continue until the emulator is ready.

------------------------------------------------------------------------

# 5. Use the Correct Endpoint

Use:

``` text
https://localhost:8081/
```

Do not accidentally use port `8080` for the HTTPS endpoint.

Prefer `localhost` unless you have a specific reason to use another
host.

------------------------------------------------------------------------

# 6. Important PowerShell 5.1 Rule

For these instructions, use:

``` powershell
curl.exe
```

not plain:

``` powershell
curl
```

This is especially important with Windows PowerShell 5.1, where
PowerShell aliases can behave differently.

Also, do not use:

``` powershell
Invoke-WebRequest -SkipCertificateCheck
```

in Windows PowerShell 5.1 because that parameter is not available there.

------------------------------------------------------------------------

# 7. Download the Emulator Certificate Bundle

Initially Windows does not trust the emulator certificate. Therefore,
use `-k` only for the initial download:

``` powershell
curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt
```

Explanation:

-   `curl.exe` = Windows curl executable.
-   `-k` = temporarily bypass certificate validation.
-   `-o emulatorcert.crt` = save the downloaded certificate bundle.

Check that the file exists:

``` powershell
dir emulatorcert.crt
```

------------------------------------------------------------------------

# 8. Do Not Immediately Trust the First Certificate

Do not assume this is enough:

``` powershell
Get-PfxCertificate .\emulatorcert.crt
```

A PEM file can contain multiple certificates. The command may lead you
to inspect only one certificate.

First, check how many certificates exist.

------------------------------------------------------------------------

# 9. Count the Certificates in the PEM Bundle

Run:

``` powershell
$pem = Get-Content .\emulatorcert.crt -Raw

$certificates = [regex]::Matches(
    $pem,
    '-----BEGIN CERTIFICATE-----.*?-----END CERTIFICATE-----',
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)

$certificates.Count
```

In the working setup that produced this guide, the result was:

``` text
2
```

If you get `2`, continue.

------------------------------------------------------------------------

# 10. Split the Certificate Bundle

Save the first certificate:

``` powershell
$certificates[0].Value |
Set-Content .\cosmos-server.crt -Encoding ASCII
```

Save the second certificate:

``` powershell
$certificates[1].Value |
Set-Content .\cosmos-root.crt -Encoding ASCII
```

Verify:

``` powershell
dir cosmos-*.crt
```

You should see:

``` text
cosmos-root.crt
cosmos-server.crt
```

------------------------------------------------------------------------

# 11. Inspect the Server Certificate

Run:

``` powershell
Get-PfxCertificate .\cosmos-server.crt |
Format-List Subject,Issuer,Thumbprint,NotBefore,NotAfter
```

You may see values similar to:

``` text
Subject    : CN=localhost, O=SqlPostgresHostConsole, ...
Issuer     : CN=localhost, O=SqlPostgresHostConsole, ...
Thumbprint : ...
NotBefore  : ...
NotAfter   : ...
```

The exact thumbprint is specific to your machine and emulator.

Do not copy a thumbprint from another machine.

------------------------------------------------------------------------

# 12. Inspect the Root Certificate

Run:

``` powershell
Get-PfxCertificate .\cosmos-root.crt |
Format-List Subject,Issuer,Thumbprint,NotBefore,NotAfter
```

This is the certificate that should be used for the trust setup
described in this guide.

Keep the thumbprint available for verification.

------------------------------------------------------------------------

# 13. Import the Root Certificate for the Current User

Run:

``` powershell
Import-Certificate `
    -FilePath .\cosmos-root.crt `
    -CertStoreLocation Cert:\CurrentUser\Root
```

This imports the root certificate into:

``` text
Current User
    └── Trusted Root Certification Authorities
```

------------------------------------------------------------------------

# 14. Verify the Current User Root Store

Run:

``` powershell
$rootCert = Get-PfxCertificate .\cosmos-root.crt

Get-ChildItem Cert:\CurrentUser\Root |
Where-Object {
    $_.Thumbprint -eq $rootCert.Thumbprint
} |
Format-List Subject,Issuer,Thumbprint,NotBefore,NotAfter
```

You should see the exact certificate.

------------------------------------------------------------------------

# 15. Recommended: Import into Local Machine Root Too

This is optional but useful for system-wide trust.

## 15.1 Open PowerShell as Administrator

1.  Close PowerShell.
2.  Open Start.
3.  Search for **Windows PowerShell**.
4.  Right-click it.
5.  Select **Run as Administrator**.

## 15.2 Go to the certificate folder

Example:

``` powershell
cd C:\Users\ABC\source\repos\your-project
```

## 15.3 Import the root certificate

``` powershell
Import-Certificate `
    -FilePath .\cosmos-root.crt `
    -CertStoreLocation Cert:\LocalMachine\Root
```

------------------------------------------------------------------------

# 16. Verify the Local Machine Root Store

Run:

``` powershell
$rootCert = Get-PfxCertificate .\cosmos-root.crt

Get-ChildItem Cert:\LocalMachine\Root |
Where-Object {
    $_.Thumbprint -eq $rootCert.Thumbprint
} |
Format-List Subject,Issuer,Thumbprint,NotBefore,NotAfter
```

You should see the exact certificate.

------------------------------------------------------------------------

# 17. Alternative Import Method: certutil

If necessary, open PowerShell as Administrator and run:

``` powershell
certutil -f -addstore "Root" .\cosmos-root.crt
```

A successful result should indicate that the certificate was added.

------------------------------------------------------------------------

# 18. Do Not Delete All localhost Certificates

You may see several certificates with:

``` text
CN=localhost
```

They may belong to:

-   Visual Studio
-   ASP.NET Core development certificates
-   Other development tools
-   Older local development environments

**Do not delete all of them.**

Always identify the Cosmos emulator certificate using its exact
thumbprint.

Safe example:

``` powershell
$rootCert = Get-PfxCertificate .\cosmos-root.crt

Get-ChildItem Cert:\CurrentUser\Root |
Where-Object { $_.Thumbprint -eq $rootCert.Thumbprint }
```

------------------------------------------------------------------------

# 19. Restart Development Applications

After importing the certificate, close:

-   PowerShell
-   Windows Terminal
-   Visual Studio
-   VS Code
-   Any running .NET application

Then reopen them.

Normally, you do not need to recreate the Cosmos Docker container merely
because the certificate was imported.

------------------------------------------------------------------------

# 20. Verify HTTPS Without -k

This is the final and most important certificate test:

``` powershell
curl.exe https://localhost:8081/_explorer/emulator.pem
```

There must be **no `-k`**.

Expected result:

``` text
-----BEGIN CERTIFICATE-----
```

You should not see:

``` text
SEC_E_UNTRUSTED_ROOT
```

If this command succeeds, Windows HTTPS trust is working.

------------------------------------------------------------------------

# 21. Diagnostic Command: curl.exe -vk

If normal curl fails, run:

``` powershell
curl.exe -vk https://localhost:8081/_explorer/emulator.pem
```

Explanation:

-   `-v` = verbose information.
-   `-k` = temporarily bypass certificate validation.

This command is useful to verify:

-   Docker networking
-   Port 8081 connectivity
-   TLS connection
-   HTTP response
-   Certificate download

If `curl.exe -vk` works but normal `curl.exe` fails, the likely problem
is certificate trust.

------------------------------------------------------------------------

# 22. Why the Verbose Test Was Important

A successful command can return:

``` text
HTTP/1.1 200 OK
```

followed by:

``` text
-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----

-----BEGIN CERTIFICATE-----
...
-----END CERTIFICATE-----
```

This proves that the downloaded PEM file contains multiple certificate
blocks.

That is why importing only the first certificate can fail.

------------------------------------------------------------------------

# 23. Common Error: SEC_E_UNTRUSTED_ROOT

Example:

``` text
curl: (60) schannel: SEC_E_UNTRUSTED_ROOT (0x80090325)
The certificate chain was issued by an authority that is not trusted.
```

## Cause

Windows does not trust the certificate chain.

## Reliable fix

1.  Download:

``` powershell
curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt
```

2.  Extract certificates.

3.  Save:

``` text
cosmos-server.crt
cosmos-root.crt
```

4.  Import `cosmos-root.crt`.

5.  Verify:

``` powershell
curl.exe https://localhost:8081/_explorer/emulator.pem
```

------------------------------------------------------------------------

# 24. Common Error: .NET SSL Connection Could Not Be Established

Example:

``` text
The SSL connection could not be established, see inner exception.
```

Before changing .NET code, verify:

``` powershell
curl.exe https://localhost:8081/_explorer/emulator.pem
```

If curl does not work without `-k`, fix the Windows certificate setup
first.

Do not permanently disable SSL certificate validation in your .NET
application.

The preferred solution is to trust the emulator's root certificate.

------------------------------------------------------------------------

# 25. Common Error: Port 8081 Is Already in Use

Docker may report that port `8081` is unavailable.

A possible reason is another local Cosmos DB Emulator or application
using the port.

Check:

``` powershell
netstat -ano | findstr :8081
```

If another Cosmos emulator is using port 8081, stop it before starting
the Docker vNext emulator on the same port.

------------------------------------------------------------------------

# 26. Common Error: Container Name Already Exists

Example:

``` text
Conflict. The container name "/cosmos-vnext" is already in use
```

Check:

``` powershell
docker ps -a
```

If the container already exists, start it:

``` powershell
docker start cosmos-vnext
```

Do not create another container with the same name.

------------------------------------------------------------------------

# 27. Check Emulator Health

Run:

``` powershell
docker ps
```

Then:

``` powershell
docker logs cosmos-vnext --tail 50
```

Look for messages similar to:

``` text
PostgreSQL=OK
Gateway=OK
Explorer=OK
```

------------------------------------------------------------------------

# 28. Diagnostic: Inspect the Certificate Presented on Port 8081

Use this only for troubleshooting:

``` powershell
$tcpClient = New-Object System.Net.Sockets.TcpClient("localhost", 8081)

$sslStream = New-Object System.Net.Security.SslStream(
    $tcpClient.GetStream(),
    $false,
    { param($sender, $certificate, $chain, $sslPolicyErrors) $true }
)

$sslStream.AuthenticateAsClient("localhost")

$certificate = New-Object `
    System.Security.Cryptography.X509Certificates.X509Certificate2(
        $sslStream.RemoteCertificate
    )

$certificate |
Format-List Subject,Issuer,Thumbprint,NotBefore,NotAfter

$sslStream.Close()
$tcpClient.Close()
```

The callback temporarily accepts the certificate so the script can
inspect it. It does not permanently change Windows trust settings.

------------------------------------------------------------------------

# 29. Do Not Recreate the Container Unnecessarily

During certificate troubleshooting, avoid repeatedly running:

``` powershell
docker rm -f cosmos-vnext
```

and recreating the emulator.

Instead:

1.  Confirm the container is running.
2.  Download the certificate from the currently running emulator.
3.  Split the certificate bundle.
4.  Import the correct root certificate.
5.  Test HTTPS.

------------------------------------------------------------------------

# 30. Complete First-Time Setup Checklist

## A. Start Docker Desktop

Make sure Docker Desktop is running.

## B. Start Cosmos vNext

Start or create the container.

## C. Wait for readiness

``` powershell
docker logs cosmos-vnext --tail 50
```

## D. Download certificate bundle

``` powershell
curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt
```

## E. Extract certificates

``` powershell
$pem = Get-Content .\emulatorcert.crt -Raw

$certificates = [regex]::Matches(
    $pem,
    '-----BEGIN CERTIFICATE-----.*?-----END CERTIFICATE-----',
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)
```

## F. Check count

``` powershell
$certificates.Count
```

## G. Split certificates

``` powershell
$certificates[0].Value |
Set-Content .\cosmos-server.crt -Encoding ASCII

$certificates[1].Value |
Set-Content .\cosmos-root.crt -Encoding ASCII
```

## H. Inspect root certificate

``` powershell
Get-PfxCertificate .\cosmos-root.crt |
Format-List Subject,Issuer,Thumbprint,NotBefore,NotAfter
```

## I. Import root certificate

``` powershell
Import-Certificate `
    -FilePath .\cosmos-root.crt `
    -CertStoreLocation Cert:\CurrentUser\Root
```

## J. Optional system-wide import

Run as Administrator:

``` powershell
Import-Certificate `
    -FilePath .\cosmos-root.crt `
    -CertStoreLocation Cert:\LocalMachine\Root
```

## K. Restart applications

Close and reopen PowerShell and your IDE.

## L. Final test

``` powershell
curl.exe https://localhost:8081/_explorer/emulator.pem
```

No `-k`.

------------------------------------------------------------------------

# 31. Quick Copy-and-Paste Script

Run after the emulator is fully ready:

``` powershell
# Download the certificate bundle.
curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt

# Read the PEM file.
$pem = Get-Content .\emulatorcert.crt -Raw

# Extract certificate blocks.
$certificates = [regex]::Matches(
    $pem,
    '-----BEGIN CERTIFICATE-----.*?-----END CERTIFICATE-----',
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)

# Display certificate count.
$certificates.Count

# Save server certificate.
$certificates[0].Value |
Set-Content .\cosmos-server.crt -Encoding ASCII

# Save root certificate.
$certificates[1].Value |
Set-Content .\cosmos-root.crt -Encoding ASCII

# Inspect root certificate.
Get-PfxCertificate .\cosmos-root.crt |
Format-List Subject,Issuer,Thumbprint,NotBefore,NotAfter

# Import root certificate for current user.
Import-Certificate `
    -FilePath .\cosmos-root.crt `
    -CertStoreLocation Cert:\CurrentUser\Root
```

Then:

``` powershell
curl.exe https://localhost:8081/_explorer/emulator.pem
```

------------------------------------------------------------------------

# 32. Final Security Notes

-   Use `-k` only to download the certificate before trust is
    configured.
-   Do not use `-k` as a permanent solution.
-   Do not permanently disable SSL validation in .NET.
-   Do not delete all `CN=localhost` certificates.
-   Identify the correct certificate by thumbprint.
-   Do not copy thumbprints from another machine.
-   Download certificates from the currently running emulator.
-   If the emulator certificate changes, repeat this setup.

------------------------------------------------------------------------

# 33. Final Summary

The key lesson is:

> **The Cosmos DB vNext Emulator `emulator.pem` can contain multiple
> certificates.**

If you see:

``` text
SEC_E_UNTRUSTED_ROOT
```

do not keep importing the first certificate repeatedly.

Instead:

1.  Download the PEM bundle.
2.  Split the certificates.
3.  Identify the root/CA certificate.
4.  Import the root certificate into Windows Trusted Root.
5.  Restart development applications.
6.  Verify with `curl.exe` without `-k`.
7.  Run the .NET application.

When:

``` powershell
curl.exe https://localhost:8081/_explorer/emulator.pem
```

works without `-k`, your Windows certificate setup is ready for Cosmos
DB vNext Emulator development.
