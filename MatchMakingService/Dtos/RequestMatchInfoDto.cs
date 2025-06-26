using DAL.Models;

namespace MatchMakingService.Dtos
{
	public class RequestMatchInfoDto
	{
		public MatchRequest MatchRequest { get; set; } = new MatchRequest();
		public MatchInfo? MatchInfo { get; set; }
	}

	public class MatchInfo
	{
		public int Player1Id { get; set; }
		public int Player2Id { get; set; }
		public int GameId { get; set; }
		public int[] BoardState { get; set; } = new int[64];
	}
}
