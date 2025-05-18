using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTOs
{
	public record SocketMessage
	{
		public string Type { get; set; } = string.Empty;
	}

	/*
	{
		"Type": "join",
		"GameId": 7,
		"PlayerId": 2
	}
	*/
	public record JoinMessage : SocketMessage
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
	}

	/*
	{
		"Type": "move",
		"GameId": 7,
		"PlayerId": 1,
		"FromIndex": 0,
		"ToIndex": 2
	}
	*/
	public record MoveMessage : SocketMessage
	{
		public int GameId { get; init; }
		public int PlayerId { get; init; }
		public int FromIndex { get; init; }
		public int ToIndex { get; init; }
	}

	/*
	{
		"Type": "resign",
		"GameId": 7,
		"PlayerId": 1
	}
	*/
	public record ResignMessage : SocketMessage
	{
		public int GameId { get; init; }
		public int PlayerId { get; init; }
	}
}
