# First Steps

- If Service or Saga-Worker is not running, check for logs and configuration
- For detailed information you can increase the log level to Debug or Trace
- Check for right credentials by the third-party components if logs showing messages like "not authorized - 401"
- Check Health-Endpoints and identify maybe broken third-party componentens (state endpoints)
  - Check if the database exists (ArangDb, Postres)
  - Check for credentials
- Check the logs for further information
- Check for messagaes in queues (error)