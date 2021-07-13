using System;
using System.Collections.Generic;

namespace PrefabLike
{
	/*
	 メインのツリー
	継承可能なのでDiffで構成される
	子ノードの生成もDiff

	Prefab
	実質メインツリーと同じ

	 */

	/*
	EditorNodeInformationから変更後データを復元する
	 */

	class Program
	{
		static void Main(string[] args)
		{
			TestDiff();
		}

		static void TestDiff()
		{
			TestStruct v = new TestStruct();

			var before = new FieldState();
			before.Store(v);

			v.A = 2.0f;

			var after = new FieldState();
			after.Store(v);

			var diff = before.GenerateDifference(after);

			foreach (var d in diff)
			{
				Console.WriteLine(d.Key);
				Console.WriteLine(d.Value);
			}
		}

		class TestNode : Node
		{
			public int Value1;
			public float Value2;
			public string Value3;
			public TestStruct Value4;
			List<float> Value5;
		}

		struct TestStruct
		{
			public float A;
			public float B;
			public float C;
		}
	}
}
