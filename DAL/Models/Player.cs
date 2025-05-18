using Microsoft.AspNetCore.Identity;

namespace DAL.Models
{
	public class Player
	{
		public int Id { get; set; }

		public int? UserId { get; set; }
		//navegacion
		public IdentityUser<int>? User { get; set; }

		public int? AnonId { get; set; }

		public DateTime CreatedAt { get; set; }

		public string? Nickname { get; set; }
	}
}
