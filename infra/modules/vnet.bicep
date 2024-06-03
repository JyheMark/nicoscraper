param location string

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
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' = {
  name: 'subnet-${uniqueString(deployment().name)}-0'
  parent: vnet
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

resource outboundSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' = {
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

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'dnszone-${uniqueString(deployment().name)}.azure.com'
  location: 'global'
  properties: {}
  dependsOn: []
}

output id string = vnet.id
output subnetId string = subnet.id
output outboundSubnetId string = outboundSubnet.id
output privateDnsZoneId string = privateDnsZone.id
