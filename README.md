# Water Caustincs uisng Snell's law and Photon Mapping

### New unityproject, raycast and linerender (3 May)
Initiated a project in uinty. Added a water mesh at top of a simple room, made up of planes and cubes. A light was added to the scene with a script. Using the script a raycast was shooting from the light and as it hit the water a linerender was created. Next a raycast was created in the direction of the normal of the hitpoint.
<img width="620" alt="image" src="https://github.com/user-attachments/assets/87228f37-bda6-41a4-b681-cc7ac582c2e3" />

### First caustics (9 May)
From looking at the scenarios in several papers we found that figure 8 in [^2] could work well. So we downloaded a model of a Cornell box [^3] from the internet. 

The angle of the new raycast from the surface was calculate using an approximate form of Snell's law found in [^1]. Once it hit the ground white dots appear where the raycast collide with the ground. This is when we also implmented the functionality to send a plane of rays into the scene in order to see that they formed accurate caustics. 

In this image we were disappointed to see that the dots seemed to be spaced out completely randomly. In order to figure out if this was a problem with the mesh or the implementation we tried different scales for the mesh. 

<img width="520" alt="Skärmbild 2026-05-08 165908" src="https://github.com/user-attachments/assets/35284d79-8e00-48e8-aa21-7f011172fa78" />

We scaled the water a lot in one direction and saw that the dots seemed to make lines, meaning they were dependent on the shape of the mesh. 

<img width="650" alt="Skärmbild 2026-05-08 165455" src="https://github.com/user-attachments/assets/0e23a32a-ca30-4b0f-9343-7ab7b7a780e1" />

We finally found that we had just compressed the mesh too much and scaled it up. This made each triangle very visible, and we realized that we might need to change water mesh in order to see results we could judge. 
<img width="650" alt="Skärmbild 2026-05-08 165249" src="https://github.com/user-attachments/assets/d5c40677-6bc9-463f-bda9-21af48848ab3" />

With this imformation we decided to create a water surface mesh of a circular sine curve to mimic a droplet disturbing the surface in [^2], since those caustics have a more recognizable shape. 

### Painting textures (19 May)
With the new models we could start experimenting on how to represent the accumulation of light. Photon mapping usually involves two passes, one to determine where the light hits and save those points. The second pass determines indirect lighting as seen from the camera, [^4]. The problems we noticed for this part were that our choice of using Unity and multiple camera angles did not make it easy to do the second pass, since that relies on a single viewing angle. 

Since our box has a 2D UV texture we wanted to make use of that datastructure in the first pass of the algorithm by saving all 2D coordinates and the number of photons that hit them. The standard datastructure for saving the points of light is a kd-tree, which allows for fast lookup of the nearest neightbour points [^4]. 

First we started with just drawing to the UV for every hit. 
<img width="650" alt="Skärmbild 2026-05-19 112620" src="https://github.com/user-attachments/assets/3a0ce8b0-0838-4ab6-b826-8c43c0a671ef" />

Then we identified for which pixels there was more than one hit.
<img width="650" alt="Skärmbild 2026-05-19 123740" src="https://github.com/user-attachments/assets/bfe7bd56-c502-4458-a508-9ed5b6101fd2" />

As the third step we scaled the brightness of the pixel according to how many hits were there. In order for us to see a clear difference we used a brightness of 0.1 + 0.4 * num_hits clamped to \[0,1].
<img width="650" alt="Skärmbild 2026-05-19 125006" src="https://github.com/user-attachments/assets/a60c6d12-040b-440f-9623-07fd27aae8a2" />


[^1]: https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-2-rendering-water-caustics

[^2]: https://www.researchgate.net/publication/6582416_Caustics_Mapping_An_Image-Space_Technique_for_Real-Time_Caustics

[^3]: https://sketchfab.com/3d-models/cornell-box-original-0d18de8d108c4c9cab1a4405698cc6b6

[^4]: https://pbr-book.org/3ed-2018/Light_Transport_III_Bidirectional_Methods/Stochastic_Progressive_Photon_Mapping
   
