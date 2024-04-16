# Datastructure
The UPS data is currently stored in [ArangoDB](http://arangodb.com), a graph database that supports graph queries. Data is stored as JSON strings, and ArangoDB offers two types of data structures for storage: Collections and EdgeCollections. Collections store JSON documents. Each document can have varying fields and structures. EdgeCollections are a specialized data store in ArangoDB used for storing relationships between documents in a graph. They enable the definition of directed or undirected edges between nodes.

## Used Collections / EdgeCollections
The service utilizes all collections with the suffix Service_. All data accessible via the API is stored in these collections. Further details about these collections will be provided in the documentation.

### Service_clientSettingsQuery
### Service_customPropertiesQuery
### Service_pathTree
### Service_pathTreeEdges
### Service_profilesQuery
### Service_projectionState
### Service_rolesFunctionsQuery
### Service_tagsQuery
### Service_tickets
### Service_v1_profilesQuery


  
