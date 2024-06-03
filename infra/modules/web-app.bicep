param name string
param location string
param subnetId string
param postgreSqlDatabaseName string
param postgreSqlServerName string
param postgreSqlServerAdminUsername string

@secure()
param postgreSqlServerAdminPwd string

resource hostingPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'ASP-nicoscraperg-9709'
  location: location
  kind: 'linux'
  tags: {
    'managed-by': 'Bicep'
  }
  properties: {
    targetWorkerCount: 1
    reserved: true
    zoneRedundant: false
  }
  sku: {
    tier: 'Basic'
    name: 'B1'
  }
  dependsOn: []
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  tags: {
    'managed-by': 'Bicep'
  }
  properties: {
    siteConfig: {
      linuxFxVersion: 'DOCKER|index.docker.io/jyhemark/nicoscrape:latest'
      alwaysOn: true
      ftpsState: 'FtpsOnly'
    }
    serverFarmId: hostingPlan.id
    clientAffinityEnabled: false
    virtualNetworkSubnetId: subnetId
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    vnetRouteAllEnabled: true
  }
  resource name_scm 'basicPublishingCredentialsPolicies@2023-12-01' = {
    name: 'scm'
    properties: {
      allow: false
    }
  }
  resource name_ftp 'basicPublishingCredentialsPolicies@2023-12-01' = {
    name: 'ftp'
    properties: {
      allow: false
    }
  }

  resource name_siteConfig 'config@2023-12-01' = {
    name: 'connectionstrings'
    properties: {
      AZURE_POSTGRESQL_CONNECTIONSTRING: {
        value: 'Database=${postgreSqlDatabaseName};Server=${postgreSqlServerName}.postgres.database.azure.com;User Id=${postgreSqlServerAdminUsername};Password=${postgreSqlServerAdminPwd}'
        type: 'PostgreSQL'
      }
    }
  }
}
