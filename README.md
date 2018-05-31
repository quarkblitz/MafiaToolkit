# Mafia 2: Tools

This includes the following;
- Library to build your own programs
- Programs to read "Material" files and "FrameResource.bin"

This library has the ability to parse the following file types.
- FrameNameTable.bin
- FrameResource.bin (partially readable, unable to read certain object types and create certain 3D data)
- VertexBufferPool.bin
- IndexBufferPool.bin

It also packages functions to export 3D data to obj/mtl filetypes, or a custom file type, "EDM" and "EDD" built for this tool.

EDD = Used to view singular meshse. Contains mesh data.
EDM = Used to build the entire scene. Contains name of mesh, position and rotation 

Finally, a Maxscript and Blender addon have been built to enable the user to import these filetypes into 3DS Max and Blender. 
