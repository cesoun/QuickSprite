# Quick Sprite

### About

`QuickSprite` is a tool designed for Graal. Specifically for those still fond of the offline editor `GraalShop.exe`. This brings the online editor sprite selector to the offline editor in a way. Although not exactly the same it helps cut down on the time that would have been spent selecting sprites. 


### Application Breakdown

Top left box is the loaded `image preview`. Clicking it will reset the forms.

Below this is the `scroll view` which shows all sprites gathered from the sprite cutter. Left clicking these will remove them.

Below this is the `save sprites` button. Click this will ask for a save location and let you save the file, yay.

Below this is the `copy sprites` button. This will copy the sprites to your clipboard and allow you to paste instead of save.

The main window to the right lets you `pan & zoom` the sprites that get cut from the main image. Right click pans, scrolling in and out zooms.

### How To Use

Drag & Drop any `.png` file with a bit depth of `8 bits`. This is due to the fact that the offline editor uses images with bit depth of 8. Also because the blob detection doesn't see anything else. unlucky.

Click any of the images within the `scroll view` to remove in the case you don't need any.

Finally, click the `save file` button and export it to wherever you want. Open that file and copy the text into a `.gani` file. Alternatively you can click `copy sprites` and paste them directly from your clipboard.

[![Video Link](https://img.youtube.com/vi/dyXG-uzXw4Y/0.jpg)](https://www.youtube.com/watch?v=dyXG-uzXw4Y)
