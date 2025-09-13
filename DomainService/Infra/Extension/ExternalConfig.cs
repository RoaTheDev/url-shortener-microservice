using System.ComponentModel.DataAnnotations;
using DomainService.Infra.Persistent;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
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
        services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("ConnectionStr")));
    }

    public static void AddMapster(this IServiceCollection services) => services.AddTransient<IMapper, Mapper>();

    public static IHostBuilder AddWolverineWithOutbox(this IHostBuilder host, IConfiguration config)
    {
        host.UseWolverine(opts =>
        {
            opts.Policies.UseDurableLocalQueues();

            opts.UseRabbitMq(rmq =>
                {
                    rmq.HostName = config["RabbitMQ:Host"]!;
                    rmq.Port = Convert.ToInt32(config["RabbitMQ:Port"]);
                    rmq.UserName = config["RabbitMQ:Username"]!;
                    rmq.Password = config["RabbitMQ:Password"]!;
                    rmq.VirtualHost = config["RabbitMQ:VirtualHost"]!;
                })
                .AutoProvision()
                .BindExchange("domain-events").ToQueue("inventory-service-queue");

            opts.PublishAllMessages().ToRabbitExchange("domain-events");

            opts.PersistMessagesWithPostgresql(config.GetConnectionString("ConnectionStr")!);
            opts.UseEntityFrameworkCoreTransactions();

            opts.Policies.OnException<ValidationException>()
                .RetryWithCooldown(
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(500)
                );
        });
        return host;
    }
}