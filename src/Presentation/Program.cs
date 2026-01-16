using Application.Extensions;
using Infrastructure.Db.Options;
using Infrastructure.Extensions;
using Itmo.Dev.Platform.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.Configure<DatabaseConfigOptions>(builder.Configuration.GetSection("Postgres"));

builder.Services.AddPlatform();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddKafkaApplication(builder.Configuration);
builder.Services.AddPresentation();

WebApplication app = builder.Build();

app.Run();