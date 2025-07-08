using ChessUtilsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
	public class MatchRequest
	{
		public int Id { get; set; }
		public int PlayerId { get; set; }
		public Player? Player { get; set; }
		public DateTime RequestedAt { get; set; }
		public EMatchRequestStatus Status { get; set; } = EMatchRequestStatus.None;
		public int? MatchedPlayerId { get; set; }
		[ForeignKey("MatchedPlayerId")]
		public Player? MatchedWith { get; set; }
		public int? GameId { get; set; }
		public Game? Game { get; set; }

		public static MatchRequest CreateWaiting(int playerId)
		{
			return new MatchRequest
			{
				PlayerId = playerId,
				RequestedAt = DateTime.UtcNow,
				Status = EMatchRequestStatus.Waiting,
			};
		}
	}
}
