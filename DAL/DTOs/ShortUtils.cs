using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTOs
{
	public class MakeMoveRequest
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
	}

	public class JoinRequest
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
	}
}
