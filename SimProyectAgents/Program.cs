using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Core;

namespace SimProyectAgents
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IAgent> agents = new List<IAgent>();

            agents.Add(new PseudoRandomAgent());
            agents.Add(new BFSAgent());
            agents.Add(new ChildFirstAgent());
            agents.Add(new CleanFirstAgent());
            foreach (IAgent agent in agents)
            {
                Directory.CreateDirectory(agent.GetType().Name);
                Console.WriteLine("new agent");
                Simulate(agent);

            }

        }

        private static void Simulate(IAgent simAgent)
        {
            string pathName = DateTime.Now.ToString();
            pathName = pathName.Replace('/', '-');
            pathName = pathName.Replace(':', '.');
            string folderName = simAgent.GetType().Name;
            string totalPath = folderName + "\\" + pathName;


            Ambient[] ambients = new Ambient[10];
            for (int i = 0; i < ambients.Length; i++)
            {
                int N = InitDim();
                int M = InitDim();
                ambients[i] = new Ambient(N, M, InitFilP(), InitObsP(), InitChiC(N * M));
            }
            int[] robotFired = new int[ambients.Length];
            int[] gotKids = new int[ambients.Length];
            double[] filths = new double[ambients.Length];
            int index = 0;
            foreach (Ambient amb in ambients)
            {
                Console.WriteLine("New Ambient\n");
                List<double> filthPercentages = new List<double>();
                int timesRobotFired = 0;
                int timesGotAllKids = 0;
                double meanFilth;

                for (int i = 0; i < 30; i++)
                {
                    simAgent.SetAgent(amb, Simulation.GetRandomInitPosition(amb));
                    Simulation simulation = new Simulation(InitT(), amb, simAgent);                    
                    simulation.Run();
                    if (simulation.robotFired)
                        timesRobotFired++;
                    if (simulation.gotAllKids)
                        timesGotAllKids++;
                    filthPercentages.Add(simulation.finalFilthPercent);
                    amb.Reset();
                }
                meanFilth = filthPercentages.Sum() / filthPercentages.Count;
                robotFired[index] = timesRobotFired;
                gotKids[index] = timesGotAllKids;
                filths[index] = meanFilth;
                index++;
            }
            string filecontent = "";
            for (int i = 0; i < ambients.Length; i++)
            {                
                string content = "Ambiente " + i + " :\n" + "      Suciedad media: " + filths[i] + " porciento\n" + "      Robot despedido: " + robotFired[i] + " veces\n" + "      Robot terminó bien: " + gotKids[i] + " veces\n";
                Console.WriteLine(content);
                filecontent += content;
            }
            totalPath += ".txt";
            File.WriteAllText(totalPath, filecontent);
        }

        public static int InitDim()
        { 
            return new Random().Next(4, 10); 
        }
        public static double InitObsP()
        {
            return (double)(new Random().Next(100, 301)) / 10;
        }

        public static double InitFilP()
        {
            return (double)(new Random().Next(100, 600)) / 10;
        }
        public static int InitChiC(int mapSize)
        {
            return new Random().Next(1, (mapSize * 20 / 100));
        }

        public static int InitT()
        { return new Random().Next(20, 51); }

    }
}
