param location string = 'AustraliaEast'
param resourceGroupName string = 'nicoscrape-rg'
param postgreSqlServerName string = 'server-${uniqueString(deployment().name)}'
param postgreSqlServerAdminUsername string = 'nicoscrapeAdmin'
param postgreSqlDatabaseName string = 'db-${uniqueString(deployment().name)}'

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

module vnetModule './modules/vnet.bicep' = {
  name: 'vnetDeploy'
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
  }
}

module dbModule './modules//postgres.bicep' = {
  name: 'dbDeploy'
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
    outboundSubnetId: vnetModule.outputs.outboundSubnetId
    postgreSqlServerAdminPwd: masterKv.getSecret('nicoscrape-db-admin-password')
    postgreSqlServerAdminUsername: postgreSqlServerAdminUsername
    privateDnsZoneId: vnetModule.outputs.privateDnsZoneId
  }
}

module appServiceModule './modules/web-app.bicep' = {
  name: 'appServiceDeploy'
  dependsOn: [
    rgModule
    dbModule
  ]
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
    name: 'nicoscrape-webapp'
    postgreSqlServerAdminPwd: masterKv.getSecret('nicoscrape-db-admin-password')
    postgreSqlServerAdminUsername: postgreSqlServerAdminUsername
    postgreSqlDatabaseName: postgreSqlDatabaseName
    postgreSqlServerName: postgreSqlServerName
    subnetId: vnetModule.outputs.subnetId
  }
}
