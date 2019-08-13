using System;
namespace LeakTest.NET.GCModel
{
	public class TypeInformation
	{
		public long TypeId { get; }
		public string Name { get; }

		public TypeInformation(long typeId, string name)
		{
			TypeId = typeId;
			Name = name;
		}
	}
}
