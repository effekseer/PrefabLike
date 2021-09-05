using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	public class Asset
	{

	}

	public class AssetDatabase
	{
		Dictionary<string, WeakReference<Asset>> assets = new Dictionary<string, WeakReference<Asset>>();

		public Asset LoadAsset(string path)
		{
			if (assets.ContainsKey(path))
			{
				Asset asset;
				if (assets[path].TryGetTarget(out asset))
				{
					return asset;
				}
				else
				{
					assets.Remove(path);
				}
			}

			// TODO
			return null;
		}

		public void SaveAsset(string path, Asset asset)
		{
			// TODO
		}
	}
}