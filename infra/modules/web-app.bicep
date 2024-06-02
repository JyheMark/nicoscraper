param name string
param location string
param postgreSqlServerName string = 'server-${uniqueString(deployment().name)}'
param postgreSqlServerAdminUsername string
param postgreSqlDatabaseName string = 'db-${uniqueString(deployment().name)}'

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

resource vnet 'Microsoft.Network/virtualNetworks@2020-07-01' = {
  location: location
  name: 'vnet-${uniqueString(deployment().name)}'
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: []
  }
  resource vnetSubnet 'subnets@2023-11-01' = {
    name: 'subnet-${uniqueString(deployment().name)}-0'
    properties: {
      delegations: [
        {
          name: 'delegation'
          properties: {
            serviceName: 'Microsoft.Web/serverfarms'
          }
        }
      ]
      serviceEndpoints: [
        {
          service: 'Microsoft.Storage'
        }
      ]
      addressPrefix: '10.0.1.0/24'
    }
  }
}

resource vnetName_outboundSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' = {
  name: 'subnet-${uniqueString(deployment().name)}-1'
  parent: vnet
  properties: {
    delegations: [
      {
        name: 'dlg-database'
        properties: {
          serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
        }
      }
    ]
    serviceEndpoints: []
    addressPrefix: '10.0.2.0/24'
  }
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
    virtualNetworkSubnetId: vnet.id
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
    dependsOn: [
      postgreSqlServer
    ]
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'dnszone-${uniqueString(deployment().name)}'
  location: 'global'
  properties: {}
  dependsOn: []
}

resource postgreSqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
  name: 'nicoscrape-app-server'
  location: location
  tags: {
    'managed-by': 'Bicep'
  }
  properties: {
    administratorLogin: postgreSqlServerAdminUsername
    administratorLoginPassword: postgreSqlServerAdminPwd
    version: '12'
    network: {
      delegatedSubnetResourceId: vnetName_outboundSubnet.id
      privateDnsZoneArmResourceId: privateDnsZone.id
    }
    availabilityZone: ''
    highAvailability: {
      mode: 'Disabled'
    }
    storage: {
      storageSizeGB: 32
    }
  }
  sku: {
    name: 'Standard_D2s_v3'
    tier: 'GeneralPurpose'
  }
  resource postgreSqlServerName_postgreSqlDatabase 'databases@2022-12-01' = {
    name: 'nicoscrape-app-database'
    properties: {
      charset: 'utf8'
      collation: 'en_US.utf8'
    }
  }
}
