# How to setup Azure

## Setup a WebAPI App Registration in Azure AD

This will be used by unattended processes that deploy the application to Azure, like TeamCity. You'll want to name the App Registration accordingly.

### Get the App Registration Key
#### Client Id
#### Client Secret
## Get the IDs
### Subscriber ID
### Application Id
### Directory Id
## Setup a Resource Group
## Give Contributor access to the App Registration
## Setup Azure Blob Storage for logging
## DO NOT STORE SECRETS UNSECURED. YOU DO NOT NEED ANY KEYS OR CONNECTION STRING ON YOUR

## Setup a Native App Registration in Azure AD



## Security
COMPUTER. PUT THEM IN TEAM CITY ONLY

MAKE SURE TO SEGREGATE SERVICES TO KEEP THEM SECURE

## Example

fake build --target deploy -e resourceGroupName=COM-Experimental -e subscriptionId=GUID -e clientId=GUID -e environment=experimental -e location=southcentralus -e tenantId=GUID -e clientSecret=SECRETKEY