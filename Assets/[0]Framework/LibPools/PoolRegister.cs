/*===============================================================
Product:    Cryoshock
Developer:  Dimitry Pixeye - pixeye@hbrew.store
Company:    Homebrew - http://hbrew.store
Date:       2/27/2018  8:43 PM
================================================================*/

using System.Collections.Generic;

using UnityEngine;

namespace Homebrew
{
	public class PoolRegister : MonoBehaviour
	{
		public List<PoolNode> nodes = new List<PoolNode>();


		public void Reigster()
		{
			for (var i = 0; i < nodes.Count; i++)
			{
		 	nodes[i].Populate();
			}
		}
 


		[System.Serializable]
		public class PoolNode
		{
			public int pool = Pool.None;
			public GameObject prefab;
			public List<GameObject> createdObjs = new List<GameObject>();


			public void Populate()
			{
				var procPool = ProcessingGoPool.Default;
			

				var poolStash = procPool.pools[pool];
				
				 poolStash.RegisterObject(prefab);
				
				for (var i = 0; i < createdObjs.Count; i++)
				{
					poolStash.AddToPool(createdObjs[i], prefab);
				}
	 
			}
			
		}
	}
}