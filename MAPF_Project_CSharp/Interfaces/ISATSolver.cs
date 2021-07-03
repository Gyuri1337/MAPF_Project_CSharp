using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SATInterface;

namespace MAPF
{
    ///Sat solver interface implementation for Cryptominisat 
    interface ISATSolver<Lit>
    {
        
        ///Getter for lambda which creates the literal, useful when more variables than range of int
        public Func<uint, bool, Lit> GetToLiteral();

        ///Getter for model
        public bool[]? GetModel();

        ///Add literal as clause to model
        public void AddClause(Lit literal);
        
        ///Add clauses to model
        public void AddClauses(Lit[][] clauses);

        ///Add n varaibles to model
        public void AddVariable(int count = 1);

        ///Solve the problem and returns true if the problem is solvable in the timeout passed
        public bool Solve(double? timeout = null);

        ///Solve the problem with assumption in timeout
        public bool Solve(double? timeout = null, Lit[]? assumption = null);

    }
}
