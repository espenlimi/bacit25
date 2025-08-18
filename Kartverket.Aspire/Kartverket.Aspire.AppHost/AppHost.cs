using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


var mysql = builder.AddMySql("mysql")
                   .WithDataBindMount(source: @"C:\MySql\Data") // Adjust the path as needed
                   .WithLifetime(ContainerLifetime.Persistent);

var mysqldb = mysql.AddDatabase("mysqldb");

builder.AddDockerfile("kartverket-web", "../../", "Kartverket.Web/Dockerfile").WithExternalHttpEndpoints()
                       .WithReference(mysqldb)
                       .WaitFor(mysqldb)
                       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "kartverket-web");

//builder.AddProject<Projects.Kartverket_Web>("kartverket-web")
//                       .WithReference(mysqldb)
//                       .WaitFor(mysqldb);

builder.Build().Run();
