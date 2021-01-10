# TODOs
## Version 0
1. Create a per tile maximum for plant life... maybe a plant points system? like
   a grass is 1 point and a tree is 10?                                                         -- Somewhat done, needs evaluation

2.  Add trees                                                                                   -- done
    
    Change the grass sprite                                                                     -- POSTPONED
    
    Trees and grass have a somewhat competetive relationship                                    -- done
    
    New trees can cause grass to die                                                            -- done
    
    Differentiate between tree saplings and full grown trees then grass reproducing has a       -- POSTPONED
        chance to kill tree saplings       
    
## Version 0.1

3.  Create some way of visually indicating moisture values. Like for dirt we                    
        could color it darker or something and for rock we could... not sure.                   -- NOT DONE                                                      

4.  Add a title screen / main menu with resolution options                                      -- done
    
    Add a pause button (space_bar)                                                              -- done
    
    Add counts of the number of grasses, trees and animals which can be optionally displayed    -- done
    
    Add a minimal user interface for accessing the menu and dispaying counts                    -- done
    
    Add animals: bunnies and foxes                                                              -- 
    
    Animals will need:
        - Health                                                                                --
        - Weather preference (heat/moisture)                                                    --
        - Rate of hunger and hydration drop                                                     --
            - Have this be influenced by health and location                                    --
        - Movement                                                                              --
        - Death                                                                                 --
        - Spawning (either reproduction or wandering into the area)                             --
        - Goal choosing and path finding                                                        --
        - Collision rules (and collision components)                                            --
        - Detect threats (bunnies run from foxes etc)                                           --
        - Sprites                                                                               --

    Improve Initialization
        - Run the simulation for 1000 or so cycles                                              --

    Small Stuff
        - Check the recycle queues and remove objects which have been there for too long        --
        - Add collision components to every tree                                                --
        - Change the Plant format to be OOP like the animal format                              --

## Version 0.2

5.  Update Terrain                                                                              --
        - shallow water
            - Rivers? as unique tile types. Rock w/ river, dirt w/ river, etc.
        - Exposed rock v Mountains.                                                             --
            Maybe exposed rock comes in as low moisture but near water
            while desert is only if we are further away from water.
        - Flat Dirt versus hills versus coast                                                   --
        - Find a way to encourage rare larger groups of water                                   --
            as opposed to many scattered groups

## Version 0.3
6.  Add more animals: deer, wolves, bears.                                                      --
    
    Add desert plants/animals such as cactus and camel                                          --

7. Add berries and such for the bears to eat in addition to meat                                --

## Version 0.4
8. Add a playable character                                                                     --
   
   Features:
     - Health and hunger like animals                                                           --
     - To start acts like any other omnivore (bears)                                            --
     - Maybe add some simple tool making (spears, baskets)                                      --
     - Maybe consider simple farming as well                                                    --

    8b. Add wheat as a different type of plant which is somewhat suseptable to                  --
       takeover by grass if not in a large group? Can be a source of food                       --

## Version 0.5
9. Begin initial testing
    
    Feedback:
        -
    Changes to be made
        -

10. Begin adding human AIs                                                                      --
   
   Features:
     - Can interact with environment in the same way as Playable character                      --
     - Also add a trading/interaction system between character and AIs or                       --
       between the AIs without the interaction of the player
       - Will need to give the AI a sense of what is valuable                                   --

## Version 0.6
11. Upgrade graphics (Ideally have someone with talent help)                                    --

## Version 0.7
12. Add in buildings and trading systems                                                        --
