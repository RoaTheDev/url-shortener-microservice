using DomainService.Infra.Extension;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddWolverineWithOutbox(builder.Configuration);
builder.Services.AddMapster();
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();