param location string
param postgreSqlServerAdminUsername string
param outboundSubnetId string
param privateDnsZoneId string
param name string

@secure()
param postgreSqlServerAdminPwd string

resource postgreSqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: name
  location: location
  tags: {
    'managed-by': 'Bicep'
  }
  properties: {
    administratorLogin: postgreSqlServerAdminUsername
    administratorLoginPassword: postgreSqlServerAdminPwd
    version: '12'
    network: {
      publicNetworkAccess: 'Disabled'
      delegatedSubnetResourceId: outboundSubnetId
      privateDnsZoneArmResourceId: privateDnsZoneId
    }
    availabilityZone: ''
    highAvailability: {
      mode: 'Disabled'
    }
    storage: {
      storageSizeGB: 32
    }
    backup:{
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
  }
  sku: {
    name: 'Standard_D2s_v3'
    tier: 'GeneralPurpose'
  }
  resource postgreSqlServerName_postgreSqlDatabase 'databases@2023-12-01-preview' = {
    name: 'nicoscrape-app-database'
    properties: {
      charset: 'utf8'
      collation: 'en_US.utf8'
    }
  }
}
