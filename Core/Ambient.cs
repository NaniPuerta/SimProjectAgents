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

        public Ambient(int n, int m, int filthPercent, int obstaclePercent, int childrenCount)
        {
            map = new AmbientBoard(n, m);
            SetInitialState(filthPercent, obstaclePercent, childrenCount);
        }
        public void Mutate()
        {
            throw new NotImplementedException();
        }

        public void SetInitialState(params object[] ps)
        {
            SetPlayPen((int)ps[2], map.Rows, map.Columns);
            SetInitialFilth((int)ps[0]);
            SetInitialObstacles((int)ps[1]);
            SetInitialChildren((int)ps[2]);
            SetInitialRest();
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
                        map.SetElementInside(posx + i, posy + j, new Playpen());
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
                        map[posx + i, posy + j].elementInside = new Playpen();
                        set++;
                    }
                }
            }

        }

        private void SetInitialFilth(int filthPercent)
        {
            int filthTotal = filthPercent * (map.Size) / 100;
            int filthCount = 0;
            List<Position> possibles = GetAvailablePositions(map);
            while(filthCount < filthTotal && possibles.Count > 0)
            {
                int rand = new Random().Next(0, possibles.Count);
                Position pos = possibles[rand];
                possibles.RemoveAt(rand);
                map.SetElementInside(pos, new Filth());
                filthCount++;
            }
        }

        private void SetInitialObstacles(int obstaclePercent)
        {
            int obstTotal = obstaclePercent * (map.Size) / 100;
            int obstCount = 0;
            List<Position> possibles = GetAvailablePositions(map);
            while (obstCount < obstTotal && possibles.Count > 0)
            {
                int rand = new Random().Next(0, possibles.Count);
                Position pos = possibles[rand];
                possibles.RemoveAt(rand);
                map.SetElementInside(pos, new Obstacle());
                obstCount++;
            }
        }

        private void SetInitialChildren(int childrenCount)
        {
            int children = 0;
            List<Position> possibles = GetAvailablePositions(map);
            while (children < childrenCount && possibles.Count > 0)
            {
                int rand = new Random().Next(0, possibles.Count);
                Position pos = possibles[rand];
                possibles.RemoveAt(rand);
                map.SetElementInside(pos, new Child());
                children++;
            }
        }


        private void SetInitialRest()
        {
            List<Position> rest = GetAvailablePositions(map);
            foreach (Position pos in rest)
            {
                map.SetElementInside(pos, new FreeBox());
            }
        }

        private List<Position> GetAvailablePositions(AmbientBoard bmap)
        {
            List<Position> result = new List<Position>();
            for (int i = 0; i < bmap.Rows; i++)
            {
                for (int j = 0; j < bmap.Columns; j++)
                {
                    if (bmap[i, j].elementInside == null)
                        result.Add(new Position(i, j));
                }
            }
            return result;
        }
    }


   
}
