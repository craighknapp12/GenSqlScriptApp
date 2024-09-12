using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

try
{
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .Build();

    var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services ) =>
        {
            services.AddTransient<IScriptCreator, ScriptCreator>();
        })
        .Build();

    if (args.Length != 2)
    {
        Console.WriteLine("Expect:");
        Console.WriteLine("\t <schemafile> <outputscriptFile>");
    }
    else
    {
        var scriptCreator = host?.Services.GetService<IScriptCreator>();
        if (scriptCreator != null)
        {
            return scriptCreator.Generate(args[0], args[1]);
        }
    }

    return -1;
}
catch (Exception e)
{
    Console.WriteLine($"Captured exception:{e.Message} ");
    return -2;
}
