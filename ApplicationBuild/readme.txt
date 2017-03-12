EXPERIMENTAL GENETIC ALGORITHM APPLICATION TO A MAZE-CHASE GAME
Author: Petru Botnar petru.botnar.1@city.ac.uk

Running the application: (windows_x86)
-Open MazeChaseGA.exe
-Preferred screen resolution: 1360x768
-Graphics quality: selecting a higher resolution will also slow down the agents movements
-Press play
-MAKE SURE THERE IS A SETTINGS.JSON FILE INSIDE "MazeChaseGA_Data" OTHERWISE THE APPLICATION WILL NOT PLAY;

Commands;
-Press Play to start;
-Press Stop to pause the iteration;
-Press Reset to skip to the next gene;
-Press key button "S" to skip 5 generations;

After each evolution the average of the fitness value will be written on a text file inside MazeChaseGA_Data folder:
- Pacman values : "MazeChaseGA_Data/pacman_averages.txt"
- Ghost values : "MazeChaseGA_Data/ghost_averages.txt"


How set up the properties of the application:

Change the values whithin the file "MazeChaseGA_Data/settings.json" (or create a new settings.json file)

Here is an example:

{
  "Population": 10, 
  "GeneSize": 6,
  "Evolutions": 30,
  "Mutation": true,
  "RandomFruitPositions": false,
  "Visual": true,
  "Logs": true
}

Population = population size used for each generation

GeneSize = the number of actions the agent will loop over defining its behaviour

Evolutions = set the limit of evolutions in case Visual is set to false
 (will decouple rendering from the application and output results when limit is reached)
 
 Mutation: true or false; set whether mutation is to be applied
 
 RandomFruitPositions: if true, fruit position will be placed randomly at each iteration
 
 Visual: when set to false the algorithm will run using CPU speed rather than rendering, useful to test the algorithm over many evolutions
 
 Logs: enables or disables the logs whithin the application

