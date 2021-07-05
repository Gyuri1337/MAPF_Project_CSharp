using System;
using System.Collections.Generic;
using System.Text;

namespace MAPF_Project_CSharp
{
    interface IEncoder<Lit>
    {
        /// <summary>
        /// Reset the internal state of encoder before next instance encoding so dont need to create a new instance
        /// </summary>
        public void Reset();

        public void Encode(TimeExpGraphBothSide TEG);
    }
}
