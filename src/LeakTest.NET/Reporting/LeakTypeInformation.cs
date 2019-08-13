using System;
using LeakTest.NET.GCModel;
using QuickGraph;
using QuickGraph.Graphviz;

namespace LeakTest.NET.Reporting
{
	public class LeakTypeInformation
	{
		/// <summary>
		/// Gets the name of the leaked class.
		/// </summary>
		/// <value>The name of the class.</value>
		public string ClassName { get; }

		/// <summary>
		/// Gets the leaked object count.
		/// </summary>
		/// <value>The count.</value>
		public int Count { get; internal set; }

		/// <summary>
		/// Gets the retention graph.
		/// </summary>
		/// <value>The retention graph.</value>
		public LeakGraph RetentionGraph { get; }

		public LeakTypeInformation(string className, int count, LeakGraph retentionGraph)
		{
			ClassName = className;
			Count = count;
			RetentionGraph = retentionGraph;
		}
	}

	public class LeakGraph
	{
		public GraphvizAlgorithm<HeapObject, SReversedEdge<HeapObject, Edge<HeapObject>>> GraphViz { get; }

		public LeakGraph(GraphvizAlgorithm<HeapObject, SReversedEdge<HeapObject, Edge<HeapObject>>> graphviz)
		{
			GraphViz = graphviz ?? throw new ArgumentNullException(nameof(graphviz));
		}
	}
}
