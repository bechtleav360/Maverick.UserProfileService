**This repository is originally from https://github.com/yojimbo87/ArangoDB-NET and changed to support asynchronous
operations and fixed some bugs.**

# ArangoDB-NET

ArangoDB-NET is a C# driver for [ArangoDB](https://www.arangodb.com/) NoSQL multi-model database. Driver implements and
communicates with database backend through its [HTTP API](https://docs.arangodb.com/3.0/HTTP/index.html) interface and
runs on Microsoft .NET and mono framework.

## Usage

### Installation

### Quick Start

The following examples can be read one after the other to form a complete working example.

#### initialize the db client connection

```csharp
// you need a connectionString to initialize the db client
string connectionString = "Endpoints = http://localhost:8529, http://127.0.0.1:8529/; Database=databaseName; UserName=yourUserName; Password=yourPassowrd; UseWebProxy=true; Debug=true; ActiveFailover=false"

using (var db = new ADatabase(connectionString, httpClientFactory))
{
 
      /* some code .... */
}

// without the httpClientFactory
using (var db = new ADatabase(connectionString))
{
 
      /* some code .... */
}
```

#### Create a new database

```csharp
// create a new database 
var newDb = await db.CreateDatabaseAsync("myDatabase");
// create a new database with users
 var users = new List<AUser>()
            {
                new AUser { Username = "fristuser", Password = "secretuser1", Active = true },
                new AUser { Username = "seconduser", Password = "secretuser2", Active = false } 
            };
 var newDb = await db.CreateDatabaseAsync("myDatabase", users);

```

#### Delete a specified database

```csharp
// drop the specified database 
var dropDb = await db.DropDatabaseAsync("myDatabase");

```

#### Retrives informations about currently connected database

```csharp

var response = await db.GetCurrentDatabaseInfoAsync();
var infos = response?.Result;
/* infos contains name, id, path and the boolean value IsSystem */

```

#### Retrieves list of accessible databases which current user can access without specifying a different username or password

```csharp

var response = await db.GetAccessibleDatabasesAsync();
var dbList = response?.Result;
/* dbList a string list of all accessibles databases */

```

#### Create a collection

```csharp
// First way to create a (edge) collection
var collection = await db.Collection.CreateCollectionAsync("myFirstCollection", ACollectionType.Edge);
// Second way
var collection = await db.Collection.CreateCollectionAsync(new CreateCollectionBody { Name = "myFirstCollection", Type = ACollectionType.Edge });
// Third way
var collection = await db.Collection.Type(ACollectionType.Edge).CreateAsync("myFirstCollection");

// you can also create a collection with some other parameters
var collection = await db.Collection.CreateCollectionAsync("myFirstCollection", ACollectionType.Edge, new CreateCollectionOptions{
   JournalSize = 1048576, WaitForSync = true, replicationFactor = 2,...
});
/* some other options (create)collectionOptions
 - journalSize : he maximal size of a journal or datafile in bytes
 - keyOptions (allowUserKeys, type, increment, offset)
 - doCompact: whether or not the collection will be compacted (default is true)
This option is meaningful for the MMFiles storage engine only.
 - shardingStrategy : his attribute specifies the name of the sharding strategy to use for the collection (community-compat,enterprise-compat, enterprise-smart-edge-compat, enterprise-hash-smart-edge)
 - isVolatile : If true then the collection data is kept in-memory only and not made persistent.
 - indexBuckets : he number of buckets into which indexes using a hash
table are split
 - smartJoinAttribute : In an Enterprise Edition cluster, this attribute determines an attribute of the collection that must contain the shard key value of the referred-to smart join collection.

 ...

*/
public class CreateCollectionOptions
    {
        
        /// <summary>
        /// (The default is ""): in an Enterprise Edition cluster, this attribute binds
        /// the specifics of sharding for the newly created collection to follow that of a
        /// specified existing collection
        /// 
        /// </summary>
        public string DistributeShardsLike { get; set; }

        public bool? DoCompact { get; set; }
        /// <summary>
        /// The number of buckets into which indexes using a hash
        /// table are split.The default is 16 and this number has to be a
        /// power of 2 and less than or equal to 1024
        /// </summary>
        public int? IndexBuckets { get; set; }

        /// <summary>
        /// If true, create a system collection. In this case collection-name
        /// should start with an underscore.End users should normally create non-system
        /// collections only
        /// </summary>
        public bool? IsSystem { get; set; }
        /// <summary>
        ///  If true then the collection data is kept in-memory only and not made persistent.
        /// </summary>
        public bool? IsVolatile { get; set; }

        // The maximal size of a journal or datafile in bytes
        public long? JournalSize { get; set; }

        public CollectionKeyOptions KeyOptions { get; set; }
        /// <summary>
        /// (The default is 1): in a cluster, this value determines the
        /// number of shards to create for the collection.In a single
        /// server setup, this option is meaningless
        /// </summary>
        public int? NumberOfShards { get; set; }

        /// <summary>
        /// The default is 1): in a cluster, this attribute determines how many copies
        ///of each shard are kept on different DBServers.The value 1 means that only one
        ///copy (no synchronous replication) is kept.A value of k means that k-1 replicas
        /// are kept.Any two copies reside on different DBServers
        /// </summary>
        public int? ReplicationFactor { get; set; }
        /// <summary>
        ///  (The default is [ "_key" ]): in a cluster, this attribute determines
        /// which document attributes are used to determine the target shard for documents
        /// </summary>
        public string ShardKeys { get; set; }

        public string ShardingStrategy { get; set; }
        /// <summary>
        /// In an Enterprise Edition cluster, this attribute determines an attribute
        /// of the collection that must contain the shard key value of the referred-to
        /// smart join collection
        /// </summary>
        public string SmartJoinAttribute { get; set; }
        /// <summary>
        /// Write concern for this collection (default: 1). It determines how many copies of
        /// each shard are required to be in sync on the different DBServers
        /// </summary>
        public int? WriteConcern { get; set; }
        /// <summary>
        /// If true then the data is synchronized to disk before returning from a
        /// document create, update, replace or removal operation. (default: false)
        /// </summary>
        public bool? WaitForSync { get; set; }
    }

```

#### Retrieves informations about specified collection

```csharp
// get basic informations about a collection
var collection = await db.Collection.GetCollectionAsync("myFirstCollection");
// the field Result contains name, Id, status , Type, IsSystem and GloballyUniqueId

// get basic infos and additional properties about a collection
var collection = await db.Collection.GetCollectionPropertiesAsync("myFirstCollection");
// the field Result contains basis infos and journalSize, IsVolatile, waitForSync and (optional) Cluster Attribute


```

#### Create a document

```csharp
// to create a document
var document = await db.Document.CreateDocumentAsync<Person>("myFirstCollection", new Person {Name = "testName", Alter = 33, IsSingle = true});

// to create document with options
var options = new CreateDocumentOptions { ReturnNew = true, Overwrite = false, Silent = false, WaitForSync = true };

var document = await db.Document.CreateDocumentAsync<Person>("myFirstCollection", new Person {Name = "testName", Alter = 33, IsSingle = true}, options);

/* some other options that can be setted by creating a document
- WaitForSync : Wait until document has been synced to disk
- ReturnOld : Additionally return the complete old document under the attribute old
- Silent : If set to true, an empty object will be returned as response
- Overwrite : If set to true, the insert becomes a replace-insert
- ReturnNew :  Additionally return the complete new document under the attribute new in the result.
*/

```

#### Update a document

```csharp
// create a document
var document = await db.Document.CreateDocumentAsync<Person>("myFirstCollection", new Person {Name = "testName", Alter = 33, IsSingle = true});

var docId = document?.Result?._id;
var newPerson = new Person{Name = "tester", Alter = 39, IsSingle = false};

//update the document
var newDoc = await db.Document.UpdateDocumentAsync<Person>(docId,newPerson);



/* some other options that can be setted by updating a document
- all document options
- KeepNull : f the intention is to delete existing attributes with the patch command, the URL query parameter keepNull can be used with a value of false.
- MergeObjects: Controls whether objects (not arrays) will be merged if present in both the existing and the patch document. If set to false, the value in the patch document will overwrite the existing document's value. If set to true, objects will be merged. The default is true.
- IgnoreRevs : By default, or if this is set to true, the _rev attributes in the given documents are ignored. If this is set to false, then any _rev attribute given in a body document is taken as a precondition. The document is only updated if the current revision is the one specified.
- ReturnNew : Return additionally the complete new documents under the attribute "new" in the result

*/

```

## Transactions

#### Execute a JSTransaction

```csharp

string action = "function () {var db = require('@arangodb').db; db.products.save({})); return db.products.count(); }";
var writeCollections = new List<string>{"products"};
var transaction = await db.Transaction.ExecuteJSTransactionAsync(action,writeCollections);

/*some other options that can be setted by executing a transaction
- MaxtransactionSize : The maximum transaction size before making intermediate commits (RocksDB only)
- LockTimeout : The maximum time to wait for required locks to be released, before the transaction times out.
- WaitForSync : an optional boolean flag that, if set, will force the transaction to write all data to disk before returning.
*/




```

#### Execute a StreamTransaction

```csharp
// begin the transaction to get the transaction id
List<string> writeCollections = new List<string>(){"TestCollection"};
List<string> readCollections = new List<string>(){"TestCollection-2"};
var transaction = await db.Transaction.BeginTransactionAsync(writeCollections, readCollections);
var transactionId = transaction?.Result?.Id;

// do some Operation on the collection
var operation = await db.Collection.DeleteAsync("TestCollection",transactionId);

var doc = db.Document.SetTransactionId(transactionId).CreateDocumentAsync<Person>("TestCollection", new Person {Name = "Max", Alter = 2, IsSingle = false});

 /* some operations with the collections .... */


// commit the transaction

var commit = await db.Transaction.CommitTransactionAsync(transactionId);


```

## AQL Query

#### Execute an AQL Query

```csharp
// with the query as string parameter
var response = await db.AQuery.ExecuteQueryAsync<User>("FOR user IN users RETURN user");
IList<User> userList = response?.QueryResult;



// with more options

string query = $"FOR user IN users FILTER user.active == 1 RETURN user";
// first way
var response = await db.AQuery.ExcecuteQueryWithOptionsAsync<User>(new CreateCursorBody{ Query = query. BatchSize = 500, Count = true});
// list of results
IList<User> userList = response?.QueryResult;

// second way
var response = await db.AQuery.Count(true).BatchSize(500).ExecuteQueryAsync<User>(query);
// list of results
IList<User> userList = response?.QueryResult;

```

#### Create a cursor which can be used to page query results

```csharp
string query = "FOR x IN actsIn COLLECT actor = x._from WITH COUNT INTO counter FILTER counter >= 3 RETURN { actor: actor, movies: counter }";
// with the query as string parameter
// this request will create a cursor with the given options and fetch the 20 first result of the query
var firstResponse = await db.AQuery.CreateCursorAsync<ActsIn>(new CreateCursorBody{Query = query, Count = true, BatchSize = 20, Cache = true, Options = new  PostCursorOptions {FullCount = true } });

// the id of the created cursor
string cursorId = firstResponse?.Id;
// the first 20 results ( can be less than 20 if the query results has totally less than 20 elements)
IEnumerable<ActsIn> firstResultList = firstResponse?.Result;
bool HasNext = firstResulList?.HasMore ?? false;
// fetch the next 20 elements...
if(HasNext)
{
   var secondResponse = await db.AQuery.PutCursorAsync(cursorId);
   IEnumerable<ActsIn> secondResultList =  secondResponse?.Result;

}



// with more options

string query = $"FOR user IN users FILTER user.active == 1 RETURN user";
  
var response = await db.AQuery.ExcecuteQueryWithOptionsAsync<User>(new CreateCursorBody{ Query = query. BatchSize = 500, Count = true});

// list of results
IList<User> userList = response?.QueryResult;



```

## Return Type for The API

### BaseApiResponse

```csharp

public abstract class BaseApiresponse
{
    //  Arango exception
   public ApiErrorException ApiException
    // Headers of the response
   public HttpContentHeaders ResponseHeaders
    // Statuscode of the response
   public HttpStatusCode Code
    // is true if an error happened
   public bool Error
    // Object containing debug informations (executiontime, request as string ...)
   public DebugInfo DebugInfos

}

public class DebugInfo
{
   /// <summary>
   /// Contains the uri string of the request.
   /// </summary>
   public string RequestUri { get; set; }
   /// <summary>
   /// Contains the body of the request
   /// </summary>
   public string RequestJsonBody { get; set; }
   /// <summary>
   /// Contains information about the used http method.
   /// </summary>
   public string RequestHttpMethod { get; set; }

   public long ExecutionTime { get; set; }


}

```

### SingleApiResponse

This is the standard response type of the public methods. It contains:

```csharp

public class SingleApiResponse<T> : SingleApiResponse
{
   //  Arango exception
   public ApiErrorException ApiException
   // original response object
   public T Result
   // Headers of the response
   public HttpContentHeaders ResponseHeaders
   // Statuscode of the response
   public HttpStatusCode Code
   // is true if an error happened
   public bool Error
   // Object containing debug informations (executiontime, request as string ...)
   public DebugInfo DebugInfos

}

public class SingleApiResponse : BaseApiResponse{ }



```

### MultiApiResponse

This is the standard response type for AQL Queries

```csharp
public sealed class MultiApiResponse<T>
{
   // the list of the Api Responses for each call which has been done to execute the Query
   public IEnumerable<BaseApiResponse> Responses
   // results of the query
   public IReadOnlyList<T> QueryResult
   // Query execution time
   public long ExecutionTime { get; }

}


```

## .NET Core

To use this client with an .NET Core App you have to register it as follow:

### Add a connection String in the AppSettings.json

 ```csharp
 {
 "ConnectionStrings": {
    "DefaultConnection": "Endpoints = http://localhost:8529; Database=mydatabase; UserName=myUserName; Password=myPassowrd; UseWebProxy=true; Debug=true;",
    "MyConn": "Endpoints = http://localhost:8529, http://127.0.0.1:8529/; Database=databaseName; UserName=yourUserName; Password=yourPassowrd; UseWebProxy=true; Debug=true;"

 }
}
```

### Register the App in the Startup class

 ```csharp
  public IConfiguration Configuration {get;}

  public void Configure(IServiceCollection services)
  {
     // using the default Connection String (field DefaultConnection)
     services.AddArangoDBClient(Configuration)
             .AddScoped<,>()
             .Add.... ;
   
     // using the other defined Connection String
     services.AddArangoDBClient(Configuration, "myConn")
             .AddScoped<,>()
             .Add .... ;
  }
        
            
 ```
