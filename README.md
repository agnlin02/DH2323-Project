# Water Caustincs uisng Snell's law and Photon Mapping

### New unityproject, raycast and linerender (3 May)
Initiated a project in uinty. Added a water mesh at top of a simple room, made up of planes and cubes. A light was added to the scene with a script. Using the script a raycast was shooting from the light and as it hit the water a linerender was created. Next a raycast was created in the direction of the normal of the hitpoint.
<img width="1768" height="645" alt="image" src="https://github.com/user-attachments/assets/87228f37-bda6-41a4-b681-cc7ac582c2e3" />


### First caustics (9 May)
The angle of the new raycast from the surface was calculate using an approximate form of Snell's law found in [^1]. Once it hit the ground white dots appear where the raycast collide with the ground. 

<img width="1876" height="911" alt="Skärmbild 2026-05-08 165908" src="https://github.com/user-attachments/assets/35284d79-8e00-48e8-aa21-7f011172fa78" />

The following screenshots are from scaling the water. 
<img width="2848" height="1049" alt="Skärmbild 2026-05-08 165455" src="https://github.com/user-attachments/assets/0e23a32a-ca30-4b0f-9343-7ab7b7a780e1" />
<img width="2820" height="1128" alt="Skärmbild 2026-05-08 165249" src="https://github.com/user-attachments/assets/d5c40677-6bc9-463f-bda9-21af48848ab3" />

### Changing scenario (10 May)
From looking at the scenarios in several papers we found that figure 8 in [^2] could work well. So we downloaded a model of a Cornell box [^3] from the internet and created a water surface mesh of a circular sine curve to mimic a droplet disturbing the surface. 

### Painting textures (19 May)
With the new models we could start experimenting on how to represent the accumulation of light. Photon mapping usually involves two passes, one to determine where the light hits and save those points. The second pass determines indirect lighting as seen from the camera, [^4]. Since our box has a 2D UV texture we wanted to make use of that in the first pass of the algorithm by saving all 2D coordinates and the number of photons that hit them. The standard datastructure for saving 

The problems we noticed for this part were that our choice of using Unity and multiple camera angles did not make it easy to do the second pass, since that relies on the viewing angle. 


[^1]: https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-2-rendering-water-caustics

[^2]: https://www.researchgate.net/publication/6582416_Caustics_Mapping_An_Image-Space_Technique_for_Real-Time_Caustics

[^3]: https://sketchfab.com/3d-models/cornell-box-original-0d18de8d108c4c9cab1a4405698cc6b6

[^4]: https://pbr-book.org/3ed-2018/Light_Transport_III_Bidirectional_Methods/Stochastic_Progressive_Photon_Mapping
   
