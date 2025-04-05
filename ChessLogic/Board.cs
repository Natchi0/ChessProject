using System.Data;
using System.Runtime.CompilerServices;

namespace ChessLogic
{
    public class Board
    {
        private static int[] mailbox = [
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1,  0,  1,  2,  3,  4,  5,  6,  7, -1,
            -1,  8,  9, 10, 11, 12, 13, 14, 15, -1,
            -1, 16, 17, 18, 19, 20, 21, 22, 23, -1,
            -1, 24, 25, 26, 27, 28, 29, 30, 31, -1,
            -1, 32, 33, 34, 35, 36, 37, 38, 39, -1,
            -1, 40, 41, 42, 43, 44, 45, 46, 47, -1,
            -1, 48, 49, 50, 51, 52, 53, 54, 55, -1,
            -1, 56, 57, 58, 59, 60, 61, 62, 63, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
        ];

        private static int[] mailbox64 = [
            21, 22, 23, 24, 25, 26, 27, 28,
            31, 32, 33, 34, 35, 36, 37, 38,
            41, 42, 43, 44, 45, 46, 47, 48,
            51, 52, 53, 54, 55, 56, 57, 58,
            61, 62, 63, 64, 65, 66, 67, 68,
            71, 72, 73, 74, 75, 76, 77, 78,
            81, 82, 83, 84, 85, 86, 87, 88,
            91, 92, 93, 94, 95, 96, 97, 98
        ];

        //                                     caballo alfil torre reina rey  
        private static bool[] slide = { false, false, true, true, true, false };
        private static int[][] offset = [
            [ 0, 0, 0, 0, 0, 0, 0, 0 ],
            [ -21, -19, -12, -8, 8, 12, 19, 21 ], //caballo
            [ -11, -9, 9, 11, 0, 0, 0, 0 ], //alfil
            [ -10, -1, 1, 10, 0, 0, 0, 0 ], //torre
            [ -11, -10, -9, -1, 1, 9, 10, 11 ], //reina
            [ -11, -10, -9, -1, 1, 9, 10, 11 ] //rey
        ];

        public static int[] squares = new int[64];

        static Board()
        {
            LoadStartingBoard();
        }

        public static void LoadStartingBoard()
        {
            string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -0 1";
            LoadPositionFromFen(startFen);

        }

        public static void LoadPositionFromFen(string fen)
        {
            //(pawn = "P", knight = "N", bishop = "B", rook = "R", queen = "Q" and king = "K")
            var piecesFromLetter = new Dictionary<char, int>
            {
                { 'P', Piece.White | Piece.Pawn },
                { 'N', Piece.White | Piece.Knight },
                { 'B', Piece.White | Piece.Bishop },
                { 'R', Piece.White | Piece.Rook },
                { 'Q', Piece.White | Piece.Queen },
                { 'K', Piece.White | Piece.King },
                { 'p', Piece.Black | Piece.Pawn },
                { 'n', Piece.Black | Piece.Knight },
                { 'b', Piece.Black | Piece.Bishop },
                { 'r', Piece.Black | Piece.Rook },
                { 'q', Piece.Black | Piece.Queen },
                { 'k', Piece.Black | Piece.King },
            };

            string boardString = fen.Split(' ')[0];
            int file = 0, rank = 0;

            foreach (char character in boardString)
            {
                if (character == '/')
                {
                    file = 0;
                    rank++;
                    continue;
                }
                else
                {
                    if (Char.IsDigit(character))
                    {
                        file += (int)Char.GetNumericValue(character);
                        continue;
                    }
                    else
                    {
                        var pos = rank * 8 + file;
                        squares[pos] = piecesFromLetter[character];
                        file++;
                    }
                }
            }
        }

        // TODO: Implementar la función para obtener la posición actual en formato FEN
        public static string GetActualFern()
        {
            return "";
        }

        public static void PrintBoard()
        {
            // Unicode para piezas de ajedrez (♔ ♕ ♖ ♗ ♘ ♙ ♚ ♛ ♜ ♝ ♞ ♟)
            Dictionary<int, char> pieceSymbols = new Dictionary<int, char>
            {
                { 0, '.' },       // Vacío
                { 9, '♙' },       // Peón blanco
                { 10, '♘' },       // Caballo blanco
                { 11, '♗' },       // Alfil blanco
                { 12, '♖' },       // Torre blanca
                { 13, '♕' },       // Reina blanca
                { 14, '♔' },       // Rey blanco
                { 1, '♟' },      // Peón negro
                { 2, '♞' },      // Caballo negro
                { 3, '♝' },      // Alfil negro
                { 4, '♜' },      // Torre negra
                { 5, '♛' },      // Reina negra
                { 6, '♚' }       // Rey negro
            };

            Console.WriteLine("    0 1 2 3 4 5 6 7");  // Letras de las columnas
            Console.WriteLine("  +-----------------+");

            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    Console.WriteLine();
                    Console.Write($"{i / 8} | ");
                }

                int piece = squares[i];
                char symbol = pieceSymbols.ContainsKey(piece) ? pieceSymbols[piece] : '.';

                Console.Write($"{piece} ");

            }

            Console.WriteLine();
            Console.WriteLine("  +-----------------+");
        }

        public static void HandlePieceMovement(int IndexActual, int NewIndex)//true si se movió la pieza
        {
            //chequeo que la pieza seleccionada sea del color correspondiente
            int pieceCode = squares[IndexActual];
            int pieceColor = pieceCode & Piece.ColorMask;
            int pieceType = pieceCode & Piece.PieceMask;
			int? PossibleEnPassant = null;
            bool? isOnLastRank = null;

			if (pieceColor != GameState.getState())
            {
                throw new InvalidOperationException($"No es el turno de {(pieceColor == 8 ? "Blanco" : "Negro")}");
            }

            //chequeo que la nueva posición sea válida
            if (!ValidMoveCheck(IndexActual, NewIndex))
            {
                return;
            }

			//verifico no haver llegado a los 50 movimientos
			if (GameState.HalfMoves >= 50)
			{
				throw new InvalidOperationException("Se ha alcanzado el límite de 50 movimientos sin capturas o movimientos de peones.");
			}

            //la posicion es valida, primero verifico en caso de que sea un peon para validar los movimientos en passant
            if (pieceType == Piece.Pawn)
            {
                //verifico que se esté intentando campturar en passant
                if (NewIndex == GameState.EnPassant)
                {
                    //captura en passant
                    squares[NewIndex + (pieceColor == Piece.White ? 8 : -8)] = 0;
                }

                //verifico si está moviendo dos espacios
                if (Math.Abs(NewIndex - IndexActual) == 16)
                {
                    PossibleEnPassant = IndexActual + (pieceColor == Piece.White ? -8 : 8);
                }

                //el peon llego a la ultima fila
                isOnLastRank = (pieceColor == Piece.White && NewIndex >= 0 && NewIndex <= 7) ||
                                     (pieceColor == Piece.Black && NewIndex >= 56 && NewIndex <= 63);
			}
			//setear el en passant
            GameState.EnPassant = PossibleEnPassant;

			//Desactivo los enrroques segun corresponda
			if (pieceType == Piece.King || pieceType == Piece.Rook)
            {
				//verifico si se movió el rey o la torre
				if (pieceType == Piece.King)
				{
					if (pieceColor == Piece.White)
                    {
					    GameState.castleWK = false;
					    GameState.castleWQ = false;
                    }
                    else
                    {
                        GameState.castleBK = false;
					    GameState.castleBQ = false;
                    }
				}
				else
				{
					if (pieceColor == Piece.White)
					{
						if (IndexActual == 56)
						{
							GameState.castleWK = false;
						}
						else if (IndexActual == 63)
						{
							GameState.castleWQ = false;
						}
					}
					else
					{
						if (IndexActual == 0)
						{
							GameState.castleBK = false;
						}
						else if (IndexActual == 7)
						{
							GameState.castleBQ = false;
						}
					}
				}
			}

			//muevo la pieza
			squares[(int)NewIndex] = squares[IndexActual];
            squares[IndexActual] = 0;

			//en caso de capturar una pieza o mover un peon, actualizo los movimientos
            if (squares[NewIndex] != 0 || pieceType == Piece.Pawn)
			{
				GameState.HalfMoves = 0;
			}
			else
			{
				GameState.HalfMoves++;
			}

			if (isOnLastRank == true)
			{
				//cambio el peon por una reina 
				//TODO: cambiar por la pieza que elija el jugador
				squares[NewIndex] = pieceColor | Piece.Queen;
			}

			GameState.ChangeTurn();

        }

        public static int[] GetPosibleMovements(int square)
        {
            int pieceCode = squares[square];
            int pieceType = pieceCode & Piece.PieceMask;
            int pieceColor = pieceCode & Piece.ColorMask;

            if (pieceType == Piece.Pawn) //los movimientos del peon son muy particular, lo muevo a otra funcion para que quede mas limpio
            {
                return GetPosiblePawnMovements(square, pieceCode, pieceType, pieceColor);
			}

            List<int> posibleMovements = new List<int>();

            int[] moves = offset[pieceType - 1];//resto uno ya que las piezas empiezan en 1

            int actualPosition = Array.IndexOf(mailbox, square);
            int valPos64 = mailbox64[square];

            foreach (int move in moves)
            {
                var aux = valPos64 + move;
                var targetPosition = mailbox[aux];

                if (targetPosition == -1)
                    continue;

                if (slide[pieceType - 1])
                {
					while (targetPosition != -1 && (squares[targetPosition] == 0 || ((squares[targetPosition] & Piece.ColorMask) != pieceColor)))
                    {
                        posibleMovements.Add(targetPosition);

						if (squares[targetPosition] != 0) // si hay una pieza en la posición destino me voy despues de agregarla
							break;

						aux = aux + move;
                        targetPosition = mailbox[aux];
                    }

                }
                else
                {
                    if ( squares[targetPosition] == 0 || ((squares[targetPosition] & Piece.ColorMask) != pieceColor) )
                    {
                        posibleMovements.Add(targetPosition);
                    }
                }
            }

            if(pieceType == Piece.King)
            {
                List<int> castlingMoves = new List<int>();
                castlingMoves = GetCastlingMoves(pieceColor);

                posibleMovements.AddRange(castlingMoves);
			}

            return posibleMovements.ToArray();
        }

        private static int[] GetPosiblePawnMovements(int square, int pieceCode, int pieceType, int pieceColor)
        {
			List<int> posibleMovements = new List<int>();
			int colorMultiplier = pieceColor == Piece.ColorMask ? -1 : 1; //asigno todos los valores y multiplico segun el color de la pieza

			int oneStep = 10 * colorMultiplier;
			int leftCapture = 9 * colorMultiplier;
			int rightCapture = 11 * colorMultiplier;

			int actualPosition = Array.IndexOf(mailbox, square);
			int valPos64 = mailbox64[square];


			//chequeo el movimiento hacia adelante de la primera casilla
			int oneStepIndex = valPos64 + oneStep;
			int oneStepTarget = mailbox[oneStepIndex];
			bool canAdvanceOne = oneStepTarget != -1 && squares[oneStepTarget] == 0;

            if (canAdvanceOne)
            {
                posibleMovements.Add(oneStepTarget);

                //movimiento de 2 casillas si está en la fila inicial
                bool isOnStartRank = (pieceColor == Piece.White && square >= 48 && square <= 55) ||
                                     (pieceColor == Piece.Black && square >= 8 && square <= 15);

                if (isOnStartRank)
                {
					int twoStep = 20 * colorMultiplier;
					int twoStepIndex = valPos64 + twoStep;
					int twoStepTarget = mailbox[twoStepIndex];

					if (twoStepTarget != -1 && squares[twoStepTarget] == 0)
					{
						posibleMovements.Add(twoStepTarget);
					}
				}
			}

			// chequeo las apturas
			foreach (int capture in new[] { leftCapture, rightCapture })
			{
				int captureIndex = valPos64 + capture;
				int captureTarget = mailbox[captureIndex];

				if (captureTarget != -1 && ((squares[captureTarget] != 0 && (squares[captureTarget] & Piece.ColorMask) != pieceColor) || captureTarget == GameState.EnPassant))
				{
					posibleMovements.Add(captureTarget);
				}
			}

			return posibleMovements.ToArray();
		}

        private static List<int> GetCastlingMoves(int pieceColor)
		{
			List<int> castlingMoves = new List<int>();
			if (pieceColor == Piece.White)//la pieza es blanca
			{
				if (GameState.castleWK) //verifico KinkSide
				{
                    bool isValid = true;
					//cerificar que no hayan piezas entre el rey y la torre
					for (int i = 61; i < 63; i++)
                    {
						if (squares[i] != 0)
						{
							isValid = false;
							break;
						}
					}
					if (isValid)
					{
						castlingMoves.Add(62);
					}
				}
				if (GameState.castleWQ) //verifico QueenSide
				{
                    bool isValid = true;
					for (int i = 57; i < 60; i++)
					{
						if (squares[i] != 0)
						{
							isValid = false;
							break;
						}
					}
					if (isValid)
					{
						castlingMoves.Add(58);
					}
				}
			}
			else
			{
				if (GameState.castleBK)
				{
                    bool isValid = true;
					for (int i = 5; i < 7; i++)
					{
						if (squares[i] != 0)
						{
							isValid = false;
							break;
						}
					}
					if (isValid)
					{
						castlingMoves.Add(6);
					}
				}
				if (GameState.castleBQ)
				{
					bool isValid = true;
					for (int i = 0; i < 4; i++)
					{
						if (squares[i] != 0)
						{
							isValid = false;
							break;
						}
					}
					if (isValid)
					{
						castlingMoves.Add(2);
					}
				}
			}
			return castlingMoves;
		}

		private static bool ValidMoveCheck(int IndexActual, int NewIndex)
        {
            int[] moves = GetPosibleMovements(IndexActual);
			foreach (int move in moves)
			{
				if (move == NewIndex)
				{
					return true;
				}
			}
			return false;
        }
    }
}
