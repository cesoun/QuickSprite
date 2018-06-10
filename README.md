# Quick Sprite

### About

`QuickSprite` is a tool designed for Graal. Specifically for those still fond of the offline editor `GraalShop.exe`. This brings the online editor sprite selector to the offline editor in a way. Although not exactly the same it helps cut down on the time that would have been spent selecting sprites. 

### Application Breakdown

![QuickSprite](https://i.imgur.com/4mgvRuG.png)

`Loaded Image`
Displays the currently loaded file. Clicking this will unload the file and reset the views.

`Detected Sprites`
Displays the individual sprites that have been detected by the SpriteCutter. These correspond the `Pan & Zoom Preview` rectangles. Left Clicking any of these will remove them from the final output and update in the `Pan & Zoom Preview`.

`Precision Slider`
Adjust how big or small you want the max sprite to be. Increasing this will in some cases remove smaller sprites but merge others that would normally overlap.

`Copy to Clipboard`
Copies the output to your clipboard for direct pasting rather than file saving.

`Save to File`
Saves the output to a file.

`Pan & Zoom Preview`
Displays the detected sprites with highlighted rectangles. Zooming is done via the mouse wheel & Panning is done via holding right click and moving the mouse.

### Usage

8-Bit images are the best way to go. Keep a transparent background and avoid shades of black & true black (#000000) as much as possible. In some cases adjusting the blacks to a bright color like red or green can help with identifying sprites. Looking to implement an automatic function for this but it might be a while before it happens. 

Dragging and dropping anywhere on the application will attempt to load the image. Upon success it will then begin identifying the sprites as best as possible. The main goal being to reduce as much time that would be spent selecting sprites manually. It's not perfect but a step up so work with me a little here, thanks. 

Increasing the precision will adjust the min-max for a sprite size. This can be helpful when sprites have spaces and you want to try and include them. It works primarly when that sprite is identified and eventually intersects another sprites selection. This will merge them into a single sprite for the final output. 

Once happy with all your sprite definitions choosing to `copy` or `save` the file will then let you copy and or paste the text into an exsisting gani. The starting index is currently set to always being at 400. Once pasted simply save and open the gani file to see all your newly added sprites!