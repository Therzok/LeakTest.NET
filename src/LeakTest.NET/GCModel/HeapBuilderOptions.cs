using System;
using System.Collections.Generic;

namespace LeakTest.NET.GCModel
{
	public class HeapBuilderOptions
	{
		public bool SupportsFieldName { get; set; }

		internal HashSet<string> TrackedTypes { get; }
		public HeapBuilderOptions(HashSet<string> trackedTypes)
		{
			TrackedTypes = trackedTypes;
		}
	}
}
