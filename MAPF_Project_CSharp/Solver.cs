using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using MAPF;

using uintmap = System.UInt32;
namespace MAPF_Project_CSharp
{
    class Solver<Lit> : ISolver<Lit>
    {
        private Func<ISATSolver<Lit>> getNewSolver;
        private Func<ISATSolver<Lit>, IEncoder<Lit>> getNewEncoder;
        private IEncoder<Lit> _encoder;
        private TimeExpGraphBothSide _TEG;
        private ISATSolver<Lit> _SAT;
        private IMap _map;
        private int _layer = 0; 

        public Solver(Func<ISATSolver<Lit>> SATSolver, Func<ISATSolver<Lit>, IEncoder<Lit>> encoder, TimeExpGraphBothSide TEG , IMap map)
        {
            getNewSolver = SATSolver;
            getNewEncoder= encoder;
            _TEG = TEG;
            _SAT = getNewSolver();
            _encoder = getNewEncoder(_SAT);
            _map = map;
            _encoder.Reset();
        }

        public bool Solve(double timeout, bool check = false) //in miliseconds
        {
            var sv = new Stopwatch();
            sv.Start();
            
            _encoder.Encode(_TEG, _map);  //Small time so no interrupt implemented
            
            sv.Stop();

            if (timeout - sv.ElapsedMilliseconds < 0 )
            {
                return false;
            }

            if (_SAT.Solve((timeout - sv.ElapsedMilliseconds) / 1000)) //Timout seconds;
            {
                if (check)
                {
                    var paths = _encoder.AgentsPath(_TEG, _map, _SAT.GetModel());
                    if(!Check(paths))
                    {
                        throw new Exception("Incorrect model result");
                    }
                }
                return true;
            }

            return false;
        }

        private bool Check(uintmap[][] agentsPath)
        {
            //Check that the paths are valid 
            var adjList = _map.AdjList;
            foreach (var path in agentsPath) //Every path
            {
                for (int i = 1; i < path.Length; i++) //Every 2 vertex on path i -1 and i
                {
                    var tmp = adjList[path[i - 1]];
                    if (!tmp.Contains(path[i]) && path[i] != path[i-1])
                    {
                        return false;
                    }
                }
            }

            //Check collision when 2 agents on the same vertex at time i
            for (int i = 0; i < agentsPath[0].Length; i++) // In each time
            {
                for (int agent1 = 0; agent1 < agentsPath.Length; agent1++) 
                {
                    for (int agent2 = agent1 + 1; agent2 < agentsPath.Length; agent2++)
                    {
                        if (agentsPath[agent1][i] == agentsPath[agent2][i])
                        {
                            return false;
                        }
                    }
                }
            }

            //Check collision when 2 agents swap places - this is not allowed
            for (int i = 1; i < agentsPath[0].Length; i++) //For each 2 layer i -1 and i
            {
                for (int agent1 = 0; agent1 < agentsPath.Length; agent1++)
                {
                    for (int agent2 = agent1 + 1; agent2 < agentsPath.Length; agent2++)  //For each two agent 
                    {
                        if (agentsPath[agent1][i] == agentsPath[agent2][i -1] && agentsPath[agent1][i-1] == agentsPath[agent2][i])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public int AddLayer()
        {
            _SAT = getNewSolver();
            _encoder = getNewEncoder(_SAT);
            _TEG.AddDistance();
            _encoder.Reset();
            return ++_layer;
        }

    }
}
