using System;
using QuickGraph;

namespace LeakTest.NET.GCModel
{
	public class HeapReference : Edge<HeapObject>
	{
		public string FieldName { get; }

		public HeapReference(HeapObject source, HeapObject target, string viaField) : base(source, target)
		{
			FieldName = viaField;
		}
	}
}
