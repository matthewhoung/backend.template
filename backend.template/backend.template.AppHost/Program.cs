var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.backend_template_Scheduler>("backend-template-scheduler");

builder.AddProject<Projects.backend_template_Core>("backend-template-core");

builder.Build().Run();
