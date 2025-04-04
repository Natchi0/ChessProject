// See https://aka.ms/new-console-template for more information
using ChessLogic;


Console.WriteLine("Hello, World!");

Board.PrintBoard();

int[] moves = { -10, -9, -11, -20 };

foreach (int move in moves)
{
	if (move % 10 == 0)
	{
		Console.WriteLine($"{move} agregado");
	}
}