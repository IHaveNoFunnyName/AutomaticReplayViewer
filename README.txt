AutomaticReplayViewer v1.33

Created by Mao (@NZMao)
InputManager.dll by shynet (https://www.codeproject.com/Articles/117657/InputManager-library-Track-user-input-and-simulate)

This program allows for sets of consecutive replays to be played back automatically.
It also has functionality for pressing hotkeys for starting and stopping recording software such as OBS.
The games supported in the current version are:
- Skullgirls
- Rivals of Aether

Before starting the viewing process, navigate to the first replay to be played.

Inputs:
- Start!
	- Starts the viewing process
- Stop
	- Stops the viewing process
- Number of replays
	- The number of replays that will be played
	- If the text inputted does not parse or is a number less than 1, this value will be set to 1 upon starting
- Start recording hotkey
	- An optional hotkey for the purpose of starting a recording of the set
	- Set this to whatever start recording hotkey you use with your software
- Stop recording hotkey
	- An optional hotkey for the purpose of stopping a recording of the set
	- Set this to whatever stop recording hotkey you use with your software
	- If either of these hotkeys are left blank or do not parse this functionality is disabled
- Display Hitboxes (SG only)
	- This will display in-game hitboxes during playback
- Display Inputs (SG only)
	- This will display both players inputs during playback
- Display Attack Data (SG only)
	- This will display attack data during playback
- Select Game
	- Sets the game for which replays are to be played
- More Settings...
	- Opens a menu where the default settings as well as the keybindings can be changed

Textboxes that contain keybindings maybe cleared by right-clicking on them

In order for the program to function correctly, the keybindings in the settings menu must match the keybindings in game.
The settings menu can be found in Select Game > More Settings...
The default settings for all the forms can be changed in the settings menu as well.

If at any point during the viewing process the current game is not open the viewing process will stop.