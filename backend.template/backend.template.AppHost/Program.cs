using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

#region PostgreSQL
const int port = 5432;
var password = builder.AddParameter("postgres", secret: true);
var postgreSql = builder.AddPostgres("postgres", password: password, port: port)
                        .WithDataVolume("./.containers/database:/var/lib/postgresql/data");
var postgresDb = postgreSql.AddDatabase("backend_template");
#endregion

#region References
var core = builder.AddProject<Projects.backend_template_Core>("core")
                  .WithReference(postgresDb);

var scheduler = builder.AddProject<Projects.backend_template_Scheduler>("scheduler")
                       .WithReference(postgresDb)
                       .WithReference(core);
#endregion

builder.Build().Run();
