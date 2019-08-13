using System;
using System.Collections.Generic;
using QuickGraph;

namespace LeakTest.NET.GCModel
{
	public class Heap
	{
		//public readonly Dictionary<long, HeapObject> Objects; - Possibly not needed.
		public readonly Dictionary<string, long> TrackedTypes;
		public readonly Dictionary<long, HeapRoot> Roots;
		public readonly Dictionary<long, HeapTypeInformation> Types;

		public readonly ReversedBidirectionalGraph<HeapObject, Edge<HeapObject>> Graph;

		internal Heap(
			Dictionary<long, HeapRoot> roots,
			Dictionary<string, long> trackedTypes,
			Dictionary<long, HeapTypeInformation> types,
			IVertexAndEdgeListGraph<HeapObject, Edge<HeapObject>> referenceGraph)
		{
			//Objects = nativeHeapshot.Objects;
			Roots = roots;
			TrackedTypes = trackedTypes;
			Types = types;

			var graphWithInReferences = new BidirectionAdapterGraph<HeapObject, Edge<HeapObject>>(referenceGraph);
			// Construct the in-edge graph, so we can trace an object's retention path.
			Graph = new ReversedBidirectionalGraph<HeapObject, Edge<HeapObject>>(graphWithInReferences);
		}

		public bool TryGetHeapshotTypeInfo(string name, out HeapTypeInformation heapshotTypeInfo)
		{
			heapshotTypeInfo = null;

			return TrackedTypes.TryGetValue(name, out var typeId) && TryGetHeapshotTypeInfo(typeId, out heapshotTypeInfo);
		}

		public bool TryGetHeapshotTypeInfo(long typeId, out HeapTypeInformation heapshotTypeInfo)
			=> Types.TryGetValue(typeId, out heapshotTypeInfo);

		public int GetObjectCount(long typeId)
			=> Types.TryGetValue(typeId, out var heapshotTypeInfo) ? heapshotTypeInfo.Objects.Count : 0;
	}
}
