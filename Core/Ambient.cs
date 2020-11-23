using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Core
{
    public class Ambient : IAmbient
    {
        public AmbientBoard map;
        //private List<IMoves> moveElements;
        private int filthAmmount;
        public double filthPercetage;
        public int looseKids;
        private int notPenAndNotObs;

        //initial parameters
        int Rows;
        int Columns;
        double initFilthPercentage;
        double initObstaPercentage;
        int initChildCount;

        public AmbientBoard AmbientBoard { get { return map; } }
        
        public int LooseKids { get { return looseKids; } set { this.looseKids = value; } }

        public double FilthPercentage { get { return filthPercetage; } }

        public int FilthAmmount { get { return filthAmmount; } }

        public Ambient(int n, int m, double filthPercent, double obstaclePercent, int childrenCount)
        {
            map = new AmbientBoard(n, m);
            Rows = n; Columns = m;
            initFilthPercentage = filthPercent;
            initObstaPercentage = obstaclePercent;
            initChildCount = childrenCount;
            SetInitialState(filthPercent, obstaclePercent, childrenCount);
        }
        public void Mutate()
        {
            List<Child> children = GetLooseChildren();
            children = MoveChildren(children);            
            GenerateFilth(GetSquaresToFilth(children));
            foreach (Child child in children)
            {
                child.UpdateRadius(this);
            }
            
        }        

        public void Reset()
        {
            map = new AmbientBoard(Rows, Columns);
            SetInitialState(initFilthPercentage, initObstaPercentage, initChildCount);
        }
        public void SetInitialState(params object[] ps)
        {
            SetPlayPen((int)ps[2], map.Rows, map.Columns);
            SetInitialObstacles((double)ps[1]);
            SetInitialFilth((double)ps[0]);
            SetInitialChildren((int)ps[2]);
            SetInitialRest();
            filthPercetage = (double)ps[0];
            looseKids = (int)ps[2];
        }

        private void SetPlayPen(int childrenCount, int n, int m)
        {
            double sq = Math.Sqrt(childrenCount);
            int low = (int)Math.Floor(sq);
            //int hig = (int)Math.Ceiling(sq);
            int factor = (int)Math.Ceiling(sq);
            int res = low * factor;
            int posx; int posy;
            while (res < childrenCount)
            {
                factor++;
                res += low;
            }
            int set = 0;
            if (n <= m)
            {
                posx = new Random().Next(0, n - low);
                posy = new Random().Next(0, m - factor);
                for (int i = 0; i < low; i++)
                {
                    for (int j = 0; j < factor; j++)
                    {
                        if (set == childrenCount) return;
                        (int i, int j) pos = (posx + i, posy + j);
                        map.SetElementInside(pos.i, pos.j, new Playpen(map[pos]));
                        set++;
                    }
                }
            }
            if (n > m)
            {
                posx = new Random().Next(0, n - factor);
                posy = new Random().Next(0, m - low);
                for (int i = 0; i < factor; i++)
                {
                    for (int j = 0; j < low; j++)
                    {
                        if (set == childrenCount) return;
                        (int i, int j) pos = (posx + i, posy + j);
                        map[pos.i, pos.j].elementInside = new Playpen(map[pos]);
                        set++;
                    }
                }
            }

        }

        private void SetInitialFilth(double filthPercent)
        {
            List<(int, int)> possibles = GetAvailablePositions(map);
            notPenAndNotObs = possibles.Count;
            int filthTotal = (int)(filthPercent * (possibles.Count) / 100);
            int filthCount = 0;
            while(filthCount < filthTotal && possibles.Count > 0)
            {
                int rand = new Random().Next(0, possibles.Count);
                (int i, int j) pos = possibles[rand];
                possibles.RemoveAt(rand);
                map.SetElementInside(pos, new Filth(map[pos]));
                filthCount++;
            }
            filthAmmount = filthTotal;
        }

        private void SetInitialObstacles(double obstaclePercent)
        {
            List<(int, int)> possibles = GetAvailablePositions(map);
            int obstTotal = (int)(obstaclePercent * (possibles.Count) / 100);
            int obstCount = 0;
            while (obstCount < obstTotal && possibles.Count > 0)
            {
                int rand = new Random().Next(0, possibles.Count);
                (int i, int j) pos = possibles[rand];
                possibles.RemoveAt(rand);
                map.SetElementInside(pos, new Obstacle(pos.i, pos.j, map[pos.i, pos.j]));
                obstCount++;
            }
        }

        private void SetInitialChildren(int childrenCount)
        {
            int children = 0;
            List<(int,int)> possibles = GetAvailablePositions(map);
            while (children < childrenCount && possibles.Count > 0)
            {
                int rand = new Random().Next(0, possibles.Count);
                (int i,int j) pos = possibles[rand];
                possibles.RemoveAt(rand);
                map.SetElementInside(pos, new Child(pos.i, pos.j, map[pos], children + 1));
                ((Child)map[pos].elementInside).SetRadius(this);
                children++;
            }
        }


        private void SetInitialRest()
        {
            List<(int, int)> rest = GetAvailablePositions(map);
            foreach ((int, int) pos in rest)
            {
                map.SetElementInside(pos, new FreeBox());
            }
        }

        private List<(int, int)> GetAvailablePositions(AmbientBoard bmap)
        {
            List<(int, int)> result = new List<(int,int)>();
            for (int i = 0; i < bmap.Rows; i++)
            {
                for (int j = 0; j < bmap.Columns; j++)
                {
                    if (bmap[i, j].elementInside == null)
                        result.Add((i,j));
                }
            }
            return result;
        }

        private List<Child> MoveChildren(List<Child> children) 
        {
            foreach (Child child in children)
                child.radius.CheckChildrenInside(this);
            List<Child> movedChildren = new List<Child>();
            foreach (Child child in children)
            {
                if (child.Move(map))
                {
                    movedChildren.Add(child);
                }
            }
            return movedChildren;
        }

        private List<Child> GetLooseChildren()
        {
            List<Child> result = new List<Child>();
            foreach(AmbientCell cell in map)
            {
                if (!cell.IsPlaypen && cell.HasChild)
                    result.Add((Child)cell.elementInside);
            }
            return result;
        }
        
        private List<Square3x3> GetSquaresToFilth(List<Child> children)
        {
            if (children.Count == 0) return null;
            List<Square3x3> temp = new List<Square3x3>();
            List<Square3x3> result = new List<Square3x3>();
            List<Child> already = new List<Child>();
            foreach (Child kid in children)
            {
                Child max = kid;
                foreach (Child mate in kid.radius.childrenInside)
                {
                    if (mate.radius.childrenCount > max.radius.childrenCount)
                        max = mate;
                }
                temp.Add(max.radius);
                //kid.radius = max.radius;
            }
            foreach (Square3x3 sq in temp)
            {
                Square3x3 tempsq = Square3x3.Copy(sq);
                tempsq.childrenCount = 0;
                for (int i = 0; i < sq.childrenInside.Count; i++)
                {
                    if(!already.Contains(sq.childrenInside[i]))
                    {
                        already.Add(sq.childrenInside[i]);
                        tempsq.AddChildren(sq.childrenInside[i]);
                    }
                }
                if (tempsq.childrenCount > 0)
                {
                    tempsq.SetCellsToFilthCount();
                    result.Add(tempsq); 
                }
            }
            return result;
        }

        private void GenerateFilth(List<Square3x3> squares) 
        {
            if (squares == null) return;
            int filthadded = 0;
            foreach (Square3x3 sq in squares)
            {
                List<AmbientCell> tofilth = Square3x3.GetCellsToFilth(sq, this);
                if (tofilth == null) continue;
                foreach (AmbientCell cell in tofilth)
                {
                    cell.elementInside = new Filth(cell);
                    filthadded++;
                }
            }
            UpdateFilth(filthadded);
        }

        public void UpdateFilth(int value)
        {
            filthAmmount += value;
            filthPercetage = filthAmmount * 100 / notPenAndNotObs;
        }
               
        public List<(int, int)> GetFreePositions()
        {
            List<(int, int)> result = new List<(int, int)>();
            for (int i = 0; i < map.Rows; i++)
            {
                for (int j = 0; j < map.Columns; j++)
                {
                    if (map[i, j].IsFree) result.Add((i, j));
                }
            }
            if(result.Count == 0)
            {
                for (int i = 0; i < map.Rows; i++)
                {
                    for (int j = 0; j < map.Columns; j++)
                    {
                        if (!map[i, j].IsObstacle) result.Add((i, j));
                    }
                }
            }
            if (result.Count == 0)
                throw new Exception("Wait...Whaaaat?...The map is a solid block");
            return result;
        }

        public void PrintMap()
        {
            Console.WriteLine(map);
        }
    }
       
}
