using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

#region PostgreSQL
var postgresDb = builder.AddConnectionString("Postgresql");
#endregion

#region References
var core = builder.AddProject<Projects.backend_template_Core>("core")
                  .WithReference(postgresDb);

var scheduler = builder.AddProject<Projects.backend_template_Scheduler>("scheduler")
                       .WithReference(postgresDb)
                       .WithReference(core);
#endregion

builder.Build().Run();
