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


	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
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
