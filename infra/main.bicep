// ********************************************************************************************** 
// 
//    Type: Main 
//    Author: Lukas Rottach 
//    Version: 0.1 
//    Provider: --- 
//    Description: Deployment of all required resources for operating the Azure Reaper solution 
//    Reference: https://github.com/lrottach/az-reaper
// 
// ********************************************************************************************** 

// Deployment Scope
// ********************************
targetScope = 'subscription'

// Parameters
// ********************************

@allowed(['West Europe', 'Switzerland North', 'East US'])
@minLength(1)
@description('Primary location for all resources')
param deploymentLocation string

@minLength(1)
@maxLength(64)
@description('Resource group name to contain all resources')
param resourceGroupName string

@minLength(1)
@maxLength(64)
@description('Name of the Azure Function App')
param azureFunctionName string

@description('Name of the Azure Storage Account, required to operate Azure Reaper')
param storageAccountName string

// Resources
// ********************************

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2020-06-01' = {
  name: resourceGroupName
  location: deploymentLocation
}
