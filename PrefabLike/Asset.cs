using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	public class Asset
	{
		internal virtual Dictionary<AccessKeyGroup, object> GetDifference(int instanceID) { return null; }
		internal virtual void SetDifference(int instanceID, Dictionary<AccessKeyGroup, object> difference) { }
	}

}
