using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using uintmap = System.UInt32;

namespace MAPF_Project_CSharp
{
    class TimeExpGraphBothSide : IEnumerable<uintmap[][]>
    {
        //Returns time expanded graph for each time i
        public IEnumerator<uintmap[][]> GetEnumerator()
        {

            avaibleVertices = new uintmap[_avaibleVertices.Length][];
            for (int i = 0; i < _avaibleVertices.Length; i++)
            {
                avaibleVertices[i] = new uint[_avaibleVertices[i].Length];
                _avaibleVertices[i].CopyTo(avaibleVertices[i], 0);
            }

            uintmap[][] previousTime = _map.Agents.Select(x => new uintmap[] { x.Item1 }).ToArray();
            List<uintmap[]> currentTime = new List<uintmap[]>();
            
            ++_position;
            while (_position < _maxDistance)
            {
                for (int agent = 0; agent < _map.Agents.Length; agent++)
                {
                    uintmap[] agentDeleted;
                    if ((_maxDistance - _position - 1) < _addedFromEnd[agent].Length)
                    {
                        agentDeleted = _addedFromEnd[agent][_maxDistance - _position - 1];
                    }
                    else
                    {
                        agentDeleted = new uintmap[] { };
                    }

                    uintmap[] agentAdded;
                    if (_addedFromStart[agent].Length > _position)
                    {
                        agentAdded = _addedFromStart[agent][_position];
                    }
                    else
                    {
                        agentAdded = new uintmap[] { };
                    }

                    foreach (var vertex in agentDeleted)
                    {
                        avaibleVertices[agent][vertex] = 0;
                    }

                    int i = 0;
                    List<uintmap> currentAgent = new List<uintmap>();
                    foreach (var vertex in previousTime[agent])
                    {
                        while (i != agentAdded.Length && agentAdded[i] < vertex)
                        {
                            if (avaibleVertices[agent][agentAdded[i]] != 0)
                            {
                                currentAgent.Add(agentAdded[i]);
                            }

                            ++i;
                        }

                        if (avaibleVertices[agent][vertex] != 0)
                        {
                            currentAgent.Add(vertex);
                        }
                    }

                    while (i != agentAdded.Length)
                    {
                        if (avaibleVertices[agent][agentAdded[i]] != 0)
                        {
                            currentAgent.Add(agentAdded[i]);
                        }

                        ++i;
                    }
                    currentTime.Add(currentAgent.ToArray());

                }

                previousTime = currentTime.ToArray();
                currentTime.Clear();
                yield return previousTime;
                ++_position;
            }

            _position = -1;
        }

        //Constructors
        public TimeExpGraphBothSide(IMap map)
        {
            _map = map;
        }
        public TimeExpGraphBothSide(TimeExpGraphBothSide copy)
        {
            _map = copy._map;
            _addedFromEnd = copy._addedFromEnd;
            _addedFromStart = copy._addedFromStart;
            _maxDistance = copy._maxDistance;
            _avaibleVertices = copy._avaibleVertices;
        }

        //Private variables
        private IMap _map;
        private uint _maxDistance = 0;
        private int _position = -1;

        //Added vertices for each agent in each timespan from end positions and start positions
        private uintmap[][][] _addedFromStart;
        private uintmap[][][] _addedFromEnd;

        //Avaible vertices at start points - Cut time expanded graph from right
        private uintmap[][] _avaibleVertices;
        private uintmap[][] avaibleVertices;

        //Public methods
        public void SetEnumerator()
        {
            BFS_Agents();
        }

        public void AddDistance()
        {
            for (int agent = 0; agent < _addedFromEnd.Length; agent++)
            {
                if (_addedFromEnd[agent].Length > _maxDistance)
                {
                    foreach (var vertex in _addedFromEnd[agent][_maxDistance])
                    {
                        avaibleVertices[agent][vertex] = 1;
                    }
                }
            }

            ++_maxDistance;
        }

        //Private methods
        private void BFS_Agents()
        {
            var agents = _map.AllAgents;
            var adjList = _map.AdjList;

            var addedFromStart = new List<uintmap[][]>();
            var addedFromEnd = new List<uintmap[][]>();

            var avaibleVertices = new List<uintmap[]>();

            foreach (var agent in agents)
            {
                addedFromStart.Add(BFS(agent.Item1, agent.Item2, adjList));
                addedFromEnd.Add(BFS(agent.Item2, agent.Item1, adjList, list: avaibleVertices, add: true));
            }

            _addedFromStart = addedFromStart.ToArray();
            _addedFromEnd = addedFromEnd.ToArray();
            _avaibleVertices = avaibleVertices.ToArray();

            SetAvaibleVertices();
        }

        private void SetAvaibleVertices()
        {
            int agent = 0;
            foreach (var added in _addedFromEnd) //Added array for each agent
            {
                if (added.Length > _maxDistance)
                {
                    for (int i = 1; i <= added.Length - _maxDistance; i++)
                    {
                        foreach (var vertex in added[added.Length - i])
                        {
                            _avaibleVertices[agent][vertex] = 0;
                        }
                    }
                }

                ++agent;
            }
        }

        private uintmap[][] BFS(uintmap start, uintmap end, uintmap[][] adjList, List<uintmap[]> list = null, bool add = false)
        {
            var addList = new List<uintmap[]>(); //List of our return value
            var bfsQueue = new Queue<uintmap>(); //Queue for bfs
            var state = new uintmap[_map.VertexCount]; //Bfs states
            var actualAdded = new List<uintmap>();

            var lastDistance = 0;

            bfsQueue.Enqueue(start);
            while (bfsQueue.Count != 0)
            {
                var actualVertex = bfsQueue.Dequeue();

                if (actualVertex == end) //We are looking for the minimum of max distances for lower bound of TEG
                {
                    _maxDistance = Math.Max(_maxDistance, state[actualVertex]);
                }

                if (state[actualVertex] > lastDistance)
                {
                    ++lastDistance;
                    actualAdded.Sort();
                    addList.Add(actualAdded.ToArray());
                    actualAdded.Clear();
                }

                var neighbors = adjList[(int) actualVertex];
                foreach (var vertex in neighbors)
                {
                    if (state[vertex] == 0 && vertex != start)
                    {
                        state[vertex] = state[actualVertex] + 1;
                        actualAdded.Add(vertex);
                        bfsQueue.Enqueue(vertex);
                    }
                }
            }

            if (add && list != null)
            {
                state[start] = 1;
                list.Add(state);
            }

            if (actualAdded.Count != 0)
            {
                addList.Add(actualAdded.ToArray());
            }

            return addList.ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
