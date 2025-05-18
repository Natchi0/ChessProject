using DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
	public class AppDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Player> Players { get; set; }
		public DbSet<Game> Games { get; set; }
		public DbSet<Move> Moves { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			//modelBuilder.Entity<Player>()
			//	.HasOne(p => p.User)
			//	.WithMany()
			//	.HasForeignKey(p => p.UserId)
			//	.OnDelete(DeleteBehavior.SetNull);
			//modelBuilder.Entity<Game>()
			//	.HasOne(g => g.Player1)
			//	.WithMany()
			//	.HasForeignKey(g => g.PlayerId1)
			//	.OnDelete(DeleteBehavior.SetNull);
			//modelBuilder.Entity<Game>()
			//	.HasOne(g => g.Player2)
			//	.WithMany()
			//	.HasForeignKey(g => g.PlayerId2)
			//	.OnDelete(DeleteBehavior.SetNull);
			//modelBuilder.Entity<Move>()
			//	.HasOne(m => m.Game)
			//	.WithMany()
			//	.HasForeignKey(m => m.GameId)
			//	.OnDelete(DeleteBehavior.Cascade);
			//modelBuilder.Entity<Move>()
			//	.HasOne(m => m.Player)
			//	.WithMany()
			//	.HasForeignKey(m => m.PlayerId)
			//	.OnDelete(DeleteBehavior.SetNull);

			PasswordHasher<IdentityUser<int>> passwordHasher = new PasswordHasher<IdentityUser<int>>();

			var user1 = new IdentityUser<int>()
			{
				Id = 1,
				Email = "test@example1.com",
				NormalizedEmail = "TEST@EXAMPLE1.COM",
				UserName = "test1",
				NormalizedUserName = "TEST1",
				ConcurrencyStamp = "11111111-1111-1111-1111-111111111111",
				SecurityStamp = "22222222-2222-2222-2222-222222222222"
			};
			var user2 = new IdentityUser<int>()
			{
				Id = 2,
				Email = "test@example2.com",
				NormalizedEmail = "TEST@EXAMPLE2.COM",
				UserName = "test2",
				NormalizedUserName = "TEST2",
				ConcurrencyStamp = "33333333-3333-3333-3333-333333333333",
				SecurityStamp = "44444444-4444-4444-4444-444444444444"
			};

			user1.PasswordHash = "AQAAAAIAAYagAAAAEDel75C1UyO7+ftDht11k+LnMnur1SO7cMUYgZCDNsBanZE2Nl4Kd3xtTyBS6mY+Hg==";
			user2.PasswordHash = "AQAAAAIAAYagAAAAEJ/mP72fwry5hglb7ubp1kd7e++gc6+fD5nI0p/smOIe0daLcSjDQ7UY4PyO+ocp4Q==";

			modelBuilder.Entity<IdentityUser<int>>().HasData(user1, user2);


			modelBuilder.Entity<Player>().HasData(
				new Player
				{
					Id = 1,
					UserId = user1.Id,
					CreatedAt = new DateTime(2024, 1, 1),
					Nickname = "TestPlayer1"
				},
				new Player
				{
					Id = 2,
					UserId = user2.Id,
					CreatedAt = new DateTime(2024, 1, 1),
					Nickname = "TestPlayer2"
				}
			);
		}

	}
}
