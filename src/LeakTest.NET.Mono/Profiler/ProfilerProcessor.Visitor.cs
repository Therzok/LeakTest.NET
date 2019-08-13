using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeakTest.NET.GCModel;
using Mono.Profiler.Log;

namespace LeakTest.NET.Mono
{
	public partial class ProfilerProcessor
	{
		sealed class Visitor : LogEventVisitor
		{
			readonly Dictionary<long, HeapRootRegisterEvent> rootsEvents = new Dictionary<long, HeapRootRegisterEvent>();
			readonly Dictionary<long, TypeInformation> typeInfos = new Dictionary<long, TypeInformation>();
			readonly Dictionary<long, long> vtableToClassInfo = new Dictionary<long, long>();
			readonly HashSet<string> trackedTypeNames;
			readonly List<long> rootsEventsBinary = new List<long>();
			readonly ProfilerProcessor profilerProcessor;
			HeapBuilder heapBuilder;

			public Visitor(ProfilerProcessor profilerProcessor, HashSet<string> trackedTypeNames)
			{
				this.profilerProcessor = profilerProcessor;
				this.trackedTypeNames = trackedTypeNames;
			}

			public override void Visit(ClassLoadEvent ev)
				=> typeInfos[ev.ClassPointer] = new TypeInformation(ev.ClassPointer, ev.Name);

			public override void Visit(VTableLoadEvent ev)
				=> vtableToClassInfo[ev.VTablePointer] = ev.ClassPointer;

			public override void Visit(HeapBeginEvent ev)
				=> heapBuilder = new HeapBuilder(new HeapBuilderOptions(trackedTypeNames));

			public override void Visit(HeapRootsEvent ev)
			{
				for (int i = 0; i < ev.Roots.Count; ++i)
				{
					var root = ev.Roots[i];
					ProcessNewRoot(root.ObjectPointer, root.SlotPointer);
				}
			}

			public override void Visit(HeapRootRegisterEvent ev)
			{
				var index = rootsEventsBinary.BinarySearch(ev.RootPointer);
				if (index < 0)
				{
					//negative index means it's not there
					index = ~index;
					if (index - 1 >= 0)
					{
						var oneBefore = rootsEvents[rootsEventsBinary[index - 1]];
						if (oneBefore.RootPointer + oneBefore.RootSize > ev.RootPointer)
						{
							Console.WriteLine("2 HeapRootRegisterEvents overlap:");
							Console.WriteLine(ev);
							Console.WriteLine(oneBefore);
						}
					}
					if (index < rootsEventsBinary.Count)
					{
						var oneAfter = rootsEvents[rootsEventsBinary[index]];
						if (oneAfter.RootPointer < ev.RootPointer + ev.RootSize)
						{
							Console.WriteLine("2 HeapRootRegisterEvents overlap:");
							Console.WriteLine(ev);
							Console.WriteLine(oneAfter);
						}
					}
					rootsEventsBinary.Insert(index, ev.RootPointer);
					rootsEvents.Add(ev.RootPointer, ev);
				}
				else
				{
					Console.WriteLine("2 HeapRootRegisterEvent at same address:");
					Console.WriteLine(ev);
					Console.WriteLine(rootsEvents[ev.RootPointer]);
					rootsEvents[ev.RootPointer] = ev;
				}
			}

			public override void Visit(HeapRootUnregisterEvent ev)
			{
				if (rootsEvents.Remove(ev.RootPointer))
				{
					var index = rootsEventsBinary.BinarySearch(ev.RootPointer);
					rootsEventsBinary.RemoveAt(index);
				}
				else
				{
					Console.WriteLine("HeapRootUnregisterEvent attempted at address that was not Registred:");
					Console.WriteLine(ev);
				}
			}

			public override void Visit(HeapObjectEvent ev)
			{
				var classInfoId = vtableToClassInfo[ev.VTablePointer];
				var typeInfo = typeInfos[classInfoId];

				if (ev.ObjectSize == 0)
				{
					// This means it's just reporting references
					// TODO: Validate if we need to handle it.
					return;
				}

				var obj = heapBuilder.AddObject(typeInfo, ev.ObjectPointer);
				for (int i = 0; i < ev.References.Count; ++i)
				{
					var reference = ev.References[i];
					// Mono does not support getting the field name.
					heapBuilder.AddReference(obj, reference.ObjectPointer, viaField: null);
				}
			}

			public override void Visit(HeapEndEvent ev)
			{
				TaskCompletionSource<Heap> source;
				lock (profilerProcessor.processingHeapshots)
				{
					source = profilerProcessor.processingHeapshots.Dequeue();
				}

				source.SetResult(heapBuilder.ToHeap ());
			}

			void ProcessNewRoot(long objAddr, long rootAddr)
			{
				var index = rootsEventsBinary.BinarySearch(rootAddr);
				if (index < 0)
				{
					index = ~index;
					if (index == 0)
					{
						Console.WriteLine($"This should not happen. Root is before any HeapRootsEvent {rootAddr}.");
						return;
					}
					var rootReg = rootsEvents[rootsEventsBinary[index - 1]];
					if (rootReg.RootPointer < rootAddr && rootReg.RootPointer + rootReg.RootSize >= rootAddr)
					{
						heapBuilder.RegisterRoot(objAddr, rootReg.ToHeapRoot());
					}
					else
					{
						Console.WriteLine($"This should not happen. Closest root is too small({rootAddr}):");
						Console.WriteLine(rootReg);
					}
				}
				else
				{
					//We got exact match
					heapBuilder.RegisterRoot(objAddr, rootsEvents[rootAddr].ToHeapRoot());
				}
			}
		}
	}
}
