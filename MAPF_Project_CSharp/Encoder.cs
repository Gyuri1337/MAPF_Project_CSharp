using System;
using System.Collections.Generic;
using System.Text;
using MAPF;
using uintmap = System.UInt32;
namespace MAPF_Project_CSharp
{
    class Encoder<Lit> : IEncoder<Lit>
    {
        private static bool PozitiveLiteral = true;

        private static bool NegativeLiteral = false;
        
        //Constructors
        public Encoder(ISATSolver<Lit> SAT)
        {
            this._SAT = SAT;
            _ToLiteral = SAT.GetToLiteral();
            _Negation = SAT.GetNegation();
            this._litBuffer =new List<Lit[]>();
        }


        //Private variables
        private uintmap [][] _oldMap; //Map vertices to indexes in oldVertices !! always + 1
        private uintmap [][] _newMap;
        private ISATSolver<Lit> _SAT;
        private Func<ulong, bool, Lit> _ToLiteral;
        private Func<Lit, Lit> _Negation;  //No way for operator ~ like in Cpp
        private List<Lit[]> _litBuffer;
        private ulong _actualVariable;
        private uintmap[][] adjList;

        private ulong[] _newVariables;
        private ulong[] _oldVariables;


        //Public methods

        public void Reset()
        {
            _litBuffer.Clear();
            _oldMap = null;
            _newMap = null;
            _actualVariable = 0;
        }

        public void Encode(IEnumerable<uintmap[][]> TEG, IMap map)
        {
            //Initialization
            var agents = map.Agents;
            adjList = map.AdjList;
            
            uintmap[][] oldVertices;
            var oldVerticesList = new List<uintmap[]>();
            
            //Variable 0 is not used because it is better to calculate with _oldMap and _newMap, maps are indexed from 1 
            _SAT.AddVariable(agents.Length + 1);
            _SAT.AddClause(_ToLiteral(0, PozitiveLiteral));

            _oldVariables = new ulong[agents.Length];
            _oldMap = new uint[agents.Length][];

            //Iterator iterate only through new layers so we need to add the first layer
            int index = 0;
            foreach (var (start, _) in agents)
            {
                oldVerticesList.Add(new uintmap[]{start});
                _oldVariables[index] = (ulong)index;
                _oldMap[index] = new uintmap[map.VertexCount];
                _oldMap[index][start] = 1;
                _SAT.AddClause(_ToLiteral((uint)++index, PozitiveLiteral));
            }

            oldVertices = oldVerticesList.ToArray();
            oldVerticesList.Clear();

            //Initialization
            _actualVariable = (uint)agents.Length;
            int actualTime = 0;

            CreateNewMap(ref _newMap, agents.Length, map.VertexCount);


            //Iterate through all new Layers in TEG
            foreach (var newVertices in TEG) //For each time
            {
                ++actualTime;
                _newVariables = new ulong[agents.Length];
                for (int agent = 0; agent < agents.Length; agent++) //For each agent
                {

                    //Update new layer
                    uintmap i = 0;
                    foreach (var vertex in newVertices[agent]) //New map update
                    {
                        _newMap[agent][vertex] = ++i;  
                    }

                    _SAT.AddVariable(newVertices[agent].Length);
                    _newVariables[agent] = _actualVariable;
                    _actualVariable += (ulong)newVertices[agent].Length;

                    //Create clauses between new Layer and Previous Layer in TEG
                    CreateNextClauses(oldVertices[agent], newVertices[agent].Length, agent);

                    
                }

                //Create clauses between new Layer and Previous Layer in TEG
                CreateSwapClauses(newVertices);

                //Update old to new
                oldVertices = newVertices;
                _oldVariables = _newVariables;
                _oldMap = _newMap;
                CreateNewMap(ref _newMap, agents.Length, map.VertexCount);
            }

            //Create the last clauses
            for (int i = 0; i < agents.Length; i++)
            {
                _SAT.AddClause(_ToLiteral(_oldVariables[i] + 1, PozitiveLiteral));
            }
        }

        /// <summary>
        /// Creating the clauses: from every vertex if agent is there then must move to an another in the next layer, But can not move to 2 vertices 
        /// </summary>
        /// <param name="vertices">vertices in previous layer</param>
        /// <param name="newVerticesCount">vertices count in the next layer</param>
        /// <param name="agent">which agent</param>
        private void CreateNextClauses(uintmap[] vertices, int newVerticesCount, int agent)
        {
            _SAT.AddVariable((int)newVerticesCount);

            List<Lit> nextClause = new List<Lit>();
            List<ulong> neighbors = new List<ulong>();

            bool[][] n2 = new bool[(int)++newVerticesCount][];
            for (int i = 0; i < (int)newVerticesCount; i++)
            {
                n2[i] = new bool[newVerticesCount];
            }

            foreach (var vertex in vertices)
            {
                int i = 0;
                nextClause.Add(_ToLiteral(_oldVariables[agent] + _oldMap[agent][vertex], NegativeLiteral));
                while (i != adjList[vertex].Length && adjList[vertex][i] < vertex)
                {
                    if (_newMap[agent][adjList[vertex][i]] != 0)
                    {
                        neighbors.Add(_newMap[agent][adjList[vertex][i]]);
                        nextClause.Add(_ToLiteral(_newVariables[agent] + _newMap[agent][adjList[vertex][i]],
                            PozitiveLiteral));
                    }

                    ++i;
                }

                if (_newMap[agent][vertex] != 0)
                {
                    neighbors.Add(_newMap[agent][vertex]);
                    nextClause.Add(_ToLiteral(_newVariables[agent] + _newMap[agent][vertex],
                        PozitiveLiteral));
                }

                while (i != adjList[vertex].Length)
                {
                    if (_newMap[agent][adjList[vertex][i]] != 0)
                    {
                        neighbors.Add(_newMap[agent][adjList[vertex][i]]);
                        nextClause.Add(_ToLiteral(_newVariables[agent] + _newMap[agent][adjList[vertex][i]],
                            PozitiveLiteral));
                    }

                    ++i;
                }

                _litBuffer.Add(nextClause.ToArray());
                for (int j = 0; j < neighbors.Count; j++)
                {
                    for (int k = j + 1; k < neighbors.Count; k++)
                    {
                        if (n2[neighbors[j]][neighbors[k]] == false)
                        {
                            n2[neighbors[j]][neighbors[k]] = true;
                            _litBuffer.Add(new Lit[]{ _Negation(nextClause[j+1]), _Negation(nextClause[k + 1]) });
                        }
                    }
                }

                neighbors.Clear();
                nextClause.Clear();
            }

            _SAT.AddClauses(_litBuffer.ToArray());
            _litBuffer.Clear();
        }

        /// <summary>
        /// Creating clauses for: 2 agents can not be on the same vertex at same time in TEG, and agent can not SWAP on the map at the time i in TEG
        /// </summary>
        /// <param name="newVertices">vertices in the new layer</param>
        private void CreateSwapClauses(uintmap[][] newVertices)
        {
            List<Lit[]> clauses = new List<Lit[]>();
            for (int agent1 = 0; agent1 < _newMap.Length; agent1++)
            {
                for (int newVertexJ = 0; newVertexJ < newVertices[agent1].Length; newVertexJ++)
                {
                    for (int agent2 = agent1 + 1; agent2 < _newMap.Length; agent2++)
                    {
                        var vertex = newVertices[agent1][newVertexJ];
                        if (_newMap[agent2][vertex] > 0)
                        {
                            clauses.Add(new Lit[]
                            {
                                _ToLiteral(_newMap[agent2][vertex] + _newVariables[agent2], NegativeLiteral),
                                _ToLiteral(_newMap[agent1][vertex] + _newVariables[agent1], NegativeLiteral)
                            });
                        }

                        if (_oldMap[agent2][vertex] > 0)
                        {
                            foreach (var newVertexI in adjList[vertex])
                            {
                                if (_newMap[agent2][newVertexI] > 0 && _oldMap[agent1][newVertexI] > 0)
                                {
                                    clauses.Add(new Lit[]
                                    {
                                        _ToLiteral(_newMap[agent1][vertex] + _newVariables[agent1], NegativeLiteral),
                                        _ToLiteral(_oldMap[agent2][vertex] + _oldVariables[agent2], NegativeLiteral),
                                        _ToLiteral(_oldMap[agent1][newVertexI] + _oldVariables[agent1],
                                            NegativeLiteral),
                                        _ToLiteral(_newMap[agent2][newVertexI] + _newVariables[agent2], NegativeLiteral)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            _SAT.AddClauses(clauses.ToArray());
            clauses.Clear();
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="map">creates a new jagged map for the pointer map - squared</param>
        /// <param name="a">size 1</param>
        /// <param name="b">size 2</param>
        private void CreateNewMap(ref uintmap[][] map, int a , ulong b)
        {
            map = new uint[a][];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new uintmap[b];
            }
        }

        /// <summary>
        /// returns the path for all agents 
        /// </summary>
        /// <param name="TEG">TEG</param>
        /// <param name="map">map</param>
        /// <param name="model">sat model</param>
        public void Write(IEnumerable<uintmap[][]> TEG, IMap map, bool[] model)
        {
            var agents = map.Agents;
            adjList = map.AdjList;

            uintmap[][] oldVertices;
            var oldVerticesList = new List<uintmap[]>();

            _SAT.AddVariable(agents.Length + 1);
            _SAT.AddClause(_ToLiteral(0, PozitiveLiteral));

            _oldVariables = new ulong[agents.Length];
            _oldMap = new uint[agents.Length][];

            int index = 0;
            foreach (var (start, _) in agents)
            {
                oldVerticesList.Add(new uintmap[] { start });
                _oldVariables[index] = (ulong)index;
                _oldMap[index] = new uintmap[map.VertexCount];
                _oldMap[index][start] = 1;
                _SAT.AddClause(_ToLiteral((uint)++index, PozitiveLiteral));
            }

            oldVertices = oldVerticesList.ToArray();
            oldVerticesList.Clear();


            _actualVariable = (uint)agents.Length;
            int actualTime = 0;

            CreateNewMap(ref _newMap, agents.Length, map.VertexCount);

            foreach (var newVertices in TEG) //For each time
            {
                ++actualTime;
                _newVariables = new ulong[agents.Length];
                for (int agent = 0; agent < agents.Length; agent++) //For each agent
                {

                    //Update new layer
                    uintmap i = 0;
                    foreach (var vertex in newVertices[agent]) //New map update
                    {
                        _newMap[agent][vertex] = ++i;
                    }

                    _SAT.AddVariable(newVertices[agent].Length);
                    _newVariables[agent] = _actualVariable;
                    _actualVariable += (ulong)newVertices[agent].Length;

                    CreateNextClauses(oldVertices[agent], newVertices[agent].Length, agent);


                }

                CreateSwapClauses(newVertices);

                oldVertices = newVertices;
                //Update old to new
                _oldVariables = _newVariables;
                _oldMap = _newMap;
                CreateNewMap(ref _newMap, agents.Length, map.VertexCount);
            }

            for (int i = 0; i < agents.Length; i++)
            {
                _SAT.AddClause(_ToLiteral(_oldVariables[i] + 1, PozitiveLiteral));
            }
        }
    }
}
