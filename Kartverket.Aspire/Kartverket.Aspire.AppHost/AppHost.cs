using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


var mysql = builder.AddMySql("mysql")
                   .WithLifetime(ContainerLifetime.Persistent);

var mysqldb = mysql.AddDatabase("mysqldb");

builder.AddProject<Projects.Kartverket_Web>("kartverket-web")
                       .WithReference(mysqldb)
                       .WaitFor(mysqldb);
builder.Build().Run();
