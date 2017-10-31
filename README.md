<img src="https://ci.appveyor.com/api/projects/status/bmq9qol3a49stp6u?svg=true" /> 
<a href="https://www.nuget.org/packages/ObjectTransport">Download From Nuget</a>

# ObjectTransport
A lightweight library that allows you to send and receive objects over TCP or UDP. It is possible to send objects over UDP reliably if needed.

Can serialize any field type as long as they are primitives inlcuding arrays. Also, if a field type is of a class type which also contains primitives, the object assigned to the field will also be serialized.

## Simple Example

### Starting the server

You can start a TCP server with the following code

```csharp
var server = ObjectTransport.Factory.CreateTCPServer("127.0.0.1",123);

```

or you can start a UDP server

```csharp
var server = ObjectTransport.Factory.CreateUDPServer("127.0.0.1",123);

```

### Receiving an Object

In this example we have a scenario where we want to handle a user logging into the server. Suppose we have a simple class called "LoginModel". For now this class only has the field "Username"

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

In the above code, we specify that when the server Receives an object of type "LoginModel", execute the given lambda. We then write the Username to the console.

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

```csharp
var loginRequest = new LoginModel()
loginRequest.Username = "TestUser";

client.Send(loginRequest).Execute();
```

## Setting up multiple responses

In the following example, we will show how a server/client can reply to a received object. 

In our previous example, we are currently sending a Username to the server but not our password, which isn't very secure. In this example, we update our model to have a "Password" field:

```csharp
public class LoginModel
{
        public string Username {get;set;}        
        public string Password {get;set;}    
}
```

### Sending Login Request from the client

Our client needs to send a login request to the server and will now need to send their password as well. Due to this, we want to handle any responses to our request including whether or not the login was successful. To handle this, we create two new classes "LoginSuccess" and "LoginFailure".

```csharp
public class LoginSuccess
{
        public string Name {get;set;}        
        public string Password {get;set;}    
}

public class LoginFailure
{
        public string Message {get;set;}                
}
```


In our client code, we will now use the "Response" function after sending the login object. When the server replies to the object that was sent, the client will handle it's responses:

```csharp
var transport = ObjectTransport.Factory.CreateTCPClient("10.0.0.1",123);

var loginRequest = new LoginModel();
loginRequest.Username = "TestUser";
loginRequest.Password = "A password";

transport.Send(loginRequest)
        .Response<LoginSuccess>(ls=>{
            Console.WriteLine("Welcome Back {0}", ls.Name);
        })
        .Response<LoginFailure>(lr=>{
            Console.WriteLine(lr.Message)
         })
         .Execute();

```
In the above example, we setup 2 response handles, one to handle "LoginSuccess" and another to handle "LoginFailure".

On the server, we will use the "Reply" function after receiving a login model. When using this function we need use a function/lambda which will "return" an object that will be sent back:

```csharp

server.Receive<LoginModel>()
        .Reply(lr=> {
            
            string user = string.empty;
            //Check if login is valid
            
            if(utilities.Login(lr, out user))
            {
            
              //Return an object back to the client
              
              var response = new LoginSuccess();
              response.Message = "Login Successful";
              response.Name = user;
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

```

## Specifying client to send to

When multiple clients are connected, it is possible to specify which client to send a message to using the "To" function. You can specify multiple clients in the "To" function.

```csharp
server.Send(anObjectToSend)
         .To(client1,client2)
         .Execute();
```
### Send to all clients

You can send to all clients using the following.

```csharp

//Send to all clients except client 3
 server.Send(anObjectToSend)
         .ToAll()
         .Execute();
         
 ```

### Send to all clients except given clients
You can also send to all clients and specify who to exclude:

```csharp

//Send to all clients except client 3
 server.Send(anObjectToSend)
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

## Sending object reliably

When sending objects over UDP, the message is sent without reliability. You can switch reliably on for UDP on with the following:

```csharp
 client.SetReliable();
```

After executing the above line, all objects that are sent will be sent reliably.

Another option is to send only a specific message reliably. The following demonstrates this:

```csharp

 client.Send(anObjectToSend)
         .Reliable();
         .Execute();
```
