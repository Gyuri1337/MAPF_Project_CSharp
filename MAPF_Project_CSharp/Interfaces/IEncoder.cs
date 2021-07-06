using System;
using System.Collections.Generic;
using System.Text;

using uintmap = System.UInt32;
namespace MAPF_Project_CSharp
{
    interface IEncoder<Lit>
    {
        /// <summary>
        /// Reset the internal state of encoder before next instance encoding so dont need to create a new instance
        /// </summary>
        public void Reset();

        /// <summary>
        /// Encode the instance with TEG into SAT solver
        /// </summary>
        /// <param name="TEG"></param>
        /// <param name="map"></param>
        public void Encode(IEnumerable<uintmap[][]> TEG, IMap map);
    }
}
