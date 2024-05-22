# Volatile Data

You can store data in a non-standard manner where it won't be persisted in the event store but will only be persisted in the PostgreSQL database. This type of data is referred to as volatile data, such as controller settings used in user settings. This data is stored only for the user. To store such data, you need a user settings section, a user Id and a JSON object. For example, you could store favorite links from YouTube or the user's preferred topics for Udemy videos. 

## Filtering the Data
If you are looking for specific user settings, you can filter and sort the results accordingly. The results are returned in a paginated format.

### Usage of the $filter query
The **$filter** query is used to filter of result-set through restrictions. It can be seen as a where-Clause  in the sense of SQL. The filter can only be applied to properties of the result item. Nested properties are not supported yet. Filter expressions can be combined with an `OR` or an `AND`. In the example below we have a simple data set that represents a user that has only five properties. The filter can be applied of all of these properties.

```json
{
    "Name": "Sam, Smith",
    "FirstName": "Sam",
    "LastName": "Smith",
    "CreatedAt": "2022-09-11T09:30:11.6796611+02:00",
    "Id": 235
}
```
Let's say we want all user that have the same last name. Several users with the same name can be stored in the database. The query would look like:

 `LastName eq 'Smith'`

The first item in the query is the property "LastName". As second item we have an operator (valid operators see below).The third item is the value with which you want to compare. The value MUST be quoted if it is a type of string or date (in out case all properties except `Id`). As quotation only single-quotation can be used. Valid quotes are ' and ′. Number must not be quoted.
Valid operator are:
 
 - **eq**: EqualsOperator
 - **ne**: NotEqualsOperator
 - **gt**: GreaterThenOperator
 - **ge**: GreaterEqualsOperator
 - **lt**: LessThenOperator 
 - **le**: LessEqualsOperator 
 - **ct**: ContainsOperator
   
 The operators can be applied nearly on every property. But for string only `eq`, `ne` and `ct` make sense. 

 **Other Examples**

 `LastName eq 'Smith' AND FirstName eq 'Sam'`

 The user with the last name Smith and first name Sam should be filtered. `Id eq 235` The user with the Id 235 should be retrieved.
 
 `CreatedAt lt '2023-09-11T09:30:11.6796611+02:00' AND Id ne 235 `

 Return all user that were created below the data **'2023-09-11T09:30:11.6796611+02:00'** and **Sam, Smith** should
 not be a part of  the result set.

 **PLEASE NOTICE**:
 The explanation for the **$filter** was in general. Every endpoint has his own **RESULT**. So please look at the Open-Api description which properties are available.
 
### Usage of the $oderBy clause

The **$orderBy** filter lets your order the result set by one or more properties. The properties must be again in the result set. As our result set we will take again our simple example with the user. To order the user by the first name we can using the query:

`FirstName`
 
This will order the user by its first name in an **ascending** order. Please notice that the default-value for the sorting order is **ascending**. If you want to order the result set in a descending order you can use the query: 

`FirstName desc`
 
If you forget the default sorting value you can still use the abbreviation asc in the query:
 
`FirstName asc`

It is also possible to sort by two properties. Again we using our simple user model to sort the results by first name in a ascending order and created at in descending order. Notice when using more than two properties you have to separate them with a comma `,`.
 
`FirstName, CreatedAt desc`

The **$filter** query and the **$orderby** query can be combined.