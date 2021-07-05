using System;
using System.Collections.Generic;
using System.Text;

namespace MAPF_Project_CSharp
{
    interface ISolver<Lit>
    {
        /// <summary>
        /// Encode the instance into a sat solver and try to solve it
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>true if solvable</returns>
        public bool Solve(int timeout);
        
        /// <summary>
        /// Add a time layer to TEG
        /// </summary>
        /// <returns>returns current time</returns>
        public int AddLayer();

    }
}
