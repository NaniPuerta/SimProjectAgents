using System;
using Core;

namespace SimProyectAgents
{
    class Program
    {
        static void Main(string[] args)
        {
            Ambient amb = new Ambient(7, 9, 10, 10, 6);
            Console.WriteLine(amb.map);
        }
    }
}
