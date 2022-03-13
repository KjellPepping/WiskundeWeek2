using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Maze_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CanvasXoffset = 10;
        private const int CanvasYoffset = 10;

        private Maze maze;
        private MazeDrawer mazeDrawer;
        private Button currentButton = null;

        public MainWindow()
        {
            InitializeComponent();

            maze = new Maze();
            maze.AnimationSpeed = (int)sliderSpeed.Value;
            mazeDrawer = new MazeDrawer(maze, canvas, CanvasXoffset, CanvasYoffset);

            maze.SolveCompleted += Maze_SolveCompleted;
        }

        private int[,] ReadMatrixToFile(string filename)
        {
            if (!File.Exists(filename))
                return null;

            StreamReader reader = new StreamReader(filename);

            int nrOfRows = int.Parse(reader.ReadLine());
            int nrOfColumns = int.Parse(reader.ReadLine());

            int[,] matrix = new int[nrOfRows, nrOfColumns];

            for (int row = 0; row < nrOfRows; row++)
            {
                string line = reader.ReadLine().Trim().Replace("  ", " ");
                string[] fields = line.Split(' ');

                for (int col = 0; col < nrOfColumns; col++)
                {
                    matrix[row, col] = int.Parse(fields[col]);
                }
            }

            reader.Close();

            return matrix;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(canvas);
            StartRow.Text = "" + (int)((point.Y - CanvasYoffset) / mazeDrawer.SquareDrawSize);
            StartColumn.Text = "" + (int)((point.X - CanvasXoffset) / mazeDrawer.SquareDrawSize);
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.maze != null)
            {
                this.maze.AnimationSpeed = (int)sliderSpeed.Value;
            }
        }

        private void btnMaze_Click(object sender, RoutedEventArgs e)
        {
            if (currentButton != null)
            {
                currentButton.FontWeight = FontWeights.Normal;
            }
            Button btnMaze = (Button)sender;
            btnMaze.FontWeight = FontWeights.Bold;
            currentButton = btnMaze;

            int mazeNr = int.Parse(btnMaze.Tag.ToString());
            maze.CellMatrix = ReadMatrixToFile($"maze{mazeNr}.txt");
            mazeDrawer.Draw();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            EnableMazeButtons(false);

            int startRow = 0;
            int startColumn = 0;
            int.TryParse(StartRow.Text, out startRow);
            int.TryParse(StartColumn.Text, out startColumn);
            maze.Solve(startRow, startColumn);
        }

        private void Maze_SolveCompleted(object sender, System.EventArgs e)
        {
            EnableMazeButtons(true);
        }

        private void EnableMazeButtons(bool enabled)
        {
            btnMaze1.IsEnabled = enabled;
            btnMaze2.IsEnabled = enabled;
            btnMaze3.IsEnabled = enabled;
            btnMaze4.IsEnabled = enabled;
            btnMaze5.IsEnabled = enabled;
            btnSolve.IsEnabled = enabled;
        }
    }
}