![.NET](https://img.shields.io/badge/-.NET-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/-RabbitMQ-FF6600?style=flat-square&logo=rabbitmq&logoColor=white)

# RabbitMQ Producer + Consumer Simulation

This project aims to show how to send and receive messages reliably using RabbitMQ and .NET.

## Project Overview

In this project, we simulate a production line scenario where sensor data from machines is sent to a central monitoring system to ensure real-time tracking and early fault detection. Communication is handled via a RabbitMQ message queue to prevent data loss even if systems go down or network failures occur. This project uses a basic point-to-point messaging model, where each message is delivered to a single consumer.

## Setup

Prerequisites:
* .NET 6+ SDK
* RabbitMQ Server
* SQL Server with a Message table:
```
CREATE TABLE [Message] (
    id INT IDENTITY PRIMARY KEY,
    machineId NVARCHAR(30),
    sensorType NVARCHAR(50),
    sensorValue NVARCHAR(20),
    transactionId NVARCHAR(50),
    broadcastDate DATETIME,
);
```

* Clone the project:
```
git clone https://github.com/miraygurbuz/RabbitMQMessageProcessor.git
cd RabbitMQMessageProcessor
```

* Restore NuGet packages:
```
cd Send
dotnet restore
dotnet build
```

```
cd ../Receive
dotnet restore
dotnet build
```
## Usage

* Add your connection string to the appsettings.json in ``Receive`` project:
```json
{
  "ConnectionStrings":
  {
    "DefaultConnection": "Data Source=SERVERNAME;Initial Catalog=DATABASENAME;Integrated Security=True"
  }
}
```

* Run the producer:
```
cd Send
dotnet run
```
