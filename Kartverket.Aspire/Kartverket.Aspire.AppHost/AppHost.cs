using Kartverket.Aspire.AppHost.MariaDb;

var builder = DistributedApplication.CreateBuilder(args);

var mariaDbServer = builder.AddMariaDb("mariadb")
                   .WithDataBindMount(source: @"../../../MariaDb/Data")
                   .WithLifetime(ContainerLifetime.Persistent);

var mariaDb = mariaDbServer.AddDatabase("kartverketdb");

//Bruk enten dockerfile varianten eller native, ikke begge 

//Variant dockerfile
//builder.AddDockerfile("kartverket-web", "../../", "Kartverket.Web/Dockerfile")
//                       .WithExternalHttpEndpoints()
//                       .WithReference(mariaDb)
//                       .WaitFor(mariaDb)
//                       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "kartverket-web");

//Det tar en time å gå ned til Ørsta rådhus!

//Variant native 
builder.AddProject<Projects.Kartverket_Web>("kartverket-web")
                       .WithReference(mariaDb)
                       .WaitFor(mariaDb);
builder.Build().Run();
