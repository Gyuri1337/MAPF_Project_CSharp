using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace MAPF_Project_CSharp
{
    /// <summary>
    /// Reader which returns IMap
    /// </summary>
    interface IReader
    {
        /// <summary>
        /// Try to open the files
        /// </summary>
        /// <param name="agents_path">Path for agents file</param>
        /// <param name="map_path">Optional path for map file</param>
        /// <param name="map_dir">Path to folder for maps</param>
        /// <param name="agents_dir">Path to folder for agents</param>
        /// <returns>True if the file is opened</returns>

        public bool Open(string agents_path, string map_path = null);
        
        /// <summary>
        /// Read agents 
        /// </summary>
        /// <returns>True if successfully read agents</returns>
        public bool ReadAgents();

        /// <summary>
        /// Read graph and saves into IMap with agents.
        /// </summary>
        /// <returns>True if successfully created IMap instance</returns>
        public bool ReadMap();

        /// <summary>
        /// Getter for IMap instance
        /// </summary>
        public IMap Map { get; }
    }
}
