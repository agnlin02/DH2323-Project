1. abstract

2. Previous reserch (not for blog)

3. Progress updates and highlights
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

3.3 Calculated the angle for the raycast using snell's law. 
Created white dots where the ray cast hit the ground. This by coloring the texture. 
<img width="1876" height="911" alt="Skärmbild 2026-05-08 165908" src="https://github.com/user-attachments/assets/35284d79-8e00-48e8-aa21-7f011172fa78" />

The folowing screenshots are from scaling the water. As you can se the raycast creates 
<img width="2848" height="1049" alt="Skärmbild 2026-05-08 165455" src="https://github.com/user-attachments/assets/0e23a32a-ca30-4b0f-9343-7ab7b7a780e1" />
<img width="2820" height="1128" alt="Skärmbild 2026-05-08 165249" src="https://github.com/user-attachments/assets/d5c40677-6bc9-463f-bda9-21af48848ab3" />






4. Snell's law
   
