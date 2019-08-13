using System;
namespace LeakTest.NET.GCModel
{
	public enum HeapRootKind
	{
		Other = 0,
		Stack = 1,
		FinalizerQueue = 2,
		Static = 3,
		ThreadStatic = 4,
		ContextStatic = 5,
		GCHandle = 6,
		Jit = 7,
		Threading = 8,
		AppDomain = 9,
		Reflection = 10,
		Marshal = 11,
		ThreadPool = 12,
		Debugger = 13,
		Handle = 14,
		Ephemeron = 15,
	}
}
