﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.SkillTreeFiles;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace UnitTests
{
    [TestClass]
    public class TestMSTAlgorithm
    {
        // Builds a graph from an adjacency matrix.
        // Only the lower left half is checked.
        SearchGraph SearchGraphFromData(bool[,] adjacencyMatrix)
        {
            int n = adjacencyMatrix.GetUpperBound(0) + 1;
            // Don't screw this up.
            Assert.IsTrue(n == adjacencyMatrix.GetUpperBound(1) + 1);

            List<SkillNode> nodes = new List<SkillNode>();
            for (int i = 0; i < n; i++)
            {
                SkillNode node = new SkillNode();
                node.Id = (ushort)i;
                nodes.Add(node);
            }

            for (int i = 0; i < n; i++)
            {
                nodes[i].Neighbor = new List<SkillNode>();
                for (int j = 0; j < i; j++)
                {
                    if (adjacencyMatrix[i, j])
                    {
                        nodes[i].Neighbor.Add(nodes[j]);
                        // No directed edges atm.
                        nodes[j].Neighbor.Add(nodes[i]);
                    }
                }
            }

            SearchGraph graph = new SearchGraph();
            foreach (SkillNode node in nodes)
            {
                graph.AddNode(node);
            }
            return graph;
        }

        Dictionary<int, GraphNode> GetGraphNodesIdIndex(SearchGraph graph)
        {
            Dictionary<int, GraphNode> retval = new Dictionary<int, GraphNode>();
            foreach (GraphNode node in graph.NodeDict.Values)
            {
                retval.Add(node.Id, node);
            }
            return retval;
        }

        [TestMethod]
        public void TestDijkstra()
        {
            // 0 -- 1 -- 2 -- 3
            //  \        |   /
            //    \      | /
            //      4 -- 5
            bool[,] graph1 = {
                                 { false, true,  false, false, true,  false },
                                 { true,  false, true,  false, false, false },
                                 { false, true,  false, true,  false, true  },
                                 { false, false, true,  false, false, true  },
                                 { true,  false, false, false, false, true  },
                                 { false, false, true,  true,  true,  false },
                             };

            SearchGraph searchGraph1 = SearchGraphFromData(graph1);

            Dictionary<int, GraphNode> graphNodes = GetGraphNodesIdIndex(searchGraph1);


            DistanceLookup distanceLookup = new DistanceLookup(graphNodes.Values.ToList());
            Assert.AreEqual((uint)0, distanceLookup[graphNodes[0].DistancesIndex, graphNodes[0].DistancesIndex], "Failed 0 distance test");
            Assert.AreEqual((uint)2, distanceLookup[graphNodes[0].DistancesIndex, graphNodes[5].DistancesIndex], "Wrong distance");
            Assert.AreEqual((uint)3, distanceLookup[graphNodes[0].DistancesIndex, graphNodes[3].DistancesIndex], "Wrong distance");
        }

        [TestMethod]
        public void DijkstraUnconnected()
        {
            // 0 -- 1    2 -- 3
            //           |   /
            //           | /
            //      4 -- 5
            bool[,] graph2 =
            {
                {false, true, false, false, false, false},
                {true, false, false, false, false, false},
                {false, false, false, true, false, true},
                {false, false, true, false, false, true},
                {false, false, false, false, false, true},
                {false, false, true, true, true, false},
            };
            SearchGraph searchGraph2 = SearchGraphFromData(graph2);
            Dictionary<int, GraphNode> graphNodes2 = GetGraphNodesIdIndex(searchGraph2);
            var mstNodes2 = new List<GraphNode> { graphNodes2[0], graphNodes2[2], graphNodes2[4], graphNodes2[3] };

            var distances = new DistanceLookup(graphNodes2.Values.ToArray());
            Assert.IsNull(distances.GetShortestPath(mstNodes2[0].DistancesIndex, mstNodes2[3].DistancesIndex));
        }
        
        [TestMethod]
        public void TestMST()
        {
            // 0 -- 1 -- 2 -- 3
            //  \        |   /
            //    \      | /
            //      4 -- 5 -- 6 -- 7
            bool[,] graph1 = {
                                 { false, true,  false, false, true,  false, false, false },
                                 { true,  false, true,  false, false, false, false, false },
                                 { false, true,  false, true,  false, true,  false, false },
                                 { false, false, true,  false, false, true,  false, false },
                                 { true,  false, false, false, false, true,  false, false },
                                 { false, false, true,  true,  true,  false, true,  false },
                                 { false, false, false, false, false, true,  false, true  },
                                 { false, false, false, false, false, false, true,  false },
                             };

            SearchGraph searchGraph1 = SearchGraphFromData(graph1);

            Dictionary<int, GraphNode> graphNodes1 = GetGraphNodesIdIndex(searchGraph1);

            var mstNodes1 = new List<GraphNode>
                { graphNodes1[3], graphNodes1[5], graphNodes1[7], graphNodes1[0] };
            var distances = new DistanceLookup(mstNodes1);
            var mst1 = new MinimalSpanningTree(mstNodes1.Select(n => n.DistancesIndex).ToList(), distances);
            mst1.Span(graphNodes1[0].DistancesIndex);

            Assert.AreEqual(3, mst1.SpanningEdges.Count, "Wrong amount of spanning edges");
            var goalEdges = new[]
            {
                new []{0, 5}, new []{5, 3}, new []{5, 7}
            };
            foreach (var edge in goalEdges)
            {
                Assert.AreEqual(1,
                    mst1.SpanningEdges.Select(e => new Tuple<ushort, ushort>(distances.IndexToNode(e.Inside).Id, distances.IndexToNode(e.Outside).Id)).Count(
                        t =>
                            (t.Item1 == edge[0] && t.Item2 == edge[1]) ||
                            (t.Item1 == edge[1] && t.Item2 == edge[0])),
                    "Edge " + edge + " not contained exactly once.");
            }
        }

        [TestMethod]
        public void TestPriorityQueue()
        {

            int[] queueTestOrder = { 10, 3, 11, 6, -3, 17, 13, -6, 2, 8, -2, -8 };

            LinkedListPriorityQueue<TestNode> queue = new LinkedListPriorityQueue<TestNode>(30, queueTestOrder.Length);

            foreach (int t in queueTestOrder)
            {
                if (t > 0)
                    queue.Enqueue(new TestNode(t), (uint) t);
                if (t < 0)
                    Assert.IsTrue(queue.Dequeue().Priority == -t);
            }
        }

        class TestNode
        {
            public int Priority { get; }

            public TestNode(int priority)
            {
                Priority = priority;
            }
        }
    }
}
