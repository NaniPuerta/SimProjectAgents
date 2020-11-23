using System;
using System.Collections.Generic;

namespace Core
{
    public interface IAmbientElement
    {
        public AmbientCell Owner { get; set; }
    }

    public interface IAmbient
    {
        public AmbientBoard AmbientBoard { get; }

        public int LooseKids { get; set; }

        public double FilthPercentage { get; }

        public int FilthAmmount { get; }
        public void Mutate();

        public void Reset();
        public void SetInitialState(params object[] ps);

        public void UpdateFilth(int value);

        public List<(int, int)> GetFreePositions();

        public void PrintMap();
    }

    public interface IAgent : IMoves
    {
        public IAmbient Ambient { get; set; }

        public IAmbientElement Carried { get; set; }
        public bool Carrying { get; }
       
        public void DoChores();

        public void Reset((int, int) newpos, IAmbient newambient);
        public void SetAgent(IAmbient ambient, (int x, int y) initPos);
    }


    public interface IMovable
    {
        public bool BeMoved(params object[] param);
    }

    public interface IMoves
    {
        public (int x, int y) Pos { get; set; }
        public bool Move(params object[] param);
    }

    

    public interface IDirection
    {
        public int x { get; }
        public int y { get; }
    }
}