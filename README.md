1. Introduction

2. Old reserch

3. the progres
3.1 Created a unityproject and first script
Added water meshes
Created a light
Added script to ligt (photonMapping.cs).
Created a raycast from ligt to surface. Created a line render that follows the raycast.
Created a line in the direction of the normal of the surface of the hitpoint
<img width="1768" height="645" alt="image" src="https://github.com/user-attachments/assets/87228f37-bda6-41a4-b681-cc7ac582c2e3" />


3.2 
I changed the line to a raycast that shoots from the water surface and hits the bottom of the ground. Once it hit the bottom a white dot is created. This is done by creating a white sphere where the ray hits the ground. In order for the ray to just hit the ground and not the water surface I created a layermask on the ground. Now when you move the light around dots apear on the ground in the direction of the normal of the water surface.
<img width="891" height="706" alt="image" src="https://github.com/user-attachments/assets/d2600240-b3c5-459f-beac-4924f6690c87" />
