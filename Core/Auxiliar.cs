namespace Core
{
    #region AuxiliaryClasses
    public class AmbientBoard
    {
        private AmbientCell[,] map;
        public int Rows { get; private set; }

        public int Columns { get; private set; }

        public int Size { get; private set; }

        public AmbientBoard(int N, int M)
        {
            map = new AmbientCell[N, M];
            this.Rows = N;
            this.Columns = M;
            this.Size = N * M;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    map[i, j] = new AmbientCell();
                }
            }
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    result += map[i, j].ToString() + " ";
                }
                result += "\n";
            }
            return result;
        }

        public IAmbientElement GetElementInside(int i, int j)
        {
            return map[i, j].elementInside;
        }

        public void SetElementInside(int i, int j, IAmbientElement elem)
        {
            map[i, j].elementInside = elem;
        }

        public IAmbientElement GetElementInside(Position pos)
        {
            return map[pos.i, pos.j].elementInside;
        }

        public void SetElementInside(Position pos, IAmbientElement elem)
        {
            map[pos.i, pos.j].elementInside = elem;
        }
        public AmbientCell this[int i, int j]
        {
            get { return map[i, j]; }
            set { map[i, j] = value; }
        }

        public AmbientCell this[Position pos]
        {
            get { return map[pos.i, pos.j]; }
            set { map[pos.i, pos.j] = value; }
        }
    }

    public class AmbientCell
    {
        public IAmbientElement elementInside;
        public bool IsFree => elementInside == null;

        public AmbientCell()
        {
        }

        public AmbientCell(IAmbientElement elem)
        {
            this.elementInside = elem;
        }

        public override string ToString()
        {
            return elementInside.ToString();
        }
    }
    public class Obstacle : IAmbientElement, IMovable
    {
        public override string ToString()
        {
            return "Obst";
        }
    }
    public class Child : IAmbientElement, IMoves
    {
        public override string ToString()
        {
            return "Chil";
        }
    }

    public class Filth : IAmbientElement
    {
        public override string ToString()
        {
            return "Filt";
        }
    }

    public class Playpen : IAmbientElement
    {
        public IAmbientElement child;
        public bool IsOccupied => child != null;

        public override string ToString()
        {
            return "PPen";
        }
    }

    public class FreeBox : IAmbientElement
    {
        public override string ToString()
        {
            return "Free";
        }
    }

    #endregion

    #region AuxiliaryStructs

    public struct Position
    {
        public int i { get; set; }
        public int j { get; set; }

        public Position(int x, int y)
        { this.i = x; this.j = y; }

        public Position GetNext(IDirection dir)
        { return new Position(this.i + dir.x, this.j + dir.y); }
    }

    //public readonly struct Direction
    //{
    //    public readonly int x;
    //    public readonly int y;
    //    //public (int x, int y) Up => (-1, 0);
    //    //public (int x, int y) Down => (1, 0);
    //    //public (int x, int y) Left => (0, -1);
    //    //public (int x, int y) Right => (0, 1);
    //    //public (int x, int y) UpLeft => (-1, -1);
    //    //public (int x, int y) UpRight => (-1, 1);
    //    //public (int x, int y) DownLeft => (1, -1);
    //    //public (int x, int y) DownRight => (1, 1);
    //}

    public readonly struct Up : IDirection
    {
        public int x => -1;

        public int y => 0;
    }
    public readonly struct Down : IDirection
    {
        public int x => 1;

        public int y => 0;
    }
    public readonly struct Left : IDirection
    {
        public int x => 0;

        public int y => -1;
    }
    public readonly struct Right : IDirection
    {
        public int x => 0;

        public int y => 1;
    }
    public readonly struct UpLeft : IDirection
    {
        public int x => -1;

        public int y => -1;
    }
    public readonly struct UpRight : IDirection
    {
        public int x => -1;

        public int y => 1;
    }
    public readonly struct DownLeft : IDirection
    {
        public int x => 1;

        public int y => -1;
    }
    public readonly struct DownRight : IDirection
    {
        public int x => 1;

        public int y => 1;
    }
    #endregion
}