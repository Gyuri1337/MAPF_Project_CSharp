using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;


using uintmap = System.UInt32; //Should overflow on big maps

namespace MAPF_Project_CSharp
{
    /// <summary>
    /// Reader for MAPF files in https://movingai.com/ format 
    /// </summary>
    class Reader : IReader
    {
        private const uint ASCII_at_sigh = 64;
        private const uint ASCII_point = 46;

        private string _mapName;
        private string _mapDir;
        private string _agentsDir;
        private uint _agentCount;
        private StreamReader _reader;
        private IMap _map;

        private uint[] _mapStart = new uint[0]; //Csharp must be initialized
        private uint[] _mapEnd = new uint[0];

        //Set the instances and maps directories.
        public Reader(string map_dir = "instances/maps/", string agents_dir = "instances/")
        {
            _mapDir = map_dir;
            _agentsDir = agents_dir;
        }
       
        public bool ReadAgents()
        {
            if (_reader == null) //Not open
            {
                Console.WriteLine("Reader is not opened - agents file");
                return false;
            }

            try
            {
                readAgents();
            }
            catch (IndexOutOfRangeException) //File format error
            {
                Console.WriteLine("Reader file format error - agents file");
                _reader.Close();
                _reader = null;
            }

            return true;
        }


        public bool ReadMap()
        {
            if (OpenMap())
            {
                try
                {
                    readMap();
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Incorrect file format - Map file");
                    return false;
                }

            }
            else
            {
                return false;
            }
            return true;
        }

        public IMap Map
        {
            get => _map;
        }

        //In our case we read the map file name from agent file -> null
        public bool Open(string agents_file_name, string map_file_name = null)
        {
            try
            {
                _reader = new StreamReader(_agentsDir + agents_file_name);
            }
            catch (FieldAccessException)
            {
                Console.WriteLine("File access exception - agents file");
                return false;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found - agents file");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Same as public Open() but it is used to open map
        /// </summary>
        /// <returns>true if file is opened</returns>
        private bool OpenMap()
        {
            try
            {
                _reader = new StreamReader(_mapDir + _mapName);
            }
            catch (FieldAccessException)
            {
                Console.WriteLine("File access exception - map file");
                return false;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found - map file");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read agents from agents file
        /// </summary>
        /// <returns>true if file was correctly read</returns>
        private int readAgents()
        {
            //Maps for agents start and end points
            uint[] mapStart = new uint[0]; //Csharp must be initialized
            uint[] mapEnd = new uint[0];

            //x,y saves the map size, others for saving agent start and end points
            uint sx, sy, ex, ey, x = 0, y = 0;
            uint agentCount = 0;

            string tmpLine;
            tmpLine= _reader.ReadLine();

            while ((tmpLine = _reader.ReadLine()) != null)
            { 
                tmpLine = tmpLine.Replace('\t', ' ');
                if (tmpLine.Length == 0)
                {
                    continue;
                }
                
                var splitedLine = tmpLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (_mapName == null) //First data line
                {
                    _mapName = splitedLine[1];
                    x = uint.Parse(splitedLine[2]);
                    y = uint.Parse(splitedLine[3]);
                    mapStart = new uint[x * y];
                    mapEnd = new uint[x * y];
                }

                ++agentCount;
                sx = uint.Parse(splitedLine[4]);
                sy = uint.Parse(splitedLine[5]);
                ex = uint.Parse(splitedLine[6]);
                ey = uint.Parse(splitedLine[7]);

                mapStart[x * sy + sx] = agentCount;
                mapEnd[x * ey + ex] = agentCount;


            }
            _reader.Close();
            _reader = null;

            _agentCount = agentCount;
            _mapEnd = mapEnd;
            _mapStart = mapStart;

            return (int)agentCount;
        }

        /// <summary>
        /// Read map from map fiel and creates IMap instancr with agents
        /// </summary>
        /// <returns>true if file was correctly read</returns>
        private bool readMap()
        {
            string actualLine;
            uint x = 0, y = 0;

            //empty line
            _reader.ReadLine();

            //Read x and y
            actualLine = _reader.ReadLine();
            var splitedLine = actualLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            y = uint.Parse(splitedLine[1]);

            actualLine = _reader.ReadLine();
            splitedLine = actualLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            x = uint.Parse(splitedLine[1]);

            actualLine = _reader.ReadLine();

            string prevLine;
            string nextLine;
            uintmap prevLineVertex = 0;
            uintmap vertexCount = 0;
            List < List < uintmap >> adjList= new List<List<uintmap>>();
            (uintmap, uintmap)[] agents = new (uint, uint)[_agentCount];

            //Prev line full of @ 
            StringBuilder sb = new StringBuilder();
            for (uint i = 0; i < x; i++)
            {
                sb.Append((char)ASCII_at_sigh);
            }

            prevLine = sb.ToString();

            for (int i = 0; i < y; i++)
            {
                actualLine = _reader.ReadLine();
                uint actualLineCount = 0;
                for (int j = 0; j < x; j++)
                {
                    if (actualLine[j] == ASCII_point)
                    {
                        ++actualLineCount;
                        adjList.Add(new List<uint>());
                        
                        if (prevLine[j] == ASCII_point) //Add edge between prev line and actual line points
                        {
                            adjList[(int)prevLineVertex].Add(vertexCount);
                            adjList[(int)vertexCount].Add(prevLineVertex);
                        }

                        if (j > 0 && actualLine[j - 1] == ASCII_point) //Add only one edge from actual vertex to previous
                        {
                            adjList[(int)vertexCount].Add(vertexCount - 1);

                        }

                        if (j < x - 1 && actualLine[j + 1] == ASCII_point)
                        {
                            adjList[(int)vertexCount].Add(vertexCount + 1);
                        }

                        ++vertexCount;
                    }

                    if (prevLine[j] == ASCII_point)
                    {
                        ++prevLineVertex;
                    }

                    if (_mapStart[x * i + j] != 0)
                    {
                        agents[_mapStart[x * i + j] - 1].Item1 = vertexCount -1;
                    }

                    if (_mapEnd[x * i + j] != 0)
                    {
                        agents[_mapEnd[x * i + j] - 1].Item2 = vertexCount -1;
                    }
                }

                prevLine = actualLine;
            }

            var adjListArray = adjList.Select(x => x.ToArray()).ToArray(); //List of list to array of array with linq
            var agentsTuple = agents.Select(x => Tuple.Create(x.Item1, x.Item2)).ToArray();
            _map = new Map(vertexCount, agentsTuple, adjListArray, _mapName);
            _reader.Close();
            _reader = null;
            return true;
        }
    }
}
