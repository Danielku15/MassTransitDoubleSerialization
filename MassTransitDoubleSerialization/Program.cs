using System.Reflection;
using MassTransit;
using MassTransit.Serialization;

namespace MassTransitDoubleSerialization;

public class Program
{
    public static async Task Main(string[] args)
    {
        SystemTextJsonMessageSerializer.Options.Converters.Add(new PointConverter());
        var host = CreateHostBuilder(args).Build();
        await host.StartAsync();

        Console.WriteLine("hit space to send a point, any other key to exit");

        var bus = host.Services.GetRequiredService<IBus>();
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Spacebar)
            {
                var builder = new RoutingSlipBuilder(NewId.NextGuid());

                builder.AddActivity(
                    nameof(PointActivity),
                    new Uri("queue:point_execute"),
                    new PointActivityArguments
                    {
                        Point = new Point
                        {
                            X = 1.2,
                            Y = 2.3
                        }
                    });

                var routingSlip = builder.Build();
                await bus.Execute(routingSlip);
            }
            else
            {
                break;
            }
        }

        await host.StopAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();

                    // By default, sagas are in-memory, but should be changed to a durable
                    // saga repository.
                    x.SetInMemorySagaRepositoryProvider();

                    var entryAssembly = Assembly.GetEntryAssembly();

                    x.AddConsumers(entryAssembly);
                    x.AddSagaStateMachines(entryAssembly);
                    x.AddSagas(entryAssembly);
                    x.AddActivities(entryAssembly);

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost",
                            "/",
                            h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });
                        
                        cfg.ConfigureEndpoints(context);
                    });
                });
            });
}