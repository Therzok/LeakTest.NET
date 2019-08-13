using System;
namespace LeakTest.NET.GCModel
{
	public class HeapRoot
	{
		public HeapRootKind RootKind { get; }
		public long Address { get; }

		public HeapObject GetObject(Heap heap) => null;

		public HeapRoot(long address, HeapRootKind rootKind)
		{
			Address = address;
			RootKind = rootKind;
		}
	}
}
