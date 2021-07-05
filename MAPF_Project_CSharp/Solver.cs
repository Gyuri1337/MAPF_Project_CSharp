using System;
using System.Collections.Generic;
using System.Text;
using MAPF;

namespace MAPF_Project_CSharp
{
    class Solver<Lit> : ISolver<Lit>
    {
        private Func<ISATSolver<Lit>> getNewSolver;
        private IEncoder<Lit> _encoder;
        private TimeExpGraphBothSide _TEG;
        private ISATSolver<Lit> _SAT;
        private int _layer = 0; 

        public Solver(Func<ISATSolver<Lit>> SATSolver, IEncoder<Lit> encoder, TimeExpGraphBothSide TEG)
        {
            getNewSolver = SATSolver;
            _encoder = encoder;
            _TEG = TEG;
            _SAT = getNewSolver();
        }

        public bool Solve(int timeout)
        {
            
        }

        public int AddLayer()
        {
            _SAT = getNewSolver();
            _encoder.Reset();
            _TEG.AddDistance();
            return ++_layer;
        }
    }
}
