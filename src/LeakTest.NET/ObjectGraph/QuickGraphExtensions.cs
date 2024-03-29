﻿using System;
using LeakTest.NET.GCModel;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.Search;
using QuickGraph.Graphviz;

namespace LeakTest.NET
{
	public static class QuickGraphExtensions
	{
		public static VertexPredecessorRecorderObserver<TVertex, TEdge> GetPredecessors<TVertex, TEdge>(this IVertexListGraph<TVertex, TEdge> graph, TVertex obj, VertexAction<TVertex> onVertex)
			where TEdge : IEdge<TVertex>
		{
			var bfsa = new BreadthFirstSearchAlgorithm<TVertex, TEdge>(graph);

			bfsa.ExamineVertex += (vertex) => {
				onVertex?.Invoke(vertex);
			};

			var vertexPredecessorRecorderObserver = new VertexPredecessorRecorderObserver<TVertex, TEdge>();
			using (vertexPredecessorRecorderObserver.Attach(bfsa))
			{
				bfsa.Compute(obj);
			}

			return vertexPredecessorRecorderObserver;
		}

		public static GraphvizAlgorithm<HeapObject, TEdge> ToLeakGraphviz<TEdge>(this IEdgeListGraph<HeapObject, TEdge> graph, Heap heapshot)
			where TEdge : IEdge<HeapObject>
		{
			var graphviz = new GraphvizAlgorithm<HeapObject, TEdge>(graph);

			graphviz.FormatVertex += (sender, e) => {
				var currentObj = e.Vertex;

				// Look up the object and set its type name.
				var typeName = currentObj.TypeInfo.Name;

				var formatter = e.VertexFormatter;

				e.VertexFormatter.Label = typeName;
				// Append root information.
				if (heapshot.Roots.TryGetValue(currentObj.Address, out var rootObject))
				{
					e.VertexFormatter.Label = $"{typeName}\\nRoot Kind: {rootObject.RootKind.ToString()}";
					e.VertexFormatter.Shape = QuickGraph.Graphviz.Dot.GraphvizVertexShape.Box;
				}
				else
				{
					e.VertexFormatter.Label = typeName;
				}
			};

			return graphviz;
		}
	}
}
