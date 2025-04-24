using ChessLogic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly Image[] pieceImages = new Image[64];
		private int? selectedPieceIndex = null;

		public MainWindow()
		{
			InitializeComponent();
			InitializeBoard();
			DrawBoard();

			//subscribirse al evento del Board para mandar mensajes
			Board.OnInfoMessage += (mensaje) =>
			{
				MessageBox.Show(mensaje);
			};
		}

		private void InitializeBoard()
		{
			//creo las imagener para cada espacio del tablero
			for (int i = 0; i < 64; i++)
			{
				//creo un contenedor que recibira el click
				Border border = new Border
				{
					Background = Brushes.Transparent,  // hacer el fondo transparente para que reciba el click
					Tag = i  //el indice en el contenedor
				};

				//creo una imagen
				Image img = new Image();

				//le asigno el evento de click y el tag con la posicion de la pieza
				border.MouseLeftButtonDown += OnPieceClick;
				//img.Tag = i;
				border.Child = img;


				//agrego la imagen al arreglo
				pieceImages[i] = img;
				//agrego la imagen al grid
				PieceGrid.Children.Add(border);
			}
		}

		private void DrawBoard()
		{
			for (int i = 0; i < 64; i++)
			{
				if(Board.squares[i] == 0)
				{
					pieceImages[i].Source = null;
					continue;
				}
				else
				{
					Image img = pieceImages[i];
					img.Source = Images.GetImage(Board.squares[i]);
				}
			}
		}

		private void OnPieceClick(object sender, MouseButtonEventArgs e)
		{
			if (sender is Border borde && borde.Tag is int index)
			{

				if (selectedPieceIndex != null)
				{
					ClearPosibleMoves();
					//en caso de seleccionar la misma pieza, la deselecciono
					if (selectedPieceIndex == index)
					{
						selectedPieceIndex = null;
						return;
					}

					//se seleccionó un espacio, muevo la pieza y limpio la selección
					try
					{
						Board.HandlePieceMovement(selectedPieceIndex.Value, index);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}

					selectedPieceIndex = null;
					DrawBoard();
				}
				else
				{
					if (Board.squares[index] != 0) { 
						selectedPieceIndex = index;

						//colorear todos los cuadros del tablero excepto el seleccionado
						HighlightPossibleMoves(index);
					}
				}

			}
		}

		private void HighlightPossibleMoves(int index)
		{
			//obtener arreglo de movimientos posibles
			int[] moves = new int[0];
			moves = Board.GetPosiblePieceMovements(index, null, true);

			if (moves.Length == 0) return;

			//obtengo la pieza seleccionada
			int pieceCode = Board.squares[index];
			int pieceColor = pieceCode & Piece.ColorMask;


			//TODO: hacer mas eficiente, quizas recorriendo el arreglo de movimientos en vez de la grilla
			foreach (var item in PieceGrid.Children)
			{
				if (item is Border border)
				{
					if (Array.IndexOf(moves, border.Tag) != -1) //si el tag se encuentra dentro de los movimientos
					{
						if (GameState.getState() == pieceColor)
						{
							//Es una pieza del trno actual, pinto los posibles movimientos con un verde semitransparente
							border.Background = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
						}
						else
						{
							//si no es del turno actual, pinto con un naranja semitransparente
							border.Background = new SolidColorBrush(Color.FromArgb(100, 255, 165, 0));
						}
					}
				}
			}
		}

		private void ClearPosibleMoves()
		{
			foreach (var item in PieceGrid.Children)
			{
				if (item is Border border)
				{
					border.Background = Brushes.Transparent;
				}
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			//desuscribirse del evento
			Board.OnInfoMessage -= (mensaje) =>
			{
				MessageBox.Show(mensaje);
			};
		}
	}
}