targetScope = 'subscription'
param name string
param location string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: name
  location: location
  tags: {
    'managed-by': 'Bicep'
  }
}

output id string = resourceGroup.id
output name string = resourceGroup.name
output location string = resourceGroup.location
