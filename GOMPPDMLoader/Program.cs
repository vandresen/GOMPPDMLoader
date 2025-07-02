using GOMPPDMLoader;
using GOMPPDMLoaderLibrary;
using GOMPPDMLoaderLibrary.Data;
using GOMPPDMLoaderLibrary.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IDataTransfer, DataTransfer>();
        services.AddHttpClient<IWellData, Welldata>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        //services.AddSingleton<IWellData, Welldata>();
        services.AddSingleton<IDataAccess>(provider => new DapperDataAccess(commandTimeout: 300));
        services.AddSingleton<App>();
    })
    .Build();
var app = host.Services.GetRequiredService<App>();

Option<string> connectionOption = new("--connection")
{
    Description = "Database connection string",
    Required = true,
};

Option<string> datatypeOption = new("--datatype")
{
    Description = "Data type to process: Wellbore or Deviations",
    Required = true,
};

RootCommand command = new()
{
    connectionOption,
    datatypeOption,
};

command.SetAction(async parseResult => await app.Run(parseResult.GetValue(connectionOption), parseResult.GetValue(datatypeOption)));

return await new CommandLineConfiguration(command).InvokeAsync(args);