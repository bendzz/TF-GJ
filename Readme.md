PLAYING THE GAME:

-Builds.zip has the most recent build and readme. Otherwise, execute the code with Unity 2021.1.15f1



USING THE GAME ASSETS:

-Model:

-girl3e_.blend is the most recent iteration. (TODO, delete the old files)

-Blender 3.2 is the version used to work on it.

-(The topology is crude but it'll work for jams)

-Weight painting is broken

	-The symmetry and auto normalize functions add random weights to the other side of the body, making even a simple tweak corrupt everything. Use auto weights. (But you have to detatch the hairHEAD, tail and ears sections into a separate armature so they don't weight the body).
	
	- https://discord.com/channels/217710636702892032/552308534906454017/986963053537021952 
	
		-(Bone groups can only be selected in pose mode)
		
		-(The selection will ignore the headHAIR bone since it's hidden in pose mode; select it in edit mode before shifting that bone group)
		
		-(If you forget to select headHAIR and can't line up that with the head bone to reposition the bones after auto weights, you can still line up the eye bones with the eyeball mesh centers)
		
-When importing to unity: turn off Resample Curves and Anim. Compression in the importer-animation settings, or the animation can get buggered up.


TF:

-Just scale the ear and tail bones

-Grow the fur in with a gradient painted onto a texture, like "TF Pattern.exr" image. (PNG doesn't have enough color resolution, looks awful). Needs a custom shader and math.

