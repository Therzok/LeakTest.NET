using System;
namespace LeakTest.NET.Util
{
	public abstract class Logger
	{
		public abstract void Log(string message);
	}

	public class ConsoleLogger : Logger
	{
		public override void Log(string message)
		{
			Console.WriteLine(message);
		}
	}
}
