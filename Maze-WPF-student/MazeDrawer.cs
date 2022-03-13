using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Maze_WPF
{
    public class MazeDrawer
    {
        private readonly Maze maze;

        private readonly int x, y;
        private int squareDrawSize = 15;
        private Canvas canvas;
        private List<Shape> mazeShapes = new List<Shape>();
        private Dictionary<string, Shape> solutionShapes = new Dictionary<string, Shape>();

        public int SquareDrawSize { get { return squareDrawSize; } }

        public MazeDrawer(Maze maze, Canvas canvas, int x, int y)
        {
            this.maze = maze;
            this.canvas = canvas;
            this.x = x;
            this.y = y;

            // hook events (for updating drawing/animation)
            maze.SolveStarted += Maze_SolveStarted;
            maze.CellStateChanged += Maze_CellStateChanged;
            maze.SolveCompleted += Maze_SolveCompleted;
        }

        public void Draw()
        {
            if (maze.CellMatrix != null)
            {
                // first time?
                if (mazeShapes.Count == 0)
                {
                    AddMazeControls();
                    AddSolutionControls();
                }
                else
                {
                    RemoveMazeControls();
                    AddMazeControls();
                    ResetSolutionControls();
                }
            }
        }

        private void RemoveMazeControls()
        {
            foreach (Shape shape in mazeShapes)
            {
                canvas.Children.Remove(shape);
            }
            mazeShapes.Clear();
        }

        private void AddMazeControls()
        {
            int nrOfRows = maze.CellMatrix.GetLength(0);
            int nrOfColumns = maze.CellMatrix.GetLength(1);

            for (int r = 0; r < nrOfRows; r++)
            {
                for (int c = 0; c < nrOfColumns; c++)
                {
                    int x0 = x + c * squareDrawSize;
                    int y0 = y + r * squareDrawSize;

                    int xStart = x0;
                    int yStart = y0;
                    int xEnd = xStart + squareDrawSize;
                    int yEnd = yStart + squareDrawSize;

                    // left side closed?
                    if ((maze.CellMatrix[r, c] & 1) > 0)
                    {
                        Line line = CreateLine(xStart, yStart, xStart, yEnd, Colors.Black);
                        canvas.Children.Add(line);
                        mazeShapes.Add(line);
                    }

                    // top side closed?
                    if ((maze.CellMatrix[r, c] & 2) > 0)
                    {
                        Line line = CreateLine(xStart, yStart, xEnd, yStart, Colors.Black);
                        canvas.Children.Add(line);
                        mazeShapes.Add(line);
                    }

                    // right side closed?
                    if ((maze.CellMatrix[r, c] & 4) > 0)
                    {
                        Line line = CreateLine(xEnd, yStart, xEnd, yEnd, Colors.Black);
                        canvas.Children.Add(line);
                        mazeShapes.Add(line);
                    }

                    // bottom side closed?
                    if ((maze.CellMatrix[r, c] & 8) > 0)
                    {
                        Line line = CreateLine(xStart, yEnd, xEnd, yEnd, Colors.Black);
                        canvas.Children.Add(line);
                        mazeShapes.Add(line);
                    }
                }
            }
        }

        private void ResetSolutionControls()
        {
            foreach (var shape in this.solutionShapes)
            {
                Ellipse ellipse = (Ellipse)shape.Value;
                ellipse.Visibility = Visibility.Hidden;
                ellipse.Stroke = new SolidColorBrush(Colors.Red);
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                ellipse.Opacity = 0.0;  // 0.0 = totally transparant
            }
        }

        private void AddSolutionControls()
        {
            int nrOfRows = maze.CellMatrix.GetLength(0);
            int nrOfColumns = maze.CellMatrix.GetLength(1);

            for (int r = 0; r < nrOfRows; r++)
            {
                for (int c = 0; c < nrOfColumns; c++)
                {
                    int x0 = x + c * squareDrawSize;
                    int y0 = y + r * squareDrawSize;

                    // add circles for path (and animations)
                    int px = x0 + squareDrawSize / 4;
                    int py = y0 + squareDrawSize / 4;
                    Ellipse e = CreateCircle(px, py, squareDrawSize / 2, squareDrawSize / 2, Colors.Red);
                    e.Name = $"dot_{r}_{c}";
                    e.Opacity = 0.0;    // 0.0 = totally transparant
                    e.Visibility = Visibility.Hidden;
                    canvas.Children.Add(e);

                    // need this for animation
                    canvas.RegisterName(e.Name, e);

                    // add to dictionary
                    this.solutionShapes.Add(e.Name, e);
                }
            }
        }

        private Line CreateLine(int x1, int y1, int x2, int y2, Color color)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(color);
            line.StrokeThickness = 1;
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            return line;
        }

        private Ellipse CreateCircle(int x, int y, int width, int height, Color color)
        {
            Ellipse circle = new Ellipse();
            circle.Stroke = new SolidColorBrush(color);
            circle.StrokeThickness = 1;
            circle.Fill = new SolidColorBrush(color);
            circle.Width = width;
            circle.Height = height;
            Canvas.SetLeft(circle, x);
            Canvas.SetTop(circle, y);
            return circle;
        }

        private void Maze_SolveStarted(object sender, SolveStartedEventArgs e)
        {
            ResetSolutionControls();
        }

        private void Maze_CellStateChanged(object sender, CellStateChangedEventArgs e)
        {
 
        }

        private void Maze_SolveCompleted(object sender, SolveCompletedEventArgs e)
        {
            if (e.SolutionFound)
            {
                DrawSolution(e.SolutionPath, e.NrOfSolutionCells);
            }
        }

        private void DrawSolution(int[] solutionPath, int nrOfSolutionCells)
        {
            int nrOfRows = maze.CellMatrix.GetLength(0);
            int nrOfColumns = maze.CellMatrix.GetLength(1);

            for (int p = 0; p < nrOfSolutionCells; p++)
            {
                int row = solutionPath[p] / nrOfColumns;
                int column = solutionPath[p] % nrOfColumns;

                string ctrlName = $"dot_{row}_{column}";
                Ellipse ellipse = (Ellipse)this.solutionShapes[ctrlName];
                ellipse.Visibility = Visibility.Visible;
                ellipse.Opacity = 1.0;
            }
        }
    }
}