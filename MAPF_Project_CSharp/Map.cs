using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAPF_Project_CSharp
{
    //Implementation for IMap
    class Map : IMap
    {
        //Constructors
        Map(uint vertices, Tuple<uint, uint>[] agentList, uint[][] adjList, string mapName)
        {
            this.VertexCount = vertices;
            this.AdjList = adjList;
            this.MapName = mapName;
            this.Agents = agentList;

        }


        //Private variables
        private int _position;
        

        //Properties
        public uint[][] AdjList { get; }

        public uint AgentCount => (uint) _position;

        public Tuple<uint, uint>[] Agents
        {
            get => Agents.Take(_position).ToArray();
            private set => Agents = value;
        }

        public string MapName { get; }

        public uint VertexCount { get; }


        //Methods
        public void Reset()
        {
            _position = 0;
        }

        public bool AddAgent(int nAgent = 1)
        {
            if (nAgent < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (Agents.Length > _position)
            {
                var tmp = Math.Min(nAgent, Agents.Length - _position);
                _position += tmp;
                return true;
            }

            return false;
        }
    }
}
