using System;
using LeakTest.NET.GCModel;
using Mono.Profiler.Log;

namespace LeakTest.NET.Mono
{
	static class LogExtensions
	{
		public static HeapRoot ToHeapRoot(this HeapRootRegisterEvent rootReg)
			=> new HeapRoot(rootReg.RootPointer, (HeapRootKind)(int)rootReg.Source);
	}
}
