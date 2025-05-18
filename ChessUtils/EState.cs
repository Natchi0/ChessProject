using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUtilsLib
{
	public enum EState
	{
		None,
		WhiteTurn,
		BlackTurn,
		Finished
	}

	public enum EEndGameState
	{
		WhiteResigned,
		BlackResigned,
		WhiteWin,
		BlackWin,
		Draw,
		Stalemate,
		//Player1Resigned,
		//Player2Resigned,
		//Player1Win,
		//Player2Win,
	}
}
