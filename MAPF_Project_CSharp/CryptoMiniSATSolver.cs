using System;
using System.Collections.Generic;
using System.Text;
using SATInterface;

namespace MAPF
{
    class CryptoMiniSATSolver : CryptoMiniSat, ISATSolver<int>
    {
        private bool[]? model = null;

        public void AddVariable(int count)
        {
            base.AddVars(count);
        }

        public Func<int, int> GetNegation() => (x) => -x;

        public bool Solve(double? timeout)
        {
            CryptoMiniSatNative.cmsat_set_max_time(base.Handle, timeout ?? 0);
            model = base.Solve();
            return model != null;
        }

        public bool Solve(double? timeout, int[]? assumption)
        {
            CryptoMiniSatNative.cmsat_set_max_time(base.Handle, timeout ?? 0);
            model = base.Solve(assumption);
            return model != null;
        }

        public bool[]? GetModel() => model;


        public void AddClauses(int[][] clauses)
        {
            foreach(var clause in clauses)
            {
                base.AddClause(clause);
                /*/ //Debug Clauses
                foreach (var VARIABLE in clause)
                {
                    Console.Write("\t{0}", VARIABLE);
                }
                Console.WriteLine();
                /**/
            }
        }

        public void AddClause(int literal)
        {
            base.AddClause(new int[] { literal });
            //Console.WriteLine(literal);
        }

        public Func<ulong, bool, int> GetToLiteral() => (variable, sign) => sign ? (int) (variable + 1) : -(int) (variable +1); //index from 0 but cryptominisat from 1
    }
}
