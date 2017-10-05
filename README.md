# ObjectTransport
A lightweight library that allows you to send and receive objects over TCP. UDP support coming soon.


## Simple Example

The following is an example when sending a simple object.

```csharp
//Server Code

var transport = ObjectTransport.Factory.CreateTCPServer("127.0.0.1",123);

//When the server receives an object of type "Message" then execute the given lambda

transport.Receive<Message>(f => Console.WriteLine(f.Message)).Execute();

//Client Code

var anObjectToSend  = new Message();
anObjectToSend.Message = "Hello World!";

var transport = ObjectTransport.Factory.CreateTCPClient("10.0.0.1",123);

//Send an object
transport.Send(anObjectToSend).Execute();

```

## Seting up multiple responses

The following is an example showing how you can setup multple handlers for different object types

```csharp
//Server

//You can setup a receive handler then you can then specify how to reply when the given object was received.

var transport = ObjectTransport.Factory.CreateTCPServer("127.0.0.1",123);

transport.Receive<LoginRequest>()
        .Reply(lr=> {
            
            string user = string.empty;
            //Check if login is valid
            
            if(utilities.Login(lr, out user))
            {
            
              //Return an object back to the client
              
              var response = new LoginSuccess();
              response.Message = "Login Successful";
              response.User = user;
              return response;
            }
            else
            {
            
              //Return an object back to the client
              
              var response = new LoginFailure();
              response.Message = "Login Failed";
              response.User = user;
              
              return response;
            }
        })
        .Execute();

//Client

var transport = ObjectTransport.Factory.CreateTCPClient("10.0.0.1",123);

var loginRequest = new LoginRequest();
loginRequest.Username = "Test user";
loginRequest.Password = "A password";

//Send an object and setup different response handles
transport.Send(loginRequest)
        .Response<LoginSuccess>(ls=>{
            Console.WriteLine("Welcome Back {0}", ls.User);
        })
        .Response<LoginFailure>(lr=>{
            Console.WriteLine("Login Failed")
         })
         .Execute();
```

## Specifying client to send to

When multiple clients are connected, it is possible to specify which client to send a message to using the "To" function. You can specify multiple clients in the "To" function.

```csharp
//Server Code

var transport = ObjectTransport.Factory.CreateTCPServer("127.0.0.1",123);

//When the server receives an object of type "Message" then execute the given lambda
var anObjectToSend  = new Message();
anObjectToSend.Message = "Hello World!";

transport.Send(anObjectToSend)
         .To(client1,client2)
         .Execute();
```

You can also send to all clients and specify who to exclude:

```csharp

//Send to all clients except client 3
 transport.Send(anObjectToSend)
         .ToAll(client3)
         .Execute();
         
 ```
 
## OnConnect / OnDisconnet handles
 
 You can specify what should happen when someone connects or disconnects:
 
 ```csharp
 
 //Setup onconnect handler
 transport.OnClientConnect(c => Console.WriteLine("A client has connected with ip {0}",c.IPAddress));
 
 //Setup onDisconnect handler
 transport.OnClientDisconnect(c=> Console.WriteLine("A client has disconnected with ip {0}",c.IPAddress));

```
