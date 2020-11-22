using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class Simulation
    {
       
        IAgent agent;
        IAmbient ambient;
        public double finalFilthPercent;
        public bool robotFired;
        public bool gotAllKids;
        int timepassed;
        List<double> filthPercents;

        //initial parameters
        int timelapse;

        public Simulation(int t, int N, int M, int filthP, int obstP, int childC)
        {
            timelapse = t;
            ambient = new Ambient(N, M, filthP, obstP, childC);
            agent = new RandomAgent(ambient, GetRandomInitPosition(ambient));
        }

        public Simulation(int time, IAmbient ambient, IAgent agent)
        {
            timelapse = time;
            this.ambient = ambient;
            this.agent = agent;
            
        }

        private void Reset()
        {
            ambient.Reset();
            agent.Reset(GetRandomInitPosition(ambient), ambient); 
        }
        public void Run()
        {
            int time = 0;
            timepassed = 0;
            filthPercents = new List<double>();
            PrintMap();
            //Console.ReadLine();
            while(true)
            {
                if (timepassed == 100)
                {
                    finalFilthPercent = filthPercents.Sum() / filthPercents.Count;
                    return;
                }
                if (time == timelapse)
                {
                    time = 0;
                    filthPercents.Add(ambient.FilthPercentage);
                    Reset();
                    timepassed++;
                    Console.WriteLine("Ambient Reset");
                }                
                if (ambient.FilthPercentage >= 60)
                {
                    //finalFilthPercent = ambient.FilthPercentage;
                    filthPercents.Add(ambient.FilthPercentage);
                    finalFilthPercent = filthPercents.Sum() / filthPercents.Count;
                    robotFired = true;
                    return;
                }
                if (ambient.LooseKids == 0 && ambient.FilthAmmount == 0)
                {
                    //finalFilthPercent = ambient.FilthPercentage;
                    finalFilthPercent = filthPercents.Sum() / filthPercents.Count;
                    gotAllKids = true;
                    return;
                }
                agent.DoChores();
                Console.WriteLine("Agent at: " + agent.Pos);
                //PrintMap();
                ambient.Mutate();
                Console.WriteLine("Ambient mutated");
                PrintMap();
                //PrintInsideStats();
                //Console.ReadLine();
                time++;
            }
            
        }

        private void PrintInsideStats()
        { 
            Console.WriteLine("Current Stats: " + "\n    filth percentage: " + ambient.FilthPercentage + "\n    robot fired: " + robotFired + "\n    loose kids: " + ambient.LooseKids);
        }

        public void PrintStats()
        {
            Console.WriteLine("Final Stats: " + "\n    mean filth percentage: " + finalFilthPercent + "\n    robot fired: " + robotFired + "\n    got all kids: " + gotAllKids);
        }
        public static (int, int) GetRandomInitPosition(IAmbient ambient)
        {
            List<(int, int)> freepos = ambient.GetFreePositions();
            int rand = new Random().Next(freepos.Count);
            return freepos[rand];
        }

        private void PrintMap()
        {
            string result = "";
            for (int i = 0; i < ambient.AmbientBoard.Rows; i++)
            {
                result += PrintLine();
                result += "|";
                for (int j = 0; j < ambient.AmbientBoard.Columns; j++)
                {
                    AmbientCell ac = ambient.AmbientBoard[i, j];

                    string[] temp = new string[5];

                    bool robot = (agent.Pos.x, agent.Pos.y) == (i, j);
                    bool robotHasChild = robot && agent.Carrying;

                    temp[0] = robot ? "@" : " ";
                    //temp[1] = ac.HasChild || robotHasChild ? "&" : " ";
                    temp[1] = " "; 
                    temp[2] = ac.IsObstacle ? "X" : " ";
                    temp[3] = ac.IsPlaypen ? "#" : " ";
                    temp[4] = ac.IsFilthy ? "*" : " ";

                    if(ac.HasChild)
                    {
                        if (ac.IsPlaypen)
                            temp[1] = ((Playpen)ac.elementInside).child.ToString();
                        else
                            temp[1] = ac.elementInside.ToString();
                    }
                    if (robotHasChild)
                        temp[1] = agent.Carried.ToString();
                        
                    result += string.Join("",temp) + "|";
                }
                result += "\n";
            }
            result += PrintLine();
            Console.WriteLine(result);
        }

        private string PrintLine()
        {
            string result = "-";
            for (int k = 0; k < ambient.AmbientBoard.Columns; k++)
            {
                result += "------";
            }
            result += "\n";
            return result;
        }
       
    }
}