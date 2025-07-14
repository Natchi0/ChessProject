namespace Shared
{
	public static class RoutingKey
	{
		public static readonly string GameUpdated = "game.updated";
		public static readonly string MoveRejected = "move.rejected";
		public static readonly string MatchFound = "match.found";//match found anuncia que se hizo match pero el juego aun noe sta listo
		public static readonly string FindMatch = "find.match";
		public static readonly string Move = "move";
		public static readonly string Join = "join";
		public static readonly string Leave = "leave";
		public static readonly string CreateGame = "create.game";
		public static readonly string GameCreated = "game.created";//el juego fue creado, esta listo apra jugar
	}
}
