using System;
using System.Collections.Generic;
using System.Text;

using uintmap = System.UInt32;
namespace MAPF_Project_CSharp
{
    class Encoder<Lit> : IEncoder<Lit>
    {
        //Constructors
        Encoder(){}


        //Private variables
        private uintmap [][] _oldMap; //Map vertices to indexes in oldVertices !! always + 1
        private uintmap[][] _newMap;


        //Public methods

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Encode(IEnumerable<uintmap[][]> TEG, IMap map)
        {
            var agents = map.Agents;
            uintmap[][] oldVertices;
            var oldVerticesList = new List<uintmap[]>();
            foreach (var (start, _) in agents)
            {
                oldVerticesList.Add(new uintmap[]{start});
            }

            oldVertices = oldVerticesList.ToArray();
            oldVerticesList.Clear();
            
            foreach (var newVertices in TEG)
            {
                
            }
        }
    }
}
