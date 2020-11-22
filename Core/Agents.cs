using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Core
{
    public class Agent : IAgent, IMoves, IAmbientElement
    {
        public IAmbient Ambient { get; set; }
        public (int x, int y) Pos { get; set; }
        public AmbientCell Owner { get; set; }

        public bool Carrying { get { return Carried != null; } }

        public IAmbientElement Carried { get; set; }

        public Agent(IAmbient ambient, (int x, int y) initPos)
        {
            this.Ambient = ambient;
            this.Pos = initPos;
        }
        public virtual void DoChores()
        {
            Move();
        }

        public virtual bool Move(params object[] param)
        {
            return false;
        }

        public void Reset((int, int) newpos, IAmbient newambient)
        {
            Pos = newpos;
            Carried = null;
            Ambient = newambient;
            Owner = null;

        }
        public override string ToString()
        {
            return "@";
        }

        protected bool IsValidPosition(bool carrying, (int, int) pos)
        {
            bool inside = Position.IsInsideMap(pos, Ambient.AmbientBoard);
            bool obstacle = inside ? (Ambient.AmbientBoard[pos].IsObstacle) : false;
            if (carrying)
                return (inside && !obstacle && !Ambient.AmbientBoard[pos].HasChild);
            return inside && !obstacle && !(Ambient.AmbientBoard[pos].IsPlaypen && Ambient.AmbientBoard[pos].HasChild);
        }

        
        protected (int i, int j) GetNextPosition(bool carrying, out bool ok)
        {
            List<(int, int)> availablePos = GetAllAvailablePos(carrying);
            if (availablePos.Count == 0)
            {
                ok = false;
                return (-1, -1);
            }
            int rand = new Random().Next(availablePos.Count);
            (int i, int j) newpos = availablePos[rand];
            ok = true;
            return newpos;
        }

        protected List<(int, int)> GetAllAvailablePos(bool carrying)
        {
            List<(int, int)> result = new List<(int, int)>();
            foreach (IDirection dir in Ambient.AmbientBoard.directions)
            {
                (int x, int y) pos = Position.GetNext(Pos.x, Pos.y, dir);
                if (IsValidPosition(carrying, pos))
                    result.Add(pos);
            }
            return result;
        }
    }

    public class RandomAgent : Agent
    {
        public RandomAgent(IAmbient ambient, (int i, int j) initPos) : base(ambient, initPos) { }
        public override bool Move(params object[] param)
        {
            if (Carrying)
            {
                MoveCarrying();
            }
            else
            { 
                if(Ambient.AmbientBoard[Pos].IsFilthy && ShouldCleanFilth())
                {
                    //Ambient.AmbientBoard[Pos].elementInside = this;
                    Ambient.AmbientBoard[Pos].SetFree();
                    Ambient.UpdateFilthPercent(-1);
                    return false;
                }

                if (!Ambient.AmbientBoard[Pos].IsPlaypen && Ambient.AmbientBoard[Pos].HasChild)
                {
                    this.Carried = Ambient.AmbientBoard[Pos].elementInside;
                    Console.WriteLine("pick up child: " + this.Carried);
                    Ambient.AmbientBoard[Pos].SetFree();
                    Console.WriteLine("freed the cell: " + Ambient.AmbientBoard[Pos].elementInside);
                    MoveCarrying();                    
                }
                else MoveRandomly(); 
            }
            return true;
        }

        private void MoveRandomly()
        {
            bool moved = MoveToNewPos(false);
        }
        
        private void MoveCarrying()
        {
            for (int k = 0; k < 2; k++)
            {
                if (!MoveToNewPos(true)) return;
                if (Ambient.AmbientBoard[Pos].IsFilthy || (Ambient.AmbientBoard[Pos].IsPlaypen && ((Playpen)Ambient.AmbientBoard[Pos].elementInside).IsOccupied))
                    continue;
                if(Ambient.AmbientBoard[Pos].IsPlaypen && !Ambient.AmbientBoard[Pos].HasChild)
                {
                    ((Playpen)Ambient.AmbientBoard[Pos].elementInside).child = Carried;
                    Console.WriteLine("droped kid");
                    Carried = null;
                    Ambient.LooseKids -= 1;
                    return;
                }                           
            }            
        }

        private bool MoveToNewPos(bool carrying)
        {
            (int i, int j) newpos = GetNextPosition(carrying, out bool ok);
            if (!ok) return false;
            //if (Ambient.AmbientBoard[Pos].elementInside == this)
            //{ Ambient.AmbientBoard[Pos].SetFree(); }
            this.Pos = newpos;
            //if (Ambient.AmbientBoard[Pos].IsFree)
            //    Ambient.AmbientBoard[Pos].elementInside = this;
            return true;
        }

        

        

        

        private bool ShouldDropChild()
        {
            double rand = new Random().NextDouble();
            if (Ambient.AmbientBoard[Pos].IsPlaypen)
                return rand < 0.7;            
            return rand < 0.3;
        }

        private bool ShouldCleanFilth()
        {
            double rand = new Random().NextDouble();
            return rand < 0.5;
        }

        public override void DoChores()
        {
            Move();
        }

        public override string ToString()
        {
            if (Carrying)
                return "@& ";
            return base.ToString();
        }

       
    }

    public class EmptyAgent : Agent, IAgent, IMoves, IAmbientElement
    {
        public EmptyAgent(IAmbient ambient, (int i, int j) initPos) : base(ambient, initPos) { }

        public override bool Move(params object[] param)
        {
            return false;
        }

        //public void Reset((int, int) newpos, IAmbient newambient)
        //{
        //    Pos = newpos;
        //    Ambient = newambient;
        //}
    }

    public class PseudoRandomAgent : Agent, IAgent, IMoves, IAmbientElement
    {

        public PseudoRandomAgent(IAmbient ambient, (int i, int j) initPos) : base(ambient, initPos) { }


        public override void DoChores()
        {
            bool moved = Move();
            Console.WriteLine("agent moved: " + moved);
        }
        public override bool Move(params object[] param)
        {
            AmbientCell cell = Ambient.AmbientBoard[Pos];
            if(!Carrying)
            {
                if(cell.IsFilthy)
                {
                    Clean();
                    return false;
                }
                if(cell.HasChild && !cell.IsPlaypen)
                {
                    PickUpChild();
                    return MoveCarrying();
                    
                }
                return MoveFree();
            }
            else
            {
                return MoveCarrying();
            }

        }

        private bool MoveCarrying()
        {
            if(Ambient.AmbientBoard[Pos].elementInside == Carried)
            {
                Ambient.AmbientBoard[Pos].SetFree();
            }
            for (int i = 0; i < 2; i++)
            {
                //(int x, int y) newpos = GetNewPos(true, out bool ok);
                (int x, int y) newpos = GetNewPos(true, out bool ok);
                if (!ok) return false;
                Pos = newpos;
                if (Ambient.AmbientBoard[Pos].IsPlaypen && !Ambient.AmbientBoard[Pos].HasChild)
                {
                    DropChild();
                    return true;
                }
            }
            return true;
        }

        private bool MoveFree()
        {
            (int x, int y) newpos = GetNewPos(false, out bool ok);
            if (!ok) return false;
            Pos = newpos;
            return true;
        }

        private void Clean()
        {
            Ambient.AmbientBoard[Pos].SetFree();
            Ambient.UpdateFilthPercent(-1);
        }

        private void PickUpChild()
        {
            Carried = Ambient.AmbientBoard[Pos].elementInside;
            Console.WriteLine("picked up kid: " + Carried.ToString());
        }

        private (int, int) GetNewPos(bool carrying, out bool ok)
        {
            (int, int) temp = (-1,-1);
            ok = false;
            //bool freefound = false;
            foreach (IDirection dir in Ambient.AmbientBoard.directions)
            {
                (int i, int j) newpos = Position.GetNext(Pos, dir);
                if (!IsValidPosition(false, newpos)) continue;
                AmbientCell newcell = Ambient.AmbientBoard[newpos];
                if (!carrying)
                {
                    if (newcell.IsFilthy || (!newcell.IsPlaypen && newcell.HasChild))
                    {
                        ok = true;
                        return newpos;
                    }
                }
                else 
                {
                    if(newcell.IsPlaypen && !newcell.HasChild)
                    {
                        ok = true;
                        return newpos;
                    }
                }
            }
            temp = GetNextPosition(carrying, out ok);
            return temp;
        }

        //private (int, int) GetFollowingPos(out bool ok)
        //{
        //    ok = false;            
        //    foreach (IDirection dir in Ambient.AmbientBoard.directions)
        //    {
        //        (int i, int j) newpos = Position.GetNext(Pos, dir);
        //        if (!IsValidPosition(true, newpos)) continue;
        //        if()
        //        ok = true;
        //        return newpos;                
        //    }
        //    return Pos;
        //}

        private void DropChild()
        {
            ((Playpen)Ambient.AmbientBoard[Pos].elementInside).child = Carried;
            Carried = null;
            Ambient.LooseKids -= 1;
        }
    }


}