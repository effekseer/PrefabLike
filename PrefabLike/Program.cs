using System;

namespace PrefabLike
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}
	}

	class TestNode : Node
	{
		public TestStruct V1;
	}

	struct TestStruct
	{
		public float A;
		public float B;
		public float C;
	}

	class Package : Resource
	{
		public Node RootNode;
	}

	class Node	
	{
		public Package Package;
		public Modified Modified;

		// なんか色々
	}

	class Resource
	{ 
		// なんか色々
		// ファイルパスやなんらかで一意に定まることを前提
	}

	/// <summary>
	/// A class to contain differences
	/// </summary>
	class Modified
	{ 
	
	}

	class CommandManager
	{
		public static void StartEdit(object o)
		{ 
		}

		public static void NotifyEdit(object o)
		{ 
		
		}
		public static void EndEdit(object o)
		{
		}
	};
}
