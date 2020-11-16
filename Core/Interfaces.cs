using System;

namespace Core
{
    public interface IAmbientElement
    {

    }

    public interface IAmbient
    {
        public void Mutate();

        public void SetInitialState(params object[] ps);
    }

    public interface IAgent
    {
        
    }

    public interface IAmbientBox
    {

    }

    public interface IMovable
    { }

    public interface IMoves
    { }

    public interface IDirection
    {
        public int x { get; }
        public int y { get; }
    }
}