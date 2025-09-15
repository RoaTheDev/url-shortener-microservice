using DomainService.Infra.Extension;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddWolverineWithOutbox(builder.Configuration);
builder.Services.AddMapster();
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddControllers();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();