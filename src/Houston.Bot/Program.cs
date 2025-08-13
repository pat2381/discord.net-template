using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TicketBot.Crons;
using TicketBot.Crons.Jobs;
using TicketBot.Database;
using TicketBot.Database.Entities;
using TicketBot.Services;

namespace TicketBot;

internal static class Program
{
	private static async Task Main(string[] args)
	{
		var host = Host.CreateDefaultBuilder(args)
			.UseSystemd()
			.UseSerilog()
			.ConfigureServices(ConfigureServices)
			.ConfigureLogging((context, logging) =>
			{
				logging.ClearProviders();
			})
			.Build();

		var jobSchedulerService = host.Services.GetRequiredService<JobSchedulerService>();
		await jobSchedulerService.Start();

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(host.Services.GetRequiredService<IConfiguration>())
			.CreateLogger();

		try
		{
			if (args.Contains("--skip-migration"))
			{
				Log.Information("Skipping database migration");
			}
			else
			{
				Log.Information("Migrating database");
				using var scope = host.Services.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
				await db.Database.MigrateAsync();
			}

			Log.Information("Running host");
			await host.RunAsync();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			Log.Fatal(ex, "Host terminated unexpectedly");
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}

	private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
	{
        var botOptions = context.Configuration.GetSection("Bot").Get<BotOptions>() ?? new BotOptions();
        services.AddSingleton(typeof(ILogger<>), typeof(Services.Logger<>));

		services.AddDbContext<DatabaseContext>(option =>
		{
			option.UseSqlite($"Data Source={botOptions.DbPath}");
        });
        services.AddDiscordShardedHost((config, _) =>
        {
            config.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200,
				GatewayIntents = GatewayIntents.Guilds
                            | GatewayIntents.GuildMessages
                            | GatewayIntents.DirectMessages
                            | GatewayIntents.GuildMessageReactions
                            | GatewayIntents.MessageContent
							| GatewayIntents.GuildMembers
            };

            config.Token = botOptions.Token;
        });

        services.AddInteractionService((config, _) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.UseCompiledLambda = true;
			
        });

        //services.AddSingleton<InteractionService>();
		//services.AddSingleton<CommandService>();
		services.AddHostedService<InteractionHandlingService>();
		services.AddHostedService<CommandHandlingService>();

		services.AddQuartz(q => q.UseJobFactory<JobFactory>());
		services.AddSingleton<JobSchedulerService>();
		services.AddSingleton<ScheduleJobs>();

		services.AddSingleton<ExampleJob>();

		services.AddHostedService<BotService>();
	}
}