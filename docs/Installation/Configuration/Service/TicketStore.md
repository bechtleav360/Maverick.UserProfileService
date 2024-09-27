# Configure Ticketstore

Here is a sample section on how to configure the ticket store:

??? abstract "Ticketstore example configuration"
    ```json
    {
      "TicketStore": {
        "Backend": "arangodb"
      }
    }
    ```
`Backend` - The Backend setting should be configured to match the database you are using for storing tickets. The possible values are `arangodb`, `sqlServer`, `postgres`, and `sqLite`