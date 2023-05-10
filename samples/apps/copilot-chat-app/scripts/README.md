# Copilot Chat Setup Scripts

> The PowerShell scripts in this directory require [PowerShell Core 6 or higher](https://github.com/PowerShell/PowerShell#get-powershell).

## Before You Begin
To run these scripts, you will need the following:
- *REACT_APP_CLIENT_ID*
  - This is the client ID (also known as application ID) associated with your Azure Active Directory (AAD) application registration, which you can find in the Azure portal.
- *REACT_APP_TENANT*
  - This is the tenant associated with your AAD app registration.
  [Learn more about possible values for this parameter](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-client-application-configuration#authority).
- *MY_AZUREOPENAI_OR_OPENAI_KEY*
  - This is your API key for Azure OpenAI or OpenAI

You also need to update [`appsettings.json`](../webapi/appsettings.json) with the relevant deployment or model information, as well as endpoint if you are using Azure OpenAI.

For more information on how to prepare this data, [see the full instructions for Copilot Chat](../README.md).

## Install Requirements
### Windows
Open a PowerShell window as an administrator, navigate to this directory, and run the following command:
```powershell
.\Install-Requirements.ps1
```
This will install .NET 6.0 SDK, Node.js, and Yarn using the Chocolatey package manager.

For all other operating systems, you need to ensure that these requirements are already installed before proceeding.

## Run Copilot Chat locally
The `Start` script initializes and runs both the backend and frontend for Copilot Chat on your local machine.

### PowerShell
Open a PowerShell window, navigate to this directory, and run the following command:

```powershell
.\Start.ps1 -ClientId REACT_APP_CLIENT_ID -Tenant REACT_APP_TENANT -AzureOpenAIOrOpenAIKey MY_AZUREOPENAI_OR_OPENAI_KEY
```

### Bash
Open a Bash window and navigate to this directory. First, ensure the `Start.sh` script is executable:
```bash
chmod +x Start.sh
```

Then run the following command:
```bash
./Start.sh REACT_APP_CLIENT_ID REACT_APP_TENANT MY_AZUREOPENAI_OR_OPENAI_KEY
```
Note that this script starts `CopilotChatApi.exe` as a background process. Be sure to terminate it when you are finished.
