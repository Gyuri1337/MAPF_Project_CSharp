using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MAPF;

namespace MAPF_Project_CSharp
{
    class MAPF
    {
        //Private variables
        private TimeExpGraphBothSide _TEG;
        private IReader _reader;
        private IMap _map;

        //Constructor
        public MAPF(IReader reader)
        {
            _reader = reader;
        }

        //Public methods
        /// <summary>
        /// MAPF example usage of classes
        /// </summary>
        public void StartMAPF()
        {
            Console.WriteLine("Multi agent path finding with SAT Solver");
            Console.WriteLine("SAT Solver - Cryptominisat");
            Console.WriteLine("Enter a file name of an instance you want to solve: ");
            Console.WriteLine("!The instance and map file must be placed in the directories of reader!");
            var instanceName = "random-8-8-20-random-1.scen"; //Console.ReadLine(); 
            Console.WriteLine("Enter a timeout in seconds for the maximum solving time: ");
            var timeout = "10"; //Console.ReadLine();
            var timoutInt = int.Parse(timeout);

            //Reading files
            if(!_reader.Open(instanceName))
            {
                Console.WriteLine("File opening error");
            }

            if (!_reader.ReadAgents() || !_reader.ReadMap())
            {
                Console.WriteLine("File reading error");
            }

            //Preprocessing
            _map = _reader.Map;                             
            _map.AddAgent(1);                               //Add first agent
            _TEG = new TimeExpGraphBothSide(_reader.Map);   //Possible change for ITEG if there are more Time Expanded graph used but we will use only one
            _TEG.SetEnumerator();


            //Solving
            while (SolveWithTimout(timoutInt, Console.Out)) //Current TEG solvable in time
            {
                if (!_map.AddAgent()) //Add agent for TEG (TEG class is using the same instance of IMap ..)
                {
                    break; //No more agents -> we solved everything in timout
                }
            }
        }


        //Private methods
        /// <summary>
        /// Creates a new ISolver with GetSolver and try to solve the current TEG with timout
        /// </summary>
        /// <param name="timeout">maximum solving time for encoding and SAT solver</param>
        /// <param name="stream">Where to write the output</param>
        /// <returns>true if solvable</returns>
        private bool SolveWithTimout(int timeout, TextWriter stream)
        {
            var sw = new Stopwatch();
            sw.Start();
            var solver = GetSolver();
            var timoutMiliSec = timeout * 1000;
            bool solved = false;
            int layer = 0;
            while (!(solved = solver.Solve(timoutMiliSec - (int) sw.ElapsedMilliseconds)))
            {
                if (timoutMiliSec - (int) sw.ElapsedMilliseconds < 0)
                {
                    break;
                }

                layer = solver.AddLayer();
            }

            if (solved)
            {
                sw.Stop();
                stream.WriteLine("{0} agents with timespan {1} in {2} ms" , _map.Agents.Length ,layer, sw.ElapsedMilliseconds);
                return true;
            }
            
            stream.WriteLine("Time out - {0} ms", timoutMiliSec);
            return false;
            

        }

        //Return a Solver, if you want to use a different solver change it here
        private ISolver<int> GetSolver()
        {
            Func<ISATSolver<int>> lambda = () => new CryptoMiniSATSolver();
            Func<ISATSolver<int>, IEncoder<int> > lambdaEncoder = (ISATSolver<int> solver) => new Encoder<int>(solver);
            var teg = new TimeExpGraphBothSide(_TEG);
            return new Solver<int>(lambda, lambdaEncoder, teg, _map);
        }
    }
}
