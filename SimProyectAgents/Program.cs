using System;
using System.Runtime.InteropServices;
using Core;

namespace SimProyectAgents
{
    class Program
    {
        static IAgent agent;
        static void Main(string[] args)
        {
         //   Ambient amb = new Ambient(4,3, 10, 10, 2);
            Ambient amb = new Ambient(5,7, 10, 10, 6);
            //amb.AmbientBoard[0, 0].SetFree();
            //agent = new RandomAgent(amb, (0, 0));
            //amb.AmbientBoard[(0, 0)].elementInside = (IAmbientElement)agent;

            //Console.WriteLine(amb.map[(0, 0)]);
            //Console.WriteLine(amb.map[0, 0].HasRobot);

            (int, int) apos = Simulation.GetRandomInitPosition(amb);
            //agent = new RandomAgent(amb, apos);
            //agent = new EmptyAgent();
            agent = new PseudoRandomAgent(amb, apos);
            agent.Pos = apos;
            Simulation sim = new Simulation(100, amb, agent);
            
            sim.Run();
            sim.PrintStats();
            Console.ReadLine();
        }

    }
}
