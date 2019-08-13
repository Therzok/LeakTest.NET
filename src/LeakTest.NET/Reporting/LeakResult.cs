using System;
using System.Collections.Generic;

namespace LeakTest.NET.Reporting
{
	public class LeakResult
	{
		/// <summary>
		/// Leak iteration id
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Leak information
		/// </summary>
		/// <summary>
		/// Each individual leak item
		/// </summary>
		public Dictionary<string, LeakTypeInformation> Leaks { get; }

		public LeakResult(string id, Dictionary<string, LeakTypeInformation> leaks)
		{
			Id = id;
			Leaks = leaks;
		}
	}
}
