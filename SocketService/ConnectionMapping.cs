namespace MatchMakingService
{
	public static class ConnectionMapping
	{
		private static readonly Dictionary<int, string> _connections = new Dictionary<int, string>();

		public static void Add(int playerId, string connectionId)
		{
			//TODO: agregar soporte para diferenciar entre varios juegos del mismo jugador
			_connections.Add(playerId, connectionId);
		}

		public static void RemoveByConnection(string connectionId)
		{
			var connection = _connections.FirstOrDefault(c => c.Value == connectionId);

			if (connection.Key != 0)
			{
				_connections.Remove(connection.Key);
			}
		}

		public static void RemoveByPlayerId(int playerId)
		{
			if (_connections.ContainsKey(playerId))
			{
				_connections.Remove(playerId);
			}
		}

		public static string? GetConnectionId(int playerId)
		{
			_connections.TryGetValue(playerId, out var connectionId);
			return connectionId;
		}

		public static int? GetPlayerId(string connectionId)
		{
			var connection = _connections.FirstOrDefault(c => c.Value == connectionId);
			return connection.Key != 0 ? connection.Key : null;
		}
	}
}
