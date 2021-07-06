using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MAPF;

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
        }

        public bool Solve(double timeout) //in miliseconds
        {
            var sv = new Stopwatch();
            sv.Start();
            _encoder.Encode(_TEG, _map);  //Small time so no interrupt implemented
            sv.Stop();

            if (timeout - sv.ElapsedMilliseconds < 0 )
            {
                return false;
            }
            return _SAT.Solve((timeout - sv.ElapsedMilliseconds) / 1000); //Timout seconds;
        }

        public int AddLayer()
        {
            _SAT = getNewSolver();
            _encoder = getNewEncoder(_SAT);
            _TEG.AddDistance();
            return ++_layer;
        }

    }
}
