using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	public class Asset
	{
		internal virtual Difference GetDifference(int instanceID) { return null; }
		internal virtual void SetDifference(int instanceID, Difference difference) { }
	}

}