using System;
using System.Collections.Generic;

namespace Core
{
    public class Obstacle : IAmbientElement, IMovable
    {
        public (int x, int y) Pos { get; set; }
        public AmbientCell Owner { get; set; }

        public override string ToString()
        {
            return "X";
        }

        public bool BeMoved(params object[] param)
        {
            AmbientBoard map = (AmbientBoard)param[0];
            IDirection dir = (IDirection)param[1];
            (int x1, int x2) newpos = Position.GetNext(Pos, dir);
            if (!Position.IsInsideMap(newpos, map))
                return false;
            bool ok = false;
            if (map[newpos].IsFree)
                ok = true;
            else if (map[newpos].elementInside is IMovable)
                ok = ((IMovable)map[newpos].elementInside).BeMoved();

            if (ok)
            {
                Owner.elementInside = new FreeBox();
                Owner = map[newpos];
                this.Pos = newpos;
                Owner.elementInside = this;
            }
            return ok;
        }

        public Obstacle(int i, int j, AmbientCell owner)
        {
            Pos = (i, j);
            Owner = owner;
        }
    }
    public class Child : IAmbientElement, IMoves
    {

        public (int x, int y) Pos { get; set; }
        public AmbientCell Owner { get; set; }
        public int ID;

        public Square3x3 radius;

        public Child(int i, int j, AmbientCell owner, int ID)
        {
            Pos = (i, j);
            Owner = owner;
            this.ID = ID;
        }


        public bool Move(params object[] param)
        {
            double rand = new Random().NextDouble();
            if (rand > 0.5) return false;
            AmbientBoard map = (AmbientBoard)param[0];
            try
            {
                IDirection dir;
                (int x1, int x2) nextpos = GetNextPos(map, out dir);
                AmbientCell nextcell = map[nextpos.x1, nextpos.x2];
                if (nextcell.IsFree || (nextcell.elementInside is IMovable && ((IMovable)nextcell.elementInside).BeMoved(map, dir)))
                {
                    //param[1] = (true, this.Pos);
                    Owner.elementInside = new FreeBox();
                    Owner = nextcell;
                    Pos = nextpos;
                    Owner.elementInside = this;
                    return true;
                }
                else return false;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetRadius(IAmbient map)
        {
            radius = new Square3x3(Pos, map);
        }
        public void UpdateRadius(IAmbient map)
        {
            this.radius.SetInitAndFinish(Pos, map);
        }

        private (int, int) GetNextPos(AmbientBoard map, out IDirection dir)
        {
            List<IDirection> dirs;
            List<(int, int)> available = GetAllAvailablePos(map, out dirs);
            if (available.Count == 0)
            {
                dir = null;
                throw new Exception("No available moves");
            }
            int rand = new Random().Next(available.Count);
            dir = dirs[rand];
            return (available[rand].Item1, available[rand].Item2);
        }

        private List<(int, int)> GetAllAvailablePos(AmbientBoard map, out List<IDirection> dirs)
        {
            List<(int, int)> result = new List<(int, int)>();
            dirs = new List<IDirection>();
            foreach (IDirection dir in map.directions)
            {
                (int i, int j) temp = Position.GetNext(Pos.x, Pos.y, dir);
                if (!Position.IsInsideMap(temp, map)) continue;
                if (map[temp.i, temp.j].IsFree || map[temp.i, temp.j].IsObstacle)
                {
                    result.Add(temp);
                    dirs.Add(dir);
                }
            }
            return result;
        }


        public override string ToString()
        {
            return ID.ToString();
        }

    }

    public class Filth : IAmbientElement
    {
        public AmbientCell Owner { get; set; }

        public override string ToString()
        {
            return "*";
        }
        public Filth(AmbientCell owner)
        {
            Owner = owner;
        }
    }

    public class Playpen : IAmbientElement
    {
        public IAmbientElement child;
        public bool IsOccupied => child != null;

        public AmbientCell Owner { get; set; }

        public override string ToString()
        {
            if (IsOccupied)
                return "#&";
            return "#";
        }
        public Playpen(AmbientCell owner)
        {
            Owner = owner;
        }
    }

    public class FreeBox : IAmbientElement
    {
        public AmbientCell Owner { get; set; }

        public override string ToString()
        {
            return "";
        }

        public FreeBox()
        { }
        public FreeBox(AmbientCell owner)
        {
            Owner = owner;
        }
    }

}