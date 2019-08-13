using System;
using System.Collections.Generic;
using QuickGraph;

namespace LeakTest.NET.GCModel
{
	public class HeapBuilder
	{
		public readonly Dictionary<string, long> TrackedTypes = new Dictionary<string, long>();
		public readonly Dictionary<long, HeapTypeInformation> Types = new Dictionary<long, HeapTypeInformation>();
		public readonly Dictionary<long, HeapObject> Objects = new Dictionary<long, HeapObject>();
		public readonly Dictionary<long, HeapRoot> Roots = new Dictionary<long, HeapRoot>();
		readonly HashSet<string> trackedTypeNames;

		public AdjacencyGraph<HeapObject, Edge<HeapObject>> Graph;

		public HeapBuilder(HeapBuilderOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			trackedTypeNames = new HashSet<string>(options.TrackedTypes);
			Graph = new AdjacencyGraph<HeapObject, Edge<HeapObject>>(allowParallelEdges: options.SupportsFieldName);
		}

		public HeapObject AddObject(TypeInformation typeInfo, long address)
		{
			string typeName = typeInfo.Name;
			long typeId = typeInfo.TypeId;

			if (trackedTypeNames.Remove(typeName))
			{
				TrackedTypes.Add(typeName, typeId);
			}

			if (!Types.TryGetValue(typeId, out var heapTypeInfo))
			{
				Types[typeId] = heapTypeInfo = new HeapTypeInformation(typeInfo);
			}

			var heapObject = GetOrCreateObject(address, heapTypeInfo);

			Graph.AddVertex(heapObject);

			return heapObject;
		}

		public void AddReference (HeapObject source, long target, string viaField = null)
		{
			var referencedObject = GetOrCreateObject(target, null);
			Graph.AddEdge(new Edge<HeapObject>(source, referencedObject));
		}

		HeapObject GetOrCreateObject(long address, HeapTypeInformation heapshotTypeInfo = null)
		{
			if (!Objects.TryGetValue(address, out var heapObject))
			{
				Objects[address] = heapObject = new HeapObject(address);
			}

			if (heapObject.TypeInfo == null && heapshotTypeInfo != null)
			{
				heapObject.TypeInfo = heapshotTypeInfo.TypeInfo;
				heapshotTypeInfo.Objects.Add(heapObject);
			}

			return heapObject;
		}

		public void RegisterRoot(long address, HeapRoot heapRootRegisterEvent)
		{
			Roots[address] = heapRootRegisterEvent;
		}

		public Heap ToHeap ()
		{
			return new Heap(Roots, TrackedTypes, Types, Graph);
		}
	}
}
