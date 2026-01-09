# Configuration Settings
## MyOptions
### Port
  - type: number
  - default: 99
  - description: The port the service is running on
### Host
  - type: string
  - default: "localhost"
  - description: The host ulr running the service
### SupportedCultures
  - type: array
  - default: [
  "en-US",
  "de-DE"
]
  - description: The supported cultures
### EnableVerboseLogging
  - type: boolean
  - default: false
  - description: Enable verbose logging
### Timeout
  - type: string
  - default: "00:00:30"
  - description: The timeout for service calls
### Optional
  - type: string
  - default: 
  - description: Database connection strings
### Required
  - type: string
  - default: 
  - description: A required setting without default value
## ConnectionStrings
### Database
  - type: string
  - default: "Server=.;Database=MyDb;Trusted_Connection=True;"
  - description: The database connection string
### Password
  - type: string
  - default: "*****"
  - description: The database users password
### MessageQueue
  - type: string
  - default: "Server=.;Queue=MyQueue"
  - description: The message queue connection string
