namespace DAL.Models
{
	public class Move
	{
		public int Id { get; set; }

		public int GameId { get; set; }

		public required Game Game { get; set; }

		public int MovNum { get; set; } // equivalente al turno de la jugada

		public int? PlayerId { get; set; }

		public Player? Player { get; set; }

		public int[] Movement { get; set; } = new int[2]; // [from index, to index]
	}
}
