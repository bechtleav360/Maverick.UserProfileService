# Range Conditions

You can assign entities to each other in the user profile service. Each assignment has a `condition` that determines when the assignment is valid. A `condition` has two properties: `start` and `end`. Assignments can have multiple conditions. Conditions can be used to create assignments either in the present or in the future. For example, if a user needs a special privilege in two months within a specific group, you can create a condition for that time period. Alternatively, the user might only need the privilege for a certain duration. 

The keyword `null` holds a specific significance for the two properties in a condition. It indicates that the time range is either unlimited or irrelevant. If you set both properties to `null`, it signifies that the assignment is valid from its inception and has no end date. When you set the start as `null` and specify a future date, it indicates that the condition is valid from the present until the specified end date. Alternatively, you can set the `start` to a specific future date and leave the `end` as `null`, signifying that the assignment will commence at that future date and remain valid indefinitely.

In the system, it's not possible to create conditions in the past, and they must not overlap temporally, as the validation process prevents this.


### Examples

```json
{
  "conditions": [
    {
      "end": null,
      "start": null
    }
  ]
}
```

The assignment starts immediately and lasts indefinitely.

```json
{

       "conditions": [
        {
          "end": "2099-05-02T12:55:19.830Z",
          "start": null
        }
       ]
}
```
The assignment starts immediately and continues until May 2099.

```json
{
       "conditions": [
        {
          "end": null
          "start": "2099-05-02T12:55:19.830Z"
          
        }
       ]
}
```
The assignment begins in May 2099 and is permanent.

```json
{
       "conditions": [
        {
          "end": "2102-05-02T12:55:19.830Z"
          "start": "2099-05-02T12:55:19.830Z"
          
        }
       ]
}
```
The assignment begins in May 2099 and ends in May 2102.

```json
{
  "conditions": [
    {
      "start": "2024-06-01",
      "end": "2024-06-30"
    },
    {
      "start": "2024-06-15",
      "end": "2024-07-15"
    }
  ]
}
```
In this example, the two conditions overlap. The first condition is valid from June 1, 2024, to June 30, 2024, while the second condition is valid from June 15, 2024, to July 15, 2024, overlapping with the first one. The validation will fail.


```json
{
  "conditions": [
    {
      "start": "2024-08-01",
      "end": "2024-08-31"
    },
    {
      "start": "2024-09-01",
      "end": "2024-09-30"
    },
    {
      "start": "2024-10-01",
      "end": "2024-10-31"
    }
  ]
}
```

In this example, the conditions are clearly separated and all lie in the future without overlapping. The first condition is valid from August 1, 2024, to August 31, 2024, the second condition from September 1, 2024, to September 30, 2024, and the third condition from October 1, 2024, to October 31, 2024.
