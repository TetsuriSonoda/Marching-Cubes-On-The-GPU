﻿using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

#pragma warning disable 162

using ImprovedPerlinNoiseProject;

namespace MarchingCubesGPUProject
{
    public class MarchingCubesGPU_4DNoise : MonoBehaviour
    {

        //The size of the voxel array for each dimension
        const int N = 40;

        //The size of the buffer that holds the verts.
        //This is the maximum number of verts that the 
        //marching cube can produce, 5 triangles for each voxel.
        const int SIZE = N * N * N * 3 * 5;

        public int m_seed;

        public float m_speed = 2.0f;

        public Material m_drawBuffer;

        public ComputeShader m_perlinNoise;

        public ComputeShader m_marchingCubes;

        public ComputeShader m_normals;

        public ComputeShader m_clearBuffer;

        ComputeBuffer m_noiseBuffer, m_meshBuffer;
        
        RenderTexture m_normalsBuffer;

        ComputeBuffer m_cubeEdgeFlags, m_triangleConnectionTable;

        GPUPerlinNoise perlin;

        void Start()
        {
            //Allows this camera to draw mesh procedurally.
            PostRenderEvent.AddEvent(Camera.main, DrawMesh);

            //There are 8 threads run per group so N must be divisible by 8.
            if (N % 8 != 0)
                throw new System.ArgumentException("N must be divisible be 8");

            //Holds the voxel values, generated from perlin noise.
            m_noiseBuffer = new ComputeBuffer(N * N * N, sizeof(float));

            //Holds the normals of the voxels.
            m_normalsBuffer = new RenderTexture(N, N, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            m_normalsBuffer.dimension = TextureDimension.Tex3D;
            m_normalsBuffer.enableRandomWrite = true;
            m_normalsBuffer.useMipMap = false;
            m_normalsBuffer.volumeDepth = N;
            m_normalsBuffer.Create();

            //Holds the verts generated by the marching cubes.
            m_meshBuffer = new ComputeBuffer(SIZE, sizeof(float) * 7);

            //These two buffers are just some settings needed by the marching cubes.
            m_cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
            m_cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
            m_triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
            m_triangleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

            //Make the perlin noise, make sure to load resources to match shader used.
            perlin = new GPUPerlinNoise(m_seed);
            perlin.LoadResourcesFor4DNoise();

        }


        void Update()
        {
            //Clear the buffer from last frame.
            m_clearBuffer.SetInt("_Width", N);
            m_clearBuffer.SetInt("_Height", N);
            m_clearBuffer.SetInt("_Depth", N);
            m_clearBuffer.SetBuffer(0, "_Buffer", m_meshBuffer);

            m_clearBuffer.Dispatch(0, N / 8, N / 8, N / 8);

            //Make the voxels.
            m_perlinNoise.SetInt("_Width", N);
            m_perlinNoise.SetInt("_Height", N);
            m_perlinNoise.SetFloat("_Frequency", 0.02f);
            m_perlinNoise.SetFloat("_Lacunarity", 2.0f);
            m_perlinNoise.SetFloat("_Gain", 0.5f);
            m_perlinNoise.SetFloat("_Time", Time.realtimeSinceStartup * m_speed);
            m_perlinNoise.SetTexture(0, "_PermTable1D", perlin.PermutationTable1D);
            m_perlinNoise.SetTexture(0, "_PermTable2D", perlin.PermutationTable2D);
            m_perlinNoise.SetTexture(0, "_Gradient4D", perlin.Gradient4D);
            m_perlinNoise.SetBuffer(0, "_Result", m_noiseBuffer);

            m_perlinNoise.Dispatch(0, N / 8, N / 8, N / 8);

            //Make the voxel normals.
            m_normals.SetInt("_Width", N);
            m_normals.SetInt("_Height", N);
            m_normals.SetBuffer(0, "_Noise", m_noiseBuffer);
            m_normals.SetTexture(0, "_Result", m_normalsBuffer);

            m_normals.Dispatch(0, N / 8, N / 8, N / 8);

            //Make the mesh verts
            m_marchingCubes.SetInt("_Width", N);
            m_marchingCubes.SetInt("_Height", N);
            m_marchingCubes.SetInt("_Depth", N);
            m_marchingCubes.SetInt("_Border", 1);
            m_marchingCubes.SetFloat("_Target", 0.0f);
            m_marchingCubes.SetBuffer(0, "_Voxels", m_noiseBuffer);
            m_marchingCubes.SetTexture(0, "_Normals", m_normalsBuffer);
            m_marchingCubes.SetBuffer(0, "_Buffer", m_meshBuffer);
            m_marchingCubes.SetBuffer(0, "_CubeEdgeFlags", m_cubeEdgeFlags);
            m_marchingCubes.SetBuffer(0, "_TriangleConnectionTable", m_triangleConnectionTable);

            m_marchingCubes.Dispatch(0, N / 8, N / 8, N / 8);
        }

        /// <summary>
        /// Draws the mesh when cameras OnPostRender called.
        /// </summary>
        /// <param name="camera"></param>
        void DrawMesh(Camera camera)
        {
            //Since mesh is in a buffer need to use DrawProcedual called from OnPostRender or OnRenderObject
            m_drawBuffer.SetBuffer("_Buffer", m_meshBuffer);
            m_drawBuffer.SetPass(0);

            Graphics.DrawProcedural(MeshTopology.Triangles, SIZE);
        }

        void OnDestroy()
        {
            //MUST release buffers.
            m_noiseBuffer.Release();
            m_meshBuffer.Release();
            m_cubeEdgeFlags.Release();
            m_triangleConnectionTable.Release();
            m_normalsBuffer.Release();

            PostRenderEvent.RemoveEvent(Camera.main, DrawMesh);
        }

    }

}




























