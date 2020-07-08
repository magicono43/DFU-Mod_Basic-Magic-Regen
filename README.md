# DFU-Mod_Basic-Magic-Regen
 Adds Very Basic Magic Regeneration System To Daggerfall Unity

Name: Basic Magic Regen
Author: Kirk.O
Special Thanks: Hazelnut, Ralzar, Jefetienne, and Interkarma
Released On: Version 1.00, Released on 7/8/2020, 11:00 AM
Version: 1.00

DESCRIPTION:
Adds a simple Passive Mana Regeneration system that effects the player. The type of regeneration can be toggled from either a flat amount based on the Willpower stat, or a 
percentage based on the Willpower stat. There are multiple options to increase or decrease the amount generated per "tick" as well as how often these regen "ticks" occur.

WHAT DOES THIS MOD CHANGE AND WHY?:
-It can be very difficult to play a "full" magic user build in Daggerfall. The main reason being that using magic can be very expensive in terms of dealing damage, and the player 
mana pool is only so large that most encounters will leave them without mana to continue fighting, so they have to switch back to being a melee fighter essentially most of the time. 
This mods aims to try and make a "full" magic user build more viable, by allowing mana to not just regenerate from resting, but passively even while not sleeping. In a similar vein 
to how passive mana regen works in Oblivion and Skyrim.

-It is extremely easy to customize the passive mana regeneration to your preference with the simple settings options available. If you want to have mana all the time, you can 
set it that way, if you just want the passive generation to be a small supplement to your resources, you can do that as well. Change the frequency of each regen "tick" and the 
amount generated each "tick" as well, all still based on the players Willpower stat. The Luck stat also has a small role as well.


SPOILERS/MORE DETAILS:
-Willpower is the main factor in determining how much mana regen will occur per "tick." However, every point of Willpower will have an effect, not just multiples of 10. 
Decimal values will be considered and have a 100 sided die roll every regen "tick." So 51 will = 10% chance of rolling a 6 on that "tick", where as 59 will = 90% chance 
of rolling a 6. This roll for the remainder values is modified further by the players LUCK stat. So higher luck will increase the odds of a higher regen "tick" value 
occurring, lower doing the opposite.

-If you want EXACT numbers and information on how some mechanics and formula work in this mod, under the hood. You can look at the source code yourself from the GitHub 
linked lower down. You can also email/post a thread on the parent forum post linked below and I would be happy to answer any questions.


OPTIONS:

	-Magic Regen Type: Toggle between "Flat" and "Percentage" based mana regen. Default 0 = Flat, 1 = Percentage.
	
	-Tick Regen Frequency: How often regen "ticks" occur. Can set from 1-6, 1 is default. 1 = ~5 seconds, 6 = ~30 seconds.
	
	-Regen Amount Modifier: Multiplier on final regen amount per "tick." Can set from 0.25 - 5, 1 is default.

VERSION HISTORY:

1.00 - Initial Release

COMPATIBILITY:
This mod should be completely compatible with all other mods out there.

INSTALLATION:
Unzip and open the folder that matches your operating system (Windows/OSX/Linux)

Copy the "basicmagicregen.dfmod" into your DaggerfallUnity_Data\StreamingAssets\Mods folder

Make sure the mod is enabled and "Mod system" is enabled in the starting menu under "Advanced -> Enhancements"

UNINSTALL:
Remove "basicmagicregen.dfmod" from the "StreamingAssets/Mods" folder.
