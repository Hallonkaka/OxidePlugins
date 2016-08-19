#OxidePlugins by Ron Dekker
OxidePlugins is my personal repository containing the source code of plugins using the [Oxide API](http://oxidemod.org/) for several games.

#License
All assets in the repository are released under the GNU GENERAL PUBLIC LICENSE (Version 3, 29 June 2007).

#Bugs, feature requests, or issues
Regarding bugs, feature requests, or other issues in and around the plugins please use the issues tab of this repository.

#Plugins
##Rust
###UiPlus
UiPlus is a plugin focusing on adding information for the user in a non-intrusive interface based on the interface by the developers already present in the game.

[Link to the page on OxideMod.org](http://oxidemod.org/plugins/uiplus.2088/).

####Images
![Close-up](http://imgur.com/qrXDviH.png)

####Features
The plugin adds the possibility for enabling several highly customizable panels:
* Active players
	* Able to reflect the current amount of players on the server, and the maximum amount of players able to join at once.
* Sleeping players
	* Able to reflect the current amount of players sleeping on the server.
* Clock
	* Able to reflect the current in-game time down to the seconds. It also allows to switch between a 12 hour and 24 hour format.

####Planned
Make sure the UI elements are only visible when the other elements are too. In other words have the elements removed when in menu, but on screen when walking around and in the inventory.

~~As of right now this plugin does not have any planned features.~~ However feel free to make suggestions using the issues tab of the repository as I am always interested in making improvements.

####Configuration
As mentioned earlier this plugin is aimed at being highly customizable. On installation it will create a configuration file when it is initialized and none is present yet. On updates of the plugin it might add new elements to the configuration file, in this case it will notify you by placing a warning in the console. It is however still recommended to remove the configuration file when updating it to a new version. Remove a part of the file within braces '{}' including them, will be reinitialized with the default the next time the plugin is loaded. This can be an easy way to reset only part of the configuration file.

Below there is the current default configuration file upon initialization. The first three lines are regarding which panels to enable. The several option below it are the individual settings of a panel. Finally there are several components laid out with all of its properties of each panel, please edit these with caution.

The RectTransform component allows control over the height, width, and positioning of each panel element. The first value is the x value or rather the distance from the left of the screen. the second value is the y value or rather the distance from the bottom of the screen.
* Anchor Min
	* The bottom left of the element.
* Anchor Max
	* The top right of the element.
* Offset
	* The offset of the element.

The Image component allows control over the image displayed and the color of it. An image can currently not be added to the background image components.
* Color
	* Color is separated into four values; Red, Green, Blue, and Alpha respectively.
* Uri
	* A Uniform Resource Identifier can be an URL linking directly to an images, as it is by default, or towards a file locally hosted. A URL has to start form http://, and a locally hosted file can start with file:///, or you can create a folder named after the plugin "UiPlus" in the data folder and add a png file to that. After which you only have to write the image name followed by .png. For example "clockIcon.png"

The Text component allows control over the label displayed per panel.
* Alignment
	* Alignment refers to where the anchor point of the labels is. The possible options are; LowerCenter, LowerLeft, LowerRight, MiddleCenter, MiddleLeft, MiddleRight, UpperCenter, UpperLeft, UpperRight.
* Color
	* Color is separated into four values; Red, Green, Blue, and Alpha respectively.
* Font
	* The font is the name of the font used for displaying the text.
* Font size
	* The size of the font when displayed.
* Format
	* The format in which the text will be displayed. Each possible field of each panels is used by default.
		* For the active panel this is {PLAYERSACTIVE}, and {PLAYERMAX}.
		* For the sleeping panel this is {PLAYERSSLEEPING}. 
		* For the clock panel this is {TIME}. 

```
{

  "__Create Active panel": true,
  
  "__Create Clock panel": true,
  
  "__Create Sleeping panel": true,
  
  "_Clock 24 hour format": true,
  
  "_Clock show seconds": false,
  
  "_Clock update frequency in milliseconds": 2000,
  
  "Active backgroundImage": {
    "Color": "1 0.95 0.875 0.025"
  },
  
  "Active backgroundRect": {
    "Anchor Max": "0.048 0.036",
    "Anchor Min": "0 0",
    "Offset": "81 72"
  },
  
  "Active iconImage": {
    "Color": "0.7 0.7 0.7 1",
    "Uri": "http://i.imgur.com/UY0y5ZI.png"
  },
  
  "Active iconRect": {
    "Anchor Max": "0.325 0.75",
    "Anchor Min": "0 0",
    "Offset": "2 3"
  },
  
  "Active textRect": {
    "Anchor Max": "1 1",
    "Anchor Min": "0 0",
    "Offset": "24 0"
  },
  
  "Active textText": {
    "Alignment": "MiddleLeft",
    "Color": "1 1 1 0.5",
    "Font": "RobotoCondensed-Bold.ttf",
    "Font size": 14,
    "Format": "{PLAYERSACTIVE}/{PLAYERMAX}"
  },
  
  "Clock backgroundImage": {
    "Color": "1 0.95 0.875 0.025"
  },
  
  "Clock backgroundRect": {
    "Anchor Max": "0.049 0.036",
    "Anchor Min": "0 0",
    "Offset": "16 72"
  },
  
  "Clock iconImage": {
    "Color": "0.7 0.7 0.7 1",
    "Uri": "http://i.imgur.com/CycsoyW.png"
  },
  
  "Clock iconRect": {
    "Anchor Max": "0.325 0.75",
    "Anchor Min": "0 0",
    "Offset": "2 3"
  },
  
  "Clock textRect": {
    "Anchor Max": "1 1",
    "Anchor Min": "0 0",
    "Offset": "24 0"
  },
  
  "Clock textText": {
    "Alignment": "MiddleLeft",
    "Color": "1 1 1 0.5",
    "Font": "RobotoCondensed-Bold.ttf",
    "Font size": 14,
    "Format": "{TIME}"
  },
  
  "Sleeping backgroundImage": {
    "Color": "1 0.95 0.875 0.025"
  },
  
  "Sleeping backgroundRect": {
    "Anchor Max": "0.049 0.036",
    "Anchor Min": "0 0",
    "Offset": "145 72"
  },
  
  "Sleeping iconImage": {
    "Color": "0.7 0.7 0.7 1",
    "Uri": "http://i.imgur.com/mvUBBOB.png"
  },
  
  "Sleeping iconRect": {
    "Anchor Max": "0.325 0.75",
    "Anchor Min": "0 0",
    "Offset": "2 3"
  },
  
  "Sleeping textRect": {
    "Anchor Max": "1 1",
    "Anchor Min": "0 0",
    "Offset": "24 0"
  },
  
  "Sleeping textText": {
    "Alignment": "MiddleLeft",
    "Color": "1 1 1 0.5",
    "Font": "RobotoCondensed-Bold.ttf",
    "Font size": 14,
    "Format": "{PLAYERSSLEEPING}"
  }
  
}
```
