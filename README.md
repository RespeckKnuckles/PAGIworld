# PAGIworld
Simulation environment for cognitive AIs.

5/30/18 AE - PAGI world has been updated to solve the following bugs:
	Fixed an issue where custom created items wont have an endorphin level.
	Fixed a bug where reporting endorphins is done through old AI messages instead of JSON
	Fixed a bug where customItems were not being saved properly in task files
	Fixed a bug where customItems were being loaded as bowling balls when loading a task file.
	Fixed a bug where backgrounded custom objects would sometimes appear in the foreground. 
	Fixed a bug where speech bubble text was drawn outside of speech bubbles.
	Fixes a bug where speech bubble position ignored position defined by the command.
	Fixed a bug where if an incorrect path to a custom object is given, a block will still be spawned.


3/20/18 AE - pyPAGI API has been updated and packaged in with the PAGI repository. It now contains functions
for all the PAGI-World commands, as well as a test script and template script.

2/26/18 AE - This updates the PAGI world environment to use JSON format for all messages going to and from Unity. The documentation has been updated to show the new format for these messages, and all commands have been tested.
