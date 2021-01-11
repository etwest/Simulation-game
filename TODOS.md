# TODOs
## Version 0
- [x] Create a per tile maximum for plant life... maybe a plant points system? like a grass is 1 point and a tree is 10? (still needs evaluation)
- [x] Add trees
- [ ] Change the grass sprite -- POSTPONED  
- [x] Trees and grass have a somewhat competetive relationship
- [x] New trees can cause grass to die
- [ ] Differentiate between tree saplings and full grown trees then grass reproducing has a chance to kill tree saplings   -- POSTPONED 
    
## Version 0.1
### UI                                                 
- [x] Add counts of the number of grasses, trees and animals which can be optionally displayed
- [x] Add a minimal user interface for accessing the menu and dispaying counts
### Animals
Bunnies to eat grass and foxes to eat the bunnies.
- [ ] Health
- [ ] Weather preference (heat/moisture)
- [ ] Rate of hunger and hydration drop
	* Have this be influenced by health and location                                    
- [ ] Movement
- [ ] Death
   * Corpse? Geez that's morbid
 - [ ] Spawning (either reproduction or wandering into the area)
- [ ] Goal choosing and path finding
- [ ] Collision rules (and collision components)
- [ ] Detect threats (bunnies run from foxes etc)
- [ ] Sprites
### Improve Initialization
- [ ] Run the simulation for 1000 or so cycles
	* Would probably work best just with the plants
	* The idea is to get a world which looks more real right from the get-go but not yet super established

### Small Stuff
- [x] Add a title screen / main menu
- [ ] Check the recycle queues and remove objects which have been there for too long
- [ ] Add collision components to every tree
- [ ] Change the Plant format to be OOP like the animal format
- [x] Add a pause button (space_bar)

## Version 0.2
### Update Terrain
- [ ] shallow water
- [ ] Rivers? as unique tile types. Rock w/ river, dirt w/ river, etc.
- [ ] Exposed rock v Mountains
	* Maybe exposed rock comes in as low moisture but near water while desert is only if we are further away from water.
	* Flat Dirt versus hills versus coast
	* Find a way to encourage rare larger groups of water as opposed to many scattered groups
- [ ] Create some way of visually indicating moisture values. Like for dirt we could color it darker or something and for rock we could... not sure.   

## Version 0.3
### Animals
Add more animals: deer, wolves, bears.
- [ ] Bears are a new type of animal omivore which has some unique rules
	* Can eat certain plants in addition to meat
	* Also gets class scavenger: does not attempt to chase food just looks to scavenge or to eat plants
- [ ] Rules for how different animals behave day vs night
### Desert
- [ ] Add desert plants -  cactus
- [ ] Add Desert animals - small nocturnal mice, snakes, camels
### Plants
- [ ] Add bushes
	* Some bushes have berries which can be eaten by plants
	* Add some way for deer to eat some portion of trees (maybe deal at most 50% damage
- [ ] Introduce a concept of damaging but not removing plants
### Environment
- [ ] Introduce the concept of day and night
- [ ] Graphical Representation

## Version 0.4
### Playable Character
- [ ] Health and hunger like animals
- [ ] To start acts like any other omnivore (bears)
- [ ] Maybe add some simple tool making (spears, baskets)
- [ ] Maybe consider simple farming as well

### Plants
- [ ] Add wheat as a different type of plant which is somewhat suseptable to takeover by grass if not in a large group? 
	* Make plants interactable by player
		* Cut grass/wheat
		* Cut down trees
	* Can be a source of food (wheat/berries)

## Version 0.5
- [ ] Begin adding human AIs
     * Can interact with environment in the same way as Playable character
     * Also add a trading/interaction system between character and AIs or between the AIs without the interaction of the player
Will need to give the AI a sense of what is valuable

## Version 0.6
- [ ] Upgrade graphics (Ideally have someone with talent help)

## Version 0.7
 - [ ] Add in buildings and trading systems
