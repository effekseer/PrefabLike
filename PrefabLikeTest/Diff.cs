using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	struct TestStruct1
	{
		public float A;
		public float B;
		public float C;
		public TestStruct2 St;
	}

	struct TestStruct2
	{
		public float A;
		public float B;
		public float C;
	}

	[System.Serializable]
	class TestClass1
	{
		public float A;
		public float B;
		public float C;
	}

	class TestClassNotSerializable
	{
		public float A;
		public float B;
		public float C;
	}

	class TestList1
	{
		public System.Collections.Generic.List<int> Values = new System.Collections.Generic.List<int>();
	}

	class TestNodePrimitive : Node
	{
		public bool ValueBool;
		public byte ValueByte;
		public sbyte ValueSByte;
		public double ValueDobule;
		public float ValueFloat;
		public int ValueInt32;
		public uint ValueUInt32;
		public long ValueInt64;
		public ulong ValueUInt64;
		public short ValueInt16;
		public ushort ValueUInt16;
		public char ValueChar;
		public string ValueString;
	}

	class TestNodeStruct : Node
	{
		public TestStruct1 Struct1;
	}

	public class Diff
	{
		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void DiffPrimitive()
		{
			var env = new Environment();
			var v = new TestNodePrimitive();

			var before = new FieldState();
			before.Store(v, env);

			v.ValueInt32 = 5;

			var after = new FieldState();
			after.Store(v, env);

			{
				var diff = after.GenerateDifference(before);
				var pair = diff.First();
				var group = pair.Key;
				var field = group.Keys[0] as AccessKeyField;
				Assert.AreEqual(1, diff.Count);
				Assert.AreEqual("ValueInt32", field.Name);
				Assert.AreEqual(5, pair.Value);
			}

			{
				var diff = before.GenerateDifference(after);
				var pair = diff.First();
				var group = pair.Key;
				var field = group.Keys[0] as AccessKeyField;
				Assert.AreEqual(1, diff.Count);
				Assert.AreEqual("ValueInt32", field.Name);
				Assert.AreEqual(0, pair.Value);
			}
		}

		[Test]
		public void DiffStruct()
		{
			var env = new Environment();
			var v = new TestNodeStruct();
			v.Struct1.A = 1.0f;
			v.Struct1.St.B = 3.0f;

			var before = new FieldState();
			before.Store(v, env);
			v.Struct1.A = 2.0f;
			v.Struct1.St.B = 4.0f;

			var after = new FieldState();
			after.Store(v, env);

			var diff = before.GenerateDifference(after);
			Assert.AreEqual(2, diff.Count);
			Assert.AreEqual(1.0f, diff.First().Value);
		}

		[Test]
		public void DiffClass()
		{
			var env = new Environment();
			var v = new TestNodeClass();
			v.Class1_1 = new TestClass1();
			v.Class1_1.A = 1.0f;

			var before = new FieldState();
			before.Store(v, env);
			v.Class1_1.A = 2.0f;

			var after = new FieldState();
			after.Store(v, env);

			var diff = before.GenerateDifference(after);
			Assert.AreEqual(1, diff.Count);
			Assert.AreEqual(1.0f, diff.First().Value);
		}

		[Test]
		public void DiffList()
		{
			var env = new Environment();
			var v = new TestList1();

			var before = new FieldState();
			before.Store(v, env);
			v.Values.Add(1);

			var after = new FieldState();
			after.Store(v, env);

			var diff1 = before.GenerateDifference(after);
			Assert.AreEqual(1, diff1.Count);

			var diff2 = after.GenerateDifference(before);
			Assert.AreEqual(2, diff2.Count);
		}
	}
}