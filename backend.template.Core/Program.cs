using backend.template.Data.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<CoreContext>("backend_template");

builder.AddServiceDefaults();
var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
