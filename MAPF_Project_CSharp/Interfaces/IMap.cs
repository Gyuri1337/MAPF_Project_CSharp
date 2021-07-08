using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace MAPF_Project_CSharp
{
    interface IMap
    {
        /// <summary>
        /// Reset agents
        /// </summary>
        public void Reset();

        /// <summary>
        /// Add n more agent to the map
        /// </summary>
        /// <returns>true if there are more agents</returns>
        public bool AddAgent(int nAgent = 1);
        
        /// <summary>
        /// Getter for agent count
        /// </summary>
        public uint AgentCount { get; }

        /// <summary>
        /// Getter for vertex count
        /// </summary>
        public uint VertexCount { get; }

        /// <summary>
        /// Getter for Adjacency list
        /// </summary>
        public uint[][] AdjList { get; }

        /// <summary>
        /// Getter for agent start and end vertices
        /// </summary>
        public Tuple<uint, uint>[] Agents { get; }

        /// <summary>
        /// Getter for map name
        /// </summary>
        public string MapName { get; }

        /// <summary>
        /// Getter for all the agents not only already added to map
        /// </summary>
        public Tuple<uint, uint>[] AllAgents { get; }

        /// <summary>
        /// Maximum of minimal distances between agents minimal paths
        /// </summary>
        /// <returns></returns>
        public long? MaxDistance { get; set; }
    }
}
