using System.ComponentModel.DataAnnotations;
using DomainService.Infra.Persistent;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace DomainService.Infra.Extension;

public static class ExternalConfig
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("ConnectionStr")),
            optionsLifetime: ServiceLifetime.Singleton);
    }

    public static void AddMapster(this IServiceCollection services) => services.AddTransient<IMapper, Mapper>();

    public static void AddSerilogConfig(this IHostBuilder host, string serviceName = "DomainService") =>
        host.UseSerilog((context, config) => 
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.WithProperty("ServiceName", serviceName)  
                .Enrich.WithProperty("Version", GetVersion())      
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
        });

    public static void AddWolverineWithOutbox(this IHostBuilder host, IConfiguration config)
    {
        host.UseWolverine(opts =>
        {
            opts.UseRabbitMq(rmq =>
                {
                    rmq.HostName = config["RabbitMQ:Host"]!;
                    rmq.Port = Convert.ToInt32(config["RabbitMQ:Port"]);
                    rmq.UserName = config["RabbitMQ:Username"]!;
                    rmq.Password = config["RabbitMQ:Password"]!;
                    rmq.VirtualHost = config["RabbitMQ:VirtualHost"]!;
                })
                .AutoProvision()
                .BindExchange("domain-events").ToQueue("domain-service-queue");
            opts.PublishAllMessages().ToRabbitExchange("domain-events").UseDurableOutbox();
            // opts.ListenToRabbitQueue("domain-service-queue");


            opts.PersistMessagesWithPostgresql(config.GetConnectionString("ConnectionStr")!);
            opts.UseEntityFrameworkCoreTransactions();

            opts.Durability.Mode = DurabilityMode.Balanced;
            opts.Durability.KeepAfterMessageHandling = TimeSpan.FromHours(1);

            opts.Policies.AutoApplyTransactions();
            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
            opts.Policies.UseDurableInboxOnAllListeners();
            opts.Policies.OnException<ValidationException>()
                .RetryWithCooldown(
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(500)
                );
        });
    }
    private static string GetVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "1.0.0";
    }
}