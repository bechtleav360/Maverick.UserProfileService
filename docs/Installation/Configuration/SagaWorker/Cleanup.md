# Configure Cleanup Service
The Cleanup configuration section outlines the frequency at which data cleanup operations are performed for various components. Here is a brief example of how to configure the cleanup:

```json
{
  "Cleanup": {
    "AssignmentProjection": "05:00:00",
    "EventCollector": "05:00:00",
    "Facade": "05:00:00",
    "FirstLevelProjection": "05:00:00",
    "Service": "05:00:00"
  }
}
```

`AssignmentProjection`- Specifies the interval at which cleanup operations are performed for the Assignment Projection component.

`EventCollector` - Specifies the interval at which cleanup operations are performed for the Event Collector component.  

`Facade` - Specifies the interval at which cleanup operations are performed for the Facade component. 

`FirstLevelProjection` - Specifies the interval at which cleanup operations are performed for the First Level Projection component. 

`Service` - Specifies the interval at which cleanup operations are performed for the Service component.

**Note**: The value is represented in hours, minutes, and seconds format (HH:MM:SS). In our sample all components will be cleaned every 5 hours.