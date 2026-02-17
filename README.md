# CampusCore

## Description

This repo contains the server architecture and business logic of CampusCore App made for Software Engineering laboratory project.

### 1. Use-case Diagram

The actors and the use cases are sown in the diagram below:
<img width="1862" height="2246" alt="use_case_proiect drawio (8)" src="https://github.com/user-attachments/assets/c8929b04-d5cd-4327-b1b1-7b734667f9ae" />


## Installation

1. Install and run [**Docker Desktop**](https://www.docker.com/products/docker-desktop/).
2. Run ```Start-CampusCore.bat```.

## Development & Debugging

### Prerequirements

1. Ensure that you have [**Visual Studio 2022**](https://visualstudio.microsoft.com/vs/) with **ASP.NET and web Development** and **Container development tools**.
2. Instal [**Docker Desktop**](https://www.docker.com/products/docker-desktop/)
3. Clone this repo using:
   ```
   git clone https://github.com/IS-Project-CampusCore/campus-server.git
   ```

### Run the Server

#### 1. Run each service manually

1. Open the project solution with Visual Studio.
2. Select the service you want to run form **Startup Item Menu** and run it with **http**.
3. A console will open and will contain all logs.

#### 2. Run entire server with Docker

1. Start **Docker Desktop**. Ensure you have a working network connection.
2. Open the project solution with Visual Studio.
3. Form **Startup Item Menu** select **docker-compose** and run. Wait for the docker image to be build.
4. Serilog console will open in Visual Studio inside **Containers Window** in *Logs section*.

#### 3. Run entire server from Console

1. Start **Docker Desktop**. Ensure you have a working network connection.
2. Run docker-compose inside Cmd or PowerShell with:
   ```
   docker compose up --build
   ```
3. In case of any errors or build fails, take the docker image down using:
   ```
   docker compose down
   ```

### Debugging with Visual Studio

Like any other project you can debug it inside Visaul Studio GUI.

### Create new Services

Services are added automatically using a simple template that respects the overall project architecture. See **Architecture** section inside this file.

#### 1. Install Service Template

* Inside the root directory you can find ```_templates``` subdirectory. If this is the first time you add a new service, be sure you run this command first:
   ```
   dotnet new install ./_templates/MyGrpcService
   ```
* If the template format was updated, you need to uninstall the old tamplate and install the new one, use those commands:
   ```
   dotnet new install ./_templates/MyGrpcService
   dotnet new uninstall ./_templates/MyGrpcService
   ```
   **Or**
   ```
   dotnet new install ./_templates/MyGrpcService --force
   ```

Now the template has the latest version and can used to create a new service

#### 2. Add new Services

To add a new service to this project you need to run this command inside PowerShell from the root directory:
```
./add-service.ps1 -ServiceName YourNewServiceName
```

This script automates the service creation process and integration by doing those operations:
1. Creates a new service with the name argument.
2. Automatically adds the new service inside the ```.sln``` file.
3. Updates the ```docker-compose.yml``` file with the new service section.
4. For the service to be usefull you would need to do some manual steps(*this steps are explained in console after the script complets*).

After the script runned succesfully you have your new service inside the project and can be used for implementation.

## Server Architecture

This service uses a Microservices architecture structured as an API Gateway(http service) and API(the rest of services).

### 1. API Gateway Structure

The **http service** aggregrates all the HTTP enpoints and expose them to the client app.
It uses *HTTP1.1 Requests* for Client-to-Server communication and *HTTP2** for Service-to-Service communication.

#### 1. Client-to-Server communication

* As explained above, this communication uses **HTTP1.1** for *REST API* requests.
* This project uses **FastEndpoints NuGet Packedge** for the endpoints aggregation.
* **Endpoint abstraction classes**: ```CampusEndpointBase``` & ```CampusEndpoint``` used to implement common parts of REST API Requests and Responses using FastEndpoint NuGet Packedge.
* It has a **JWT Bearer Token** schema for secure requests authentication.

* For debuging and testing the API Gateway you can use any API tasting tool like *Postman* or *Insomnia*.

#### 2. Service-to-Service communication

* The API Gateway does not expose any **gRPC Servers**, it is designed to be a **gRPC Client** for every service that has an endpoint implemented.
* For more information about the actual communication design and implementation go to [**gRPC Communication Design**](#3-grpc-communication-design) section.
* Some services also use an event system using RabbitMQ, more informations about this design can be found in [**Event System**](#event-system-communication-design) section.

#### 3. Server-to-Client communication

* For *real time* features, like the Chat feature, SignalR event system is used, making the communications and synchronization instant between users.
* More informations about this design and implementation can be found in [**SignalR System**]() section.

### 2. API Structure

The API is composed from multiple **Microservices** with a **gRPC Communication** design.
It uses *HTTP2* for Service-to-Service communication exposing a **gRPC Server** for each service and uses **gRPC Clients** to call other services.

#### 3. gRPC Communication Design

This design uses an atypical gRPC communication design with a *common response message*. The main features of this communication architecure are:
 * **Message abstraction classes**: ```CampusEndpointBase``` & ```CampusMessage<TReq, TRes>``` or ```CampusMessage<TReq>``` used to implement common parts of the gRPC Requests and Responses by implementing ```IRequestHandler<TReq, MessageResponse>``` interface for MediatR.
 * **Service middle ware class**: ```SerivceInterceptor``` inherits ```Interceptor``` class from Grpc.Core NuGet Packedge and is used to intercept any request, procces the response and handle any errors.
 * **MediatR pattern**: each service uses a mediator to send request, using this pattern every massage cand be implemented individually and be separated in levels of concern, this is also a dev friendly implementation that makes the code cleaner. This pattern comes with suplimentary logic to work, the request class needs to implement ```IRequestBase``` and the message class needs to inherit from ```CampusMessage<TReq, MessageResponse>```.

##### 1. Common Messages Response

For each service message the response is common and it is defined in ```MessageResponse```. The reasons behind this implementation are:
* **Better message agregation**: Because the services needs to communicate with eachother and aggregate their responses to other services and the API Gateway this design makes the communication easier.
* **Better error handling**: Because all the messages returns the same response this simplify the error handling process and error response just by using ```ServiceMessageException.cs```.

This common respons is defined in ```common.proto``` and ```MessageResponse.cs``` and is structured as below:
```
message MessageResponse {
	bool success = 1;
	int32 code = 2;
	optional string body = 3;
	optional string errors = 4;
}
```
This message has a direct translation to a **C# class** named MessageResponse.cs that contains some crucial static methods for the communication:
```
public static MessageResponse Ok(object? response = null);
public static MessageResponse BadRequest(string errorMessage, object? response = null);
public static MessageResponse Unauthorized(string errorMessage, object? response = null);
public static MessageResponse Forbidden(string errorMessage, object? response = null);
public static MessageResponse NotFound(string errorMessage, object? response = null);
public static MessageResponse Error(string errorMessage, object? response = null);
public static MessageResponse Error(Exception ex);
```
* Each of the above methods returns a MessageResponse object that has:
1. **Success flag**: if the message executed successfully and the Status Code is Ok(200).
2. **Code**: the returned Status Code of the message.
3. **Body**: a MessageBody property that has the response body if there is one.
4. **Errors**: if the message has returned with an error.

* The MessageResponse body is a MessageBody property that works like a wrapper for JsonElement and implements methods for getting the data out in different types.

##### 2. Communication path

The communication path is show in the below diagram:

<img width="861" height="561" alt="CommunicationDiagram drawio (1)" src="https://github.com/user-attachments/assets/a38e172a-1e27-4329-8ead-cc170f1757dc" />

* This communication represent the data path for between API Gataway and API, it also represents a simple two service communication and the ServiceInterceptor role as a middle ware system.

#### 4. Event System Communication Design

The event system provides a robust, asynchronous messaging infrastructure built on top of MassTransit and RabbitMQ. It utilizes a decoupled "Envelope" pattern to facilitate both fire-and-forget publishing and request-response patterns across microservices.

* **Envelope Pattern**: Instead of sending raw objects, all messages are wrapped in an ```Envelope.cs```. This contains the EventType (routing key) and a Payload (JSON-serialized body). This abstraction allows the system to route messages dynamically without the transport layer needing deep knowledge of the underlying C# types.

* **Message Abstraction**:
	* **CampusConsumerBase<TResponse>**: The foundation for all consumers. It handles logging scopes (CorrelationId, RequestId), error handling, and automated response generation.
	* **CampusConsumer<TResponse>**: Used for consumers that return a specific data type.
	* **CampusConsumer**: A specialized version for "void" operations that returns an EmptyResponse.
	* **Scoped Publisher**: The ScopedMessagePublisher ensures that messages are sent within a valid Dependency Injection scope. It supports:
		* *Publish*: Standard async message distribution.
  		* *SendAsync*: A request-response implementation that waits for a MessageResponse from the consumer.

##### 1. Direct Routing & Topology

The system uses a Direct Exchange strategy to ensure messages reach the correct specialized queues based on their intent:
* **Attribute-Based Mapping**: Consumers are decorated with the ```[EnvelopeAttribute("EventName")]```.
* **Automatic Registration**: The MassTransitExtension scans assemblies for these attributes and automatically configures RabbitMQ endpoints.
* **Routing Keys**: The EventType in the Envelope serves as the routing key, ensuring the Direct Exchange delivers the message only to the bound queues.

##### 2. Error Handling & Status Codes

Similar to the gRPC implementation, the Event System uses specialized exceptions to maintain consistency across the architecture:
* **Event-Specific Exceptions**: ```EventException.cs``` provides a suite of specialized exceptions (e.g., EventNotFoundException, EventValidationException).
* **Automatic Response Mapping**:
	* If a consumer is invoked via a Request/Response pattern (context.RequestId.HasValue), the base consumer catches these exceptions and converts them into a failed MessageResponse with the appropriate status code.
	* If it is a Fire-and-Forget message, the exception is re-thrown to trigger standard MassTransit retry policies.

##### 3. Communication path

The communication path is show in the below diagram:

<img width="861" height="381" alt="EventSystem (8)" src="https://github.com/user-attachments/assets/34e54fbc-2ccb-45b3-8400-7a6fc4ff843a" />

#### 5. SignalR Communication Design
The SignalR system provides real-time, bi-directional communication between the server and clients, integrated seamlessly with the event-driven architecture. It leverages a mapping system to track active user connections and specialized consumers to broadcast events to the UI.

* **CampusHub<THub>**: A base Hub class that manages the connection lifecycle. It automatically extracts user identity from JWT claims and registers the active ConnectionId in the mapping system upon connection.
* **Connection Tracking**: The IConnectionMapping<THub> and its implementation ConnectionMapping<THub> use a ConcurrentDictionary to keep track of multiple active connections per user. This allows the system to support a single user being logged in across multiple devices or browser tabs.
* **The Notifier Pattern**: The INotifier<THub> interface abstracts the SignalR HubContext, providing high-level methods to send messages to specific users or groups without needing to manage raw connection IDs manually.

##### 1. Event-to-SignalR Integration
A key feature of this design is the ability to bridge backend events (MassTransit) directly to the frontend via SignalR:

* **SignalRConsumer**: This is a specialized version of the CampusConsumer that has access to the INotifier and IConnectionMapping.
* **Flow**: When a backend event is received, a SignalRConsumer can check if the target user is online using IsUserOnline(userId) and then push a real-time notification using the _notifier.
* **ISignalRDefinition**: Used to standardize the structure of messages sent over the socket, ensuring that the Message name and Content payload remain consistent across different services.
