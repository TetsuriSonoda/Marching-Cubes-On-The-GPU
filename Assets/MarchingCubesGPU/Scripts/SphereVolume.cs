// Copyright (c) by T. Sonoda
// Released under the MIT license.
// see https ://opensource.org/licenses/MIT

using UnityEngine;
using UnityEngine.Events;
public class SphereVolume : MonoBehaviour {
	struct Voxel
	{
		public float sdf;
		public float colorR;
		public float colorG;
		public float colorB;
		public float colorA;
		public int weight;
	};

	// event
	[System.Serializable]
	public class ComputeBufferEvent : UnityEvent<ComputeBuffer> { }

	[Space]
	public ComputeBufferEvent bufferBinding;

	//The size of the voxel array for each dimension
	public int gridSize = 64;

	private ComputeBuffer voxelBuffer = null; // provided from outside
	private Voxel[] voxelArray; // for debug input use

	// Use this for initialization
	void Start () {
		//Holds the voxel values, generated from perlin noise.
		voxelBuffer = new ComputeBuffer(gridSize * gridSize * gridSize, sizeof(float) * 6);
		voxelArray = new Voxel[gridSize * gridSize * gridSize];

		for (int z = 0; z < gridSize; z++)
		{
			for (int y = 0; y < gridSize; y++)
			{
				for (int x = 0; x < gridSize; x++)
				{
					var targetIndex = gridSize * gridSize * z + gridSize * y + x;

					var sx = x - gridSize / 2;
					var sy = y - gridSize / 2;
					var sz = z - gridSize / 2;
					var amount = 800.0f - (sx * sx + sy * sy + sz * sz);

					voxelArray[targetIndex].sdf = amount;
				}
			}
		}

		voxelBuffer.SetData(voxelArray);

		bufferBinding.Invoke(voxelBuffer);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
