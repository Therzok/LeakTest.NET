using System;
namespace LeakTest.NET.GCModel
{
	/// <summary>
	/// Base type represented in an in-memory layout.
	/// </summary>
	public class HeapObject : IEquatable<HeapObject>
	{
		public long Address { get; }
		public TypeInformation TypeInfo { get; set; }

		internal HeapObject(long address)
		{
			Address = address;
		}

		public bool Equals(HeapObject other) => Address == other.Address;
		public override bool Equals(object obj) => obj is HeapObject other && Equals(other);
		public override int GetHashCode() => Address.GetHashCode();
	}
}
