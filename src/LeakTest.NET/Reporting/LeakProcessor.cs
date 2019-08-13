using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using System.Threading.Tasks;
using System.Diagnostics;
using LeakTest.Util;
using LeakTest.NET.GCModel;

namespace LeakTest.NET.Reporting
{
	public class LeakProcessor
	{
		readonly TaskQueue taskQueue = new TaskQueue();
		readonly List<LeakResult> leakResults = new List<LeakResult>();

		public LeakProcessor()
		{
		}

		public void Process(Task<Heap> heapTask, string id)
		{
			taskQueue.Enqueue(async () =>
			{
				var heap = await heapTask;

				var previousData = leakResults.LastOrDefault();
				var leakedObjects = DetectLeakedObjects(heap, previousData, id);

				leakResults.Add(new LeakResult(id, leakedObjects));
			});
		}

		public IEnumerable<LeakResult> GetLeaks()
		{
			taskQueue.Complete();

			return leakResults;
		}

		Dictionary<string, LeakTypeInformation> DetectLeakedObjects(Heap heap, LeakResult previousData, string id)
		{
			Debug.Assert(heap != null, "Failed to get heap");

			var trackedLeaks = heap.TrackedTypes;
			if (trackedLeaks.Count == 0)
				return new Dictionary<string, LeakTypeInformation>();

			// Create generator abstraction for output
			var leakedObjects = new Dictionary<string, LeakTypeInformation>(trackedLeaks.Count);

			foreach (var kvp in trackedLeaks)
			{
				var leaks = TryGetDefiniteLeaks(heap, kvp.Key);
				if (leaks != null)
					leakedObjects.Add(kvp.Key, leaks);
			}

			return leakedObjects;
		}

		static bool IsActualLeakSource(HeapRootKind rootKind)
		{
			return rootKind == HeapRootKind.Static
				|| rootKind == HeapRootKind.ContextStatic
				|| rootKind == HeapRootKind.GCHandle
				|| rootKind == HeapRootKind.ThreadStatic;
		}

		LeakTypeInformation TryGetDefiniteLeaks(Heap heap, string name)
		{
			if (!heap.TryGetHeapshotTypeInfo(name, out var typeInfo))
			{
				return null;
			}

			var visitedRoots = new HashSet<HeapObject>();

			int objectCount = 0;
			LeakGraph leakGraph = null;

			foreach (var obj in typeInfo.Objects)
			{
				visitedRoots.Clear();

				bool objectIsLeaked = false;
				var paths = heap.Graph.GetPredecessors(obj, vertex =>
				{
					if (heap.Roots.TryGetValue(vertex.Address, out var root))
					{
						visitedRoots.Add(vertex);
						objectIsLeaked |= IsActualLeakSource(root.RootKind);
					}
				});

				if (objectIsLeaked)
				{
					objectCount++;

					// TODO: Instead of just one graph, traverse all of them and group by common common paths.
					// Maybe create typePath graph for a given object type and reuse retention type information there
	 				if (leakGraph == null)
					{
						var objectRetentionGraph = new AdjacencyGraph<HeapObject, SReversedEdge<HeapObject, Edge<HeapObject>>>();

						// TODO: We need to check if the root is finalizer or ephemeron, and not report the value.
						foreach (var root in visitedRoots)
						{
							if (paths.TryGetPath(root, out var edges))
								objectRetentionGraph.AddVerticesAndEdgeRange(edges);
						}
						var graphviz = objectRetentionGraph.ToLeakGraphviz(heap);
						leakGraph = new LeakGraph(graphviz);
					}

				}
			}

			return objectCount != 0 ? new LeakTypeInformation(name, objectCount, leakGraph) : null;
		}
	}
}