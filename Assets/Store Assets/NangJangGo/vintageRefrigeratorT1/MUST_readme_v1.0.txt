
** vintage_refrigerator type 1 **
Nov, 2018. ver 1.0

-------------
1. mesh model

/ in [mesh] folder,
3d objects are grouped and set child
"mesh_vintage_RefrigeratorT1"
 : mesh_vintage_RefrigeratorT1_body
 : mesh_vintage_RefrigeratorT1_doorFreezer (can be opened and closed)
 : mesh_vintage_RefrigeratorT1_doorFridge (can be opened and closed)

Try to rotate each door, and you can put any objects inside of refrigerator.
I added Pointlight inside of fridge, and I saved as prefab. You can use it rightway or change any options what I set.

** IMPORTANT **
For people who want the mesh set to 'STATIC", I strongly recommend TURN "UV Charting Control / Optimize Realtime UVs" OFF from mesh (especially "mesh_vintage_RefrigeratorT1_body") before use the mesh from the project folder, because realtime shadow isn't working well when the option is "on".

-------------------------
2. materials and textures

/ in [texture] folder,
There are four texture files included:
- diffuse : tex_vintage_RefrigeT1_diffuse.png
- diffuse : tex_vintage_RefrigeT1_green_diffuse.png
- metal : tex_vintage_RefrigeT1_metal.png
- PSD : tex_vintage_RefrigeT1.psd

in sample project, I used default Unity standard shader. it looks ok when only using diffuse texture, but I added metal texture for just in case.

Also, I put an original PSD file, you can do anything you want, for example, change color or adding dirt or rust on the case of refrigerator. "tex_vintage_RefrigeratorT1_green_diffuse.png" is a sample texture, you can check it on sample project file.


I'm always accepting any questions and suggestions.
Thank you and good luck to you and your project.

- openplay -
