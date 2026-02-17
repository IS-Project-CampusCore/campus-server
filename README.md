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
* It has a **JWT Bearer Token** schema for secure requests authentication.

* For debuging and testing the API Gateway you can use any API tasting tool like *Postman* or *Insomnia*.

#### 2. Service-to-Service communication

* The API Gateway does not expose any **gRPC Servers**, it is designed to be a **gRPC Client** for every service that has an endpoint implemented.
* For more information about the actual communication design and implementation go to [**gRPC Communication Design**](#3-grpc-communication-design).

### 2. API Structure

The API is composed from multiple **Microservices** with a **gRPC Communication** design.
It uses *HTTP2* for Service-to-Service communication exposing a **gRPC Server** for each service and uses **gRPC Clients** to call other services.

#### 3. gRPC Communication Design

This design uses an atypical gRPC communication design with a *common response message*. The main features of this communication architecure are:
 * **Endpoint abstraction classes**: ```CampusEndpointBase``` & ```CampusEndpoint``` used to implement common parts of REST API Requests and and Responses using FastEndpoint NuGet Packedge.
 * **Service middle ware class**: ```SerivceInterceptor``` inherits ```Interceptor``` class from Grpc.Core NuGet Packedge and is used to intercept any request, procces the response and handle any errors.
 * **MediatR pattern**: each service uses a mediator to send request, using this pattern every massage cand be implemented individually and be separated in levels of concern, this is also a dev friendly implementation that makes the code cleaner. This pattern comes with suplimentary logic to work, the request class needs to implement IRequest<MessageResponse> and the message class needs to implement IRequestHandler<TReq, MessageResponse>.

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

