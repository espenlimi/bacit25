
var builder = DistributedApplication.CreateBuilder(args);


var mysql = builder.AddMySql("mysql")
                   .WithDataBindMount(source: @"../../../MySql/Data") // Changed to relative path
                   .WithLifetime(ContainerLifetime.Persistent);


var mysqldb = mysql.AddDatabase("mysqldb");

//Bruk enten dockerfile varianten eller native, ikke begge 

//Variant dockerfile
builder.AddDockerfile("kartverket-web", "../../", "Kartverket.Web/Dockerfile")
                       .WithExternalHttpEndpoints()
                       .WithReference(mysqldb)
                       .WaitFor(mysqldb)
                       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "kartverket-web");


//Det tar en time å gå ned til Ørsta rådhus!

//Variant native 
/*builder.AddProject<Projects.Kartverket_Web>("kartverket-web")
                       .WithReference(mysqldb)                      
                       .WaitFor(mysqldb); 
*/

builder.Build().Run();
