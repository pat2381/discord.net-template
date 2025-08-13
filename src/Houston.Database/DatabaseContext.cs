using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TicketBot.Database.Entities;
using TicketBot.Database.Extensions;
using System;

namespace TicketBot.Database;

public class DatabaseContext : DbContext
{
	public DbSet<ReputationMember> ReputationMembers { get; internal set; }
	public DbSet<User> Users { get; internal set; }

	private readonly string _connectionString;

	public DatabaseContext(IConfiguration configuration)
	{
		_connectionString = Environment.GetEnvironmentVariable("DATABASE") ?? configuration["Bot:DbPath"];

	}

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		options.UseSqlite($"Data Source={_connectionString}");
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ConfigureGuildEntities();
		modelBuilder.ConfigureMemberEntities();
		modelBuilder.ConfigureOtherEntities();
	}
}
