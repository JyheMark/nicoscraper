param location string = 'AustraliaEast'
param resourceGroupName string = 'nicoscrape-rg'
param dbAdminUsername string = 'nicoscrapeAdmin'

module rgModule 'modules/resource-group.bicep' = {
  name: 'rgDeploy'
  scope: subscription()
  params: {
    location: location
    name: resourceGroupName
  }
}

resource masterKv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: 'kv-meta-master'
  scope: resourceGroup('meta')
}

module appServiceModule './modules/web-app.bicep' = {
  name: 'appServiceDeploy'
  dependsOn: [
    rgModule
  ]
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
    name: 'nicoscrape-webapp'
    postgreSqlServerAdminPwd: masterKv.getSecret('nicoscrape-db-admin-password')
    postgreSqlServerAdminUsername: dbAdminUsername
  }
}
