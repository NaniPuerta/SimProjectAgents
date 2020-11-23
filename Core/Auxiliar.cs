using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

namespace Core
{
    #region AuxiliaryClasses
    public class AmbientBoard : IEnumerable
    {
        private AmbientCell[,] map;
        public int Rows { get; private set; }

        public int Columns { get; private set; }

        public int Size { get; private set; }

        public IDirection[] directions = { new Up(), new Down(), new Left(), new Right() };

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
                result += "-";
                for (int k = 0; k < Columns; k++)
                {
                    result += "----";
                }
                result += "\n";
                result += "|";
                for (int j = 0; j < Columns; j++)
                {
                    result += map[i, j].ToString() + "|";
                }
                result += "\n";
            }
            result += "-";
            for (int k = 0; k < Columns; k++)
            {
                result += "----";
            }
            result += "\n";
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

        public IAmbientElement GetElementInside((int i, int j) pos)
        {
            return map[pos.i, pos.j].elementInside;
        }

        public void SetElementInside((int i, int j) pos, IAmbientElement elem)
        {
            map[pos.i, pos.j].elementInside = elem;
        }

        public IEnumerator GetEnumerator()
        {
            return map.GetEnumerator();
        }

        public AmbientCell this[int i, int j]
        {
            get { return map[i, j]; }
            set { map[i, j] = value; }
        }

        public AmbientCell this[(int i, int j) pos]
        {
            get { return map[pos.i, pos.j]; }
            set { map[pos.i, pos.j] = value; }
        }
    }


    public class AmbientCell
    {
        public IAmbientElement elementInside;
        public bool IsFree => (elementInside.GetType() == typeof(FreeBox));

        public bool IsObstacle => (elementInside.GetType() == typeof(Obstacle));

        public bool IsFilthy => (elementInside.GetType() == typeof(Filth));

        public bool IsPlaypen => (elementInside.GetType() == typeof(Playpen));

        public bool HasChild => (elementInside.GetType() == typeof(Child) || (this.IsPlaypen && ((Playpen)elementInside).IsOccupied));

        public bool HasRobot => (elementInside is IAgent);
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

        public void SetFree()
        {
            this.elementInside = new FreeBox();
        }
    }
    
    public class Square3x3
    {        
        (int i, int j) initCell;
        (int i, int j) finishCell;
        int cellsToFilth;
        public List<Child> childrenInside;
        public int childrenCount;
        public List<AmbientCell> freeCells;

        public Square3x3() { }
        public Square3x3((int i, int j) center, IAmbient map)
        {
            SetInitAndFinish(center, map);
        }

        public void SetInitAndFinish((int i, int j) center, IAmbient map)
        {
            SetInit(center, map);
            SetFinish(center, map);
        }

        private void SetFinish((int i, int j) center, IAmbient map)
        {
            for (int k = center.i + 1; k >= center.i; k--)
            {
                for (int l = center.j + 1; l >= center.j; l--)
                {
                    if (!Position.IsInsideMap(k, l, map.AmbientBoard))
                        continue;
                    else
                    {
                        finishCell = (k, l);
                        return;
                    }
                }
            }
        }

        private void SetInit((int i, int j) center, IAmbient map)
        {
            for (int k = center.i - 1; k < center.i + 1; k++)
            {
                for (int l = center.j - 1; l < center.j + 1; l++)
                {
                    if (!Position.IsInsideMap(k, l, map.AmbientBoard))
                        continue;
                    else
                    {
                        initCell = (k, l);
                        return;
                    }
                }
            }
        }

        public void CheckChildrenInside(IAmbient map)
        {
            childrenInside = new List<Child>();
            for (int k = initCell.i; k < finishCell.i+1; k++)
            {
                for (int l = initCell.j; l < finishCell.j+1; l++)
                {
                    if (!map.AmbientBoard[k,l].IsPlaypen && map.AmbientBoard[k, l].HasChild)
                        childrenInside.Add((Child)map.AmbientBoard[k, l].elementInside);
                }
            }
            SetCellsToFilthCount();
        }

        public void SetCellsToFilthCount()
        {
            cellsToFilth = childrenInside.Count switch
            {
                0 => 0,
                1 => 1,
                2 => 3,
                _ => 6,
            };
            childrenCount = childrenInside.Count;
        }

        public static List<AmbientCell> GetCellsToFilth(Square3x3 square, IAmbient map)
        {
            int rand = new Random().Next(square.cellsToFilth);
            square.freeCells = square.GetFreeCells(map);
            if (rand == 0 || square.freeCells.Count == 0) return null;
            if (square.freeCells.Count <= rand) return square.freeCells;
            List<AmbientCell> result = new List<AmbientCell>();
            while(result.Count < rand)
            {
                int rand2 = new Random().Next(square.freeCells.Count);
                result.Add(square.freeCells[rand2]);
                square.freeCells.RemoveAt(rand2);
            }
            return result;
        }

        private List<AmbientCell> GetFreeCells(IAmbient map)
        {
            List<AmbientCell> result = new List<AmbientCell>();
            for (int k = initCell.i; k < finishCell.i; k++)
            {
                for (int l = initCell.j; l < finishCell.j; l++)
                {
                    if (map.AmbientBoard[k, l].IsFree)
                        result.Add(map.AmbientBoard[k, l]);
                }
            }
            return result;
        }
        public static Square3x3 Copy(Square3x3 sq)
        {
            Square3x3 result = new Square3x3();
            result.initCell = sq.initCell;
            result.finishCell = sq.finishCell;
            result.childrenCount = 0;
            result.childrenInside = new List<Child>();
            result.freeCells = sq.freeCells;
            return result;
        }

        public void AddChildren(Child child)
        {
            childrenInside.Add(child);
            childrenCount++;
        }
        public void RemoveChild(Child child)
        {
            childrenInside.Remove(child);
            childrenCount--;
        }
    }

    public static class Position
    {
        public static (int, int) GetNext(int i, int j, IDirection dir)
        { return (i + dir.x, j + dir.y); }

        public static (int, int) GetNext((int i, int j) pos, IDirection dir)
        {
            return (pos.i + dir.x, pos.j + dir.y);
        }

        public static bool IsInsideMap((int i, int j) pos, AmbientBoard map)
        {
            return (pos.i >= 0 && pos.j >= 0 && pos.i < map.Rows && pos.j < map.Columns);
        }

        public static bool IsInsideMap(int i, int j, AmbientBoard map)
        {
            return (i >= 0 && j >= 0 && i < map.Rows && j < map.Columns);
        }
    }

    #endregion

    #region AuxiliaryStructs  

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