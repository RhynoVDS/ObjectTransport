<img src="https://ci.appveyor.com/api/projects/status/bmq9qol3a49stp6u?svg=true" /> 
<a href="https://www.nuget.org/packages/ObjectTransport">Download From Nuget</a>

# ObjectTransport
A lightweight library that allows you to send and receive objects over TCP. UDP support coming soon.

Can serialize any field type as long as they are primitives inlcuding arrays. Also, if a field type is of a class type which also contains primitives, the object assigned to the field will also be serialized.

## Simple Example

### Starting the server

You can start a server with the following code

```csharp
var server = ObjectTransport.Factory.CreateTCPServer("127.0.0.1",123);

```
### Receiving an Object

In this example, we want to handle a user logging into the server. Suppose we have a simple class called "LoginModel". For now this class only has the field "Username"

```csharp
public class LoginModel
{
        public string Username {get;set;}        
}
```

We want the server to receive this object and handle it. This can be done using the "Receive" function:

```csharp
server.Receive<LoginModel>(lm => 
                                {
                                  Console.WriteLine(lm.Username);
                                })
                              .Execute();
```

In the above code, we specify that when the server Receives an object of type "LoginModel", execute the given lambda. The received object is passed into the lambda as "lm". We then write the Username to the console.

It is possible to set up multiple Receive functions and handle other types:

```csharp
server.Receive<LoginModel>(lm => ... ).Execute();

server.Receive<LogOutModel>(lm => ... ).Execute();

server.Receive<PlayerPosition>(lm => ... ).Execute();
...
```

### Starting the client

You can start a TCP client with the following code:

```csharp
var client = ObjectTransport.Factory.CreateTCPClient("10.0.0.1",123);
```

To send an object over the channel, use the "Send" function:

```
var loginRequest = new LoginModel()
loginRequest.Username = "TestUser";

client.Send(loginRequest).Execute();
```

## Setting up multiple responses

In the following example, we will show how a server/client can reply to a received object. 

In our previous exapmle, we are currently sending a Username to the server but not our password, which isn't very secure. In this example, we update our model to have a "Password" field:

```csharp
public class LoginModel
{
        public string Username {get;set;}        
        public string Password {get;set;}    
}
```

### Sending Login Request from the client

Our client needs to send a login request to the server and will now need to send their password as well. Due to this, we want to handle any responses to our request including whether or not the login was successful. To handle this, we create two new classes "LoginSuccess" and "LoginFailure" each which contain a property "Message".

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

var anObjectToSend  = new Message();
anObjectToSend.Message = "Hello World!";

//Send the object to the specified clients
transport.Send(anObjectToSend)
         .To(client1,client2)
         .Execute();
```
## Send to all clients except given clients
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
