namespace GameServer.Dtos
{
	public class MoveRequestDto
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
	}
}
