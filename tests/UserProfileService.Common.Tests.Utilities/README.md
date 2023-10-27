# How to generate some mockdata for the UPSv2

We describe here a guideline to generate some mockdata for the UPSv2

## Using the class MockDataGenerator

You can use the mockDataGenerator class to generate simple sample data of types: user, userView, group, groupView,
role,etc..
The generation of the data can be realised as follows:

  ```csharp
  // Create a List with 1000 generated users
   var testUsers = MockDataGenerator.GenerateUsers(1000);
   // Create a List with 500 generated groups
   var testGroups = MockDataGenerator.GenerateGroups(500);
   // Create a List with only one role
   var role = MockDataGenerator.GenerateRoles(); 
       ....      
    }
    
```

This class is used for simple use cases, where you need to generate a big amount of object of a given class without any
relations between the generated objects.

## Using specifical builder classes

If you want to write a test with an model object configured in a fluent API way, you can use a special builder class for
this class. Currently there are: *UserBuilder, FunctionBuilder, GroupBuilder, RoleBuilder, SecObjectBuilder,
UserModifiablesPropertiesBuilder*.
The following is an example of some calls to the UserBuilder:

 ```csharp
  // Create a User with special properties
   var userBuilder = new UserBuilder()
                     .GenerateSampleData()        
                     .WithId("userId")                           
                     .WithLastName("Mustermann")
                     .WithMemberAssignment(groupObject)
                     .Build();
    
```

By calling the method ***GenerateSampleData()*** all default fields of the user are filled with fake data.

 ```csharp
  // Create a User with special properties
   var userBuilder = new UserBuilder()
                     .GenerateSampleData()                           
                     .BuildBasic();
    
```

By calling the method ***BuildBasic()*** an object of the basic type is generated. In this case a user of type ***
UserBasic*** will be generated.
With the Fluent API, you can specifically assign a different value to certain fields.

## Using the SampleDataHelper

With the SampleDataHelper you can generate test data with all possible constellations.
By default, already generated json files with sample data are located in the Resources folder.

### Generate sample data

With the followimng call yan generate some users, groups, functions etc. Some groups are empty, others contain users and
groups. Some users and groups have functions and roles, others do not.

```csharp
  // Generate sample data
  var (groups, users, functions, roles, customProperties, secOs, tags)
                = SampleDataHelper.GenerateSampleData();
   
    
```

You can define the number of each data type that should be generated

 ```csharp
  // Generate sample data
  var (groups, users, functions, roles, customProperties, secOs, tags)
                = SampleDataHelper.GenerateSampleData(amountGroups : 100,
                amountUsers : 500,
                amountFunctions : 100,
                amountRoles : 50,
                amountTags : 50,
                amountSecOs : 100);

    
```

### Generate sample data and overwrite existing stored test data

When generating sample data, you can overwrite the existing test data, that have been already stored in json files .
This can be realised by setting the optional boolean parameter ***writeData*** to true.

 ```csharp
  // Generate sample data
  var (groups, users, functions, roles, customProperties, secOs, tags)
                = SampleDataHelper.GenerateSampleData(writeData:true);
   
    
```

### Load sample data from the stored files

  ```csharp
   
        /// Get all test groups        
        var groups = SampleDataHelper.GetTestGroups();
       
        /// Get all test users             
        var users = SampleDataHelper.GetTestUsers();
       
        /// Get all test roles       
        var roles = SampleDataHelper.GetTestRoles();
        
        /// Get all test custom properties               
        var customProperties = SampleDataHelper.GetTestCustomProperties();   
           
        /// Get all test functions        
        var functions = SampleDataHelper.GetTestFunctions();
       
        /// Get all test tags              
        var tags = SampleDataHelper.GetTestTags();
       
        /// Get all test security objects       
        var secOs = SampleDataHelper.GetTestSecOs(); 
       
   
    
```

All Data are loaded and stored from the directory: *\Resources\Sample[type]s.json*. Example: the generated users are
located in *\Resources\SampleUsers.json*

### Mapping to basic types

It is possible to map the generated objects into the basic types via a mapper.

  ```csharp

  
  var mapper = SampleDataHelter.GetDefaultMapper();
  var users = mapper.Map<UserBasic>(SampleDataHelper.GetTestUsers());
   
    
```