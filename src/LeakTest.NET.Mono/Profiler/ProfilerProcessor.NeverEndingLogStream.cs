using System;
using System.IO;
using System.Threading;
using Mono.Profiler.Log;

namespace LeakTest.NET.Mono
{
	public partial class ProfilerProcessor
	{
		sealed class NeverEndingLogStream : LogStream
		{
			readonly CancellationToken token;
			readonly byte[] _byteBuffer = new byte[1];

			public NeverEndingLogStream(Stream baseStream, CancellationToken token) : base(baseStream)
			{
				this.token = token;
			}

			public override bool EndOfStream => false;

			public override int ReadByte()
			{
				while (BaseStream.Length - BaseStream.Position < 1)
				{
					Thread.Sleep(100);
					token.ThrowIfCancellationRequested();
				}
				// The base method on Stream is extremely inefficient in that it
				// allocates a 1-byte array for every call. Simply use a private
				// buffer instead.
				return Read(_byteBuffer, 0, sizeof(byte)) == 0 ? -1 : _byteBuffer[0];
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				while (BaseStream.Length - BaseStream.Position < count)
				{
					Thread.Sleep(100);
					token.ThrowIfCancellationRequested();
				}
				return BaseStream.Read(buffer, offset, count);
			}
		}
	}
}
