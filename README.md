# Marching-Cubes-On-The-GPU

Modified SCrawk's Marching Cubes project (https://github.com/Scrawk/Marching-Cubes-On-The-GPU).

-Simplified with one scene.
-Changed input data from float array to Voxel structure for future update.
-Input data is provided by OnVoxelUpdate event.
-Changed rendering from Camera post render event to OnRenderObject.
-Removed perlin noise dependency and added simple sphere volume example.
-Applied Object transformation to rendered mesh.
-Applied diffuse Object lighting depends on Unity light.

