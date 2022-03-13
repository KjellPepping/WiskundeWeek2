using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Maze_WPF
{
    public class SolveStartedEventArgs : EventArgs
    {
        // ...
    }

    public class CellStateChangedEventArgs : EventArgs
    {
        // ...
    }

    public class SolveCompletedEventArgs : EventArgs
    {
        public bool SolutionFound { get; set; }
        public int[] SolutionPath { get; set; }
        public int NrOfSolutionCells { get; set; }
    }

    public class Maze
    {
        private int[,] cellMatrix;
        private int nrOfRows;
        private int nrOfColumns;

        // solution fields
        private int startRow, startColumn;
        private bool[] cellsInvestigated;
        private int[] solutionPath;
        private int nrOfSolutionCells = 0;
        private bool solutionFound = false;

        private BackgroundWorker myBackgroundWorker;
        private int workerSleep;

        // events
        public event EventHandler<SolveStartedEventArgs> SolveStarted;
        public event EventHandler<CellStateChangedEventArgs> CellStateChanged;
        public event EventHandler<SolveCompletedEventArgs> SolveCompleted;

        public int[,] CellMatrix
        {
            set
            {
                cellMatrix = value;
                if (cellMatrix != null)
                {
                    nrOfRows = cellMatrix.GetLength(0);
                    nrOfColumns = cellMatrix.GetLength(1);
                }
            }
            get
            {
                return cellMatrix;
            }
        }

        public int AnimationSpeed
        {
            set { this.workerSleep = 100 - value; }
        }

        public Maze()
        {
            //
        }


        private void InitSolution()
        {
            cellsInvestigated = new bool[nrOfColumns * nrOfRows];
            for (int c = 0; c < nrOfColumns * nrOfRows; c++)
                cellsInvestigated[c] = false;

            solutionPath = new int[nrOfColumns * nrOfRows];
            nrOfSolutionCells = 0;

            solutionFound = false;
        }

        #region Solve methods
        public void Solve(int startRow, int startColumn)
        {
            this.startRow = startRow;
            this.startColumn = startColumn;

            OnSolveStarted(new SolveStartedEventArgs());

            // use a background worker
            myBackgroundWorker = new BackgroundWorker();
            myBackgroundWorker.WorkerReportsProgress = true;
            myBackgroundWorker.DoWork += worker_DoWork;
            myBackgroundWorker.ProgressChanged += worker_ProgressChanged;
            myBackgroundWorker.RunWorkerCompleted += worker_RunWorkerCompleted;
            myBackgroundWorker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            InitSolution();

            if (cellMatrix != null)
            {
                Solve(0, startRow, startColumn);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                OnCellStateChanged(new CellStateChangedEventArgs());
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SolveCompletedEventArgs args = new SolveCompletedEventArgs();
            args.SolutionFound = this.solutionFound;
            args.SolutionPath = this.solutionPath;
            args.NrOfSolutionCells = this.nrOfSolutionCells;
            OnSolveCompleted(args);
        }

        protected virtual void OnSolveStarted(SolveStartedEventArgs e)
        {
            EventHandler<SolveStartedEventArgs> handler = SolveStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCellStateChanged(CellStateChangedEventArgs e)
        {
            EventHandler<CellStateChangedEventArgs> handler = CellStateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSolveCompleted(SolveCompletedEventArgs e)
        {
            EventHandler<SolveCompletedEventArgs> handler = SolveCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Solve(int level, int row, int column)
        {
            if(!solutionFound)
            {
                int nextRow = row;
                int nextColumn = column;
                int currentCellValue = cellMatrix[row, column];

                try
                {
                    if ((currentCellValue & 4) == 0 && !cellsInvestigated[(row * nrOfColumns) + (column + 1)])
                        nextColumn += 1;
                    else if ((currentCellValue & 8) == 0 && !cellsInvestigated[((row + 1) * nrOfColumns) + column])
                        nextRow += 1;
                    else if ((currentCellValue & 1) == 0 && !cellsInvestigated[(row * nrOfColumns) + (column - 1)])
                        nextColumn -= 1;
                    else if ((currentCellValue & 2) == 0 && !cellsInvestigated[((row - 1) * nrOfColumns) + column])
                        nextRow -= 1;
                    else
                        callBack(row, column);
                }
                catch (IndexOutOfRangeException e)
                {
                    Trace.WriteLine("The method is trying to access a cell outside of the maze");
                    if(row==19&&column==39)
                    {
                        if (row - startRow > 2 || column - startColumn > 2)
                            solutionFound = true;
                        else
                            nextRow -= 1;
                    }
                    else if(row==0&&column==0)
                    {
                        if (row + startRow > 2 || column + startColumn > 2)
                            solutionFound = true;
                        else
                            nextRow += 1;
                    }
                }

                cellsInvestigated.SetValue(true, (row * nrOfColumns) + column);

                solutionPath.SetValue((row * nrOfColumns) + column, nrOfSolutionCells);
                if (level <= 0)
                    nrOfSolutionCells++;

                Solve(0, nextRow, nextColumn);
            }
            
        }

        private void callBack(int row, int column)
        {
            //Previous cell has to be marked as exit
            cellsInvestigated.SetValue(false, solutionPath[nrOfSolutionCells - 1]);

            //Route needs to be cleared
            solutionPath.SetValue(0, nrOfSolutionCells - 1);

            //Amount of steps in route needs to be decremented
            nrOfSolutionCells -= 1;

            Solve(1, row, column);
        }
        #endregion
    }
}