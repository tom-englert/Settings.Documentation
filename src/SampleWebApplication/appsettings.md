# Configuration Settings
## MyOptions
### Port
  - type: number
  - default: 99
  - description: The port the service is running on
### Host
  - type: string
  - default: localhost
  - description: The host ulr running the service
## ConnectionStrings
### Database
  - type: string
  - default: Server=.;Database=MyDb;Trusted_Connection=True;
  - description: The database connection string
### Password
  - type: string
  - default: *****
  - description: The database users password
### MessageQueue
  - type: string
  - default: Server=.;Queue=MyQueue
  - description: The message queue connection string
