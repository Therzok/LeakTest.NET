using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LeakTest.NET.GCModel;
using Mono.Profiler.Log;

namespace LeakTest.NET.Mono
{
	public partial class ProfilerProcessor
	{
		private Thread processingThread;
		private Visitor visitor;
		private LogProcessor processor;
		private CancellationTokenSource cts = new CancellationTokenSource();
		TcpClient client;
		StreamWriter writer;
		readonly Queue<TaskCompletionSource<Heap>> processingHeapshots = new Queue<TaskCompletionSource<Heap>>();

		public ProfilerProcessor(ProfilerOptions options, HashSet<string> trackedTypes)
		{
			Options = options;

			visitor = new Visitor(this, trackedTypes);
			processingThread = new Thread(new ThreadStart(ProcessFile));
			processingThread.Start();
		}

		public ProfilerOptions Options { get; }

		public Task RemainingHeapshotsTask => Task.WhenAll(processingHeapshots.Select(x => x.Task));

		public void Stop() => cts.Cancel();

		private void ProcessFile()
		{
			try
			{
				//Give runtime 10 seconds to create .mlpd
				for (int i = 0; i < 100; i++)
				{
					if (File.Exists(Options.MlpdOutputPath))
						break;
					Thread.Sleep(100);
				}
				using (var fs = new FileStream(Options.MlpdOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var logStream = new NeverEndingLogStream(fs, cts.Token))
				{
					processor = new LogProcessor(logStream, null, visitor);
					processor.Process(cts.Token);
				}
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		TaskCompletionSource<Heap> QueueHeapshot()
		{
			var tcs = new TaskCompletionSource<Heap>();

			lock (processingHeapshots)
			{
				processingHeapshots.Enqueue(tcs);
			}

			TriggerHeapshot();

			return tcs;

			void TriggerHeapshot()
			{
				if (client == null)
				{
					client = new TcpClient();
					client.Connect(IPAddress.Loopback, processor.StreamHeader.Port);
					writer = new StreamWriter(client.GetStream());
				}
				writer.Write("heapshot\n");
				writer.Flush();
			}
		}

		public Task<Heap> TakeHeapshot()
		{
			var tcs = QueueHeapshot();

			return tcs.Task;
		}

		public string GetMonoArguments()
		{
			switch (Options.Type)
			{
				case ProfilerOptions.ProfilerType.HeapOnly:
					return $"--profile=log:nodefaults,heapshot=ondemand,output=\"{Options.MlpdOutputPath}\"";
				case ProfilerOptions.ProfilerType.All:
					return $"--profile=log:nodefaults,heapshot-on-shutdown,heapshot=ondemand,gcalloc,gcmove,gcroot,counter,maxframes={Options.MaxFrames},output=\"{Options.MlpdOutputPath}\"";
				case ProfilerOptions.ProfilerType.Custom:
					return Options.CustomProfilerArguments;
				default:
					throw new NotImplementedException(Options.Type.ToString());
			}
		}
	}
}
