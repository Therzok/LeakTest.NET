using System;
using System.Collections.Generic;

namespace LeakTest.NET.GCModel
{
	public class HeapTypeInformation
	{
		public TypeInformation TypeInfo { get; }
		public List<HeapObject> Objects { get; } = new List<HeapObject>();

		public HeapTypeInformation(TypeInformation typeInfo)
		{
			TypeInfo = typeInfo;
		}
	}
}
