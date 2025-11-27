using Aspire.Hosting;
using ReplicaSet.Aspire.MongoDB;

var builder = DistributedApplication.CreateBuilder(args);

var mongoDbUsername = builder.AddParameter("MongoDb-Username", "admin");
var mongoDbPassword = builder.AddParameter("MongoDb-Password", "admin", secret: true);

var mongo = builder.AddMongoDB("mongo", 27017, mongoDbUsername, mongoDbPassword)
    .WithImageTag("8.2.2")
    .WithContainerName("mongo")
    .WithVolume("mongodb-data", "/data/db")
    .WithVolume("mongodb-configdb", "/data/configdb")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(
        "tcp",
        e =>
        {
            e.Port = 27017;
            e.TargetPort = 27017;
            e.IsProxied = false;
            e.IsExternal = false;
        })
    .WithReplicaSet()
    .WithMongoExpress(mongoExpress =>
      mongoExpress
      .WithContainerName("mongo-express")
      .WithEndpoint(
        "http",
        e =>
        {
            e.Port = 8081;
            e.TargetPort = 8081;
            e.IsProxied = false;
            e.IsExternal = false;
        })
     .WithLifetime(ContainerLifetime.Persistent)
    )                   
    .WithDbGate(dbGate=>
    dbGate.WithContainerName("dbgate")
   .WithEndpoint(
        "http",
        e =>
        {
            e.Port = 300;
            e.TargetPort = 3000;
            e.IsProxied = false;
            e.IsExternal = false;
        })
   .WithVolume("dbgate-data", "/root/.dbgate")
    .WithLifetime(ContainerLifetime.Persistent)
    );

var mongodb = mongo.AddDatabase("mongoDatabase");
var mongoReplicaSet = builder
    .AddMongoReplicaSet("mongoDb", mongodb.Resource);
builder.AddProject<Projects.AspireMongoDbWithReplicaSet>("aspiremongodbwithreplicaset")
    .WithReference(mongodb)
    .WithReference(mongoReplicaSet)
    .WaitFor(mongodb)
    .WaitFor(mongoReplicaSet);

builder.Build().Run();
