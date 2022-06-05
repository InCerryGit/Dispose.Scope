using System.Diagnostics;
using System.Runtime.InteropServices;
using Dispose.Scope.AspNetCore;

// set core 11 and 12
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Process.GetCurrentProcess().ProcessorAffinity = (IntPtr) 0xC00;
}


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddControllers();

var app = builder.Build();

if (Environment.GetEnvironmentVariable("Use_Pooled") == "1")
{
    app.UsePooledScope(options =>
    {
        options.DisposeObjListDefaultSize = 16;
    });
    Console.WriteLine("Used pooled scope");
}

app.MapGet("/", () => "Hello World!");

app.MapControllers();

app.Run();