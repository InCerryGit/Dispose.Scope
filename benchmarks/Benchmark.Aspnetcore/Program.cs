using Dispose.Scope.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
app.UsePooledScope(options =>
{
    options.DisposeObjListDefaultSize = 16;
});

app.MapGet("/", () => "Hello World!");

app.MapControllers();

app.Run();