# A Simulation in Unity
This project is currently very much in progress.

The goal is to develop a fully working simulation with plants and animals within Unity with the possibility of adding a controllable human character and turning the project into a survival game with systemic mechanics. Check `TODOS.md` for more information.

## Running
This codebase contains all the assets to create a Unity project. After downloading and installing Unity, the project can be imported and opened.
### Navigation
Once you open the project with Unity you can run the simulation by pressing the play button. The world is then generated and can be navigated using the arrow keys or WASD to move about and scrolling to zoom in/out.

## Contributions
All contributions are welcome. Branch from main with a meaningful branch name and make a pull request to get your changes incorporated.

I recommend you use Visual Studio with the Unity extension for writing code. Also, please remember to keep the code efficient and try to drive all execution from as few `Update()` functions as possible.

## Codebase
The c# files where most of the action happens are found in `Assets/scripts/`. `Object_Manager` is the 'main' file driving the execution of most of the code. For more details check the files, they should be well commented.
