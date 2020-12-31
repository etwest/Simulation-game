using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Object_Manager : MonoBehaviour {
    public Create_Terrain create_terrain;
    public Text Count_Display;
    public Sprite grass;
    public Sprite tree;
    public Sprite bunny;

    // counts of initial objects
    public int initial_grass = 100;
    public int initial_trees = 10;

    public int grassToRep = 100;
    public int grassToDie = 1000;

    public int treeToRep = 250;
    public int treeToDie = 10000;

    public int grass_cost = 1;
    public int tree_cost = 5;

    // List of animals
    List<Animal> animals = new List<Animal>();
    List<Animal> animals_copy = new List<Animal>(); //backup list for deleting from

    // dictionary to track the events we need to manage
    Dictionary<int, HashSet<Plant>> update_grass =
        new Dictionary<int, HashSet<Plant>>();

    Dictionary<int, HashSet<Plant>> update_tree =
        new Dictionary<int, HashSet<Plant>>();

    // lists which we walk when considering plants
    // every plant needs to be in this list
    int num_plants = Enum.GetNames(typeof(Plant_type)).Length;
    Dictionary<int, HashSet<Plant>>[] updaters;
    int[] initial_counts;

    // the 'clock' we will use to manage these objects
    private int clock = 0;
    private bool paused = false;

    private System.Random rand;
    // our own copies of create_terrain's lists
    private Tile[] dirt_tiles;
    private Tile[] rock_tiles;
    private Tile[] water_tiles;
    private Tile[] desert_tiles;

    private void Start() {
        create_terrain.createTerrain();
        dirt_tiles = create_terrain.dirt_tiles;
        rock_tiles = create_terrain.rock_tiles;
        water_tiles = create_terrain.water_tiles;
        desert_tiles = create_terrain.desert_tiles;
        Initialize();
    }

    // function that calculates the distance between two game objects
    public static float CalcDistance(GameObject first, GameObject second) {
        Vector3 position1 = first.transform.position;
        Vector3 position2 = second.transform.position;

        float x_comb = position1.x - position2.x;
        float y_comb = position1.y - position2.y;

        return Mathf.Abs(Mathf.Sqrt(x_comb * x_comb + y_comb * y_comb));
    }

    // set up the initial plant objects
    public void Initialize() {
        // set up
        rand = new System.Random(create_terrain.seed);

        updaters = new Dictionary<int, HashSet<Plant>>[num_plants];
        updaters[(int)Plant_type.Grass] = update_grass;
        updaters[(int)Plant_type.Tree] = update_tree;

        initial_counts = new int[num_plants];
        initial_counts[(int)Plant_type.Grass] = initial_grass;
        initial_counts[(int)Plant_type.Tree] = initial_trees;

        Plant.Reset();

        Plant.grass_sprite = grass;
        Plant.tree_sprite = tree;
        Plant.baseRepGrass = grassToRep;
        Plant.baseDieGrass = grassToDie;
        Plant.baseRepTree = treeToRep;
        Plant.baseDieTree = treeToDie;
        Plant.grass_cost = grass_cost;
        Plant.tree_cost = tree_cost;

        Bunny.sprite = bunny;
        Queue<Tuple<Plant, int>> new_plants = new Queue<Tuple<Plant, int>>();

        // create plants
        for (int plant = 0; plant < num_plants; plant++) {
            Plant_type type = (Plant_type)plant;
            Dictionary<int, HashSet<Plant>> update = updaters[plant];
            int initial = initial_counts[plant];
            for (int i = 0; i < initial; i++) {
                // choose random dirt hex
                Tile tile = dirt_tiles[rand.Next() % dirt_tiles.Length];
                Vector3 position = tile.obj.transform.position;

                // create a plant and place it at a random position
                // within this tile
                float x = (float)(rand.NextDouble() * 0.25);
                float y = (float)(rand.NextDouble() * 0.25);

                // flip the x y to negative on random chance
                if (rand.Next() % 2 == 1)
                    x *= -1;
                if (rand.Next() % 2 == 1)
                    y *= -1;

                float[] delta = { x, y };

                x += position.x;
                y += position.y;

                Vector3 new_position = new Vector3(x, y, 0);
                Plant new_plant = Plant.CreatePlant(0, new_position, delta, tile, type);

                if (new_plant != null) {
                    if(new_plant.obj == null) {
                        Debug.Log("Case where just created plant has null obj");
                    }
                    else {
                        new_plants.Enqueue(new Tuple<Plant, int>(new_plant, 0));
                    }
                }
            }
        }
        // run the plants for awhile to have them reproduce
        while(new_plants.Count > 0) {
            Debug.Log(new_plants.Count);
            Tuple<Plant, int> cur = new_plants.Dequeue();
            if(cur.Item1.obj == null) {
                Debug.Log(string.Format("Error: null obj in queue count = {0}", cur.Item2));
                continue;
            }
            //Debug.Log(string.Format("pulled {0} {1} off the queue", cur.Item1.obj.name, cur.Item2));
            if (cur.Item2 > 1) {
                // modify this plant's time to live
                cur.Item1.ticksToDie -= cur.Item1.ticksToRep;
                if(cur.Item1.ticksToDie <= 0) {
                    Debug.Log("ticks to die less than ticks to rep");
                    cur.Item1.removePlant();
                }
                else {
                    // reproduce this plant
                    Plant rep_plant = cur.Item1.Reproduce(0); // use 0 as clock time
                    if (rep_plant != null) {
                        Debug.Log("enqueue rep plant");
                        new_plants.Enqueue(new Tuple<Plant, int>(rep_plant, cur.Item2 - 1));
                    }
                    // enqueue this plant
                    Debug.Log("enqueue this plant");
                    new_plants.Enqueue(new Tuple<Plant, int>(cur.Item1, cur.Item2 - 1));
                }
            }
            else {
                // setup the updaters for this plant
                Plant new_plant = cur.Item1;
                Dictionary<int, HashSet<Plant>> update = updaters[(int)new_plant.type];
                if (!update.ContainsKey(clock + new_plant.ticksToRep))
                    update[clock + new_plant.ticksToRep] = new HashSet<Plant>();

                update[clock + new_plant.ticksToRep].Add(new_plant);

                if (!update.ContainsKey(clock + new_plant.ticksToDie))
                    update[clock + new_plant.ticksToDie] = new HashSet<Plant>();
                update[clock + new_plant.ticksToDie].Add(new_plant);
            }
        }

        // create animals
        Tile _tile = dirt_tiles[rand.Next() % dirt_tiles.Length];
        Vector3 _position = _tile.obj.transform.position;
        float _x = (float)(rand.NextDouble() * 0.25);
        float _y = (float)(rand.NextDouble() * 0.25);

        _x += _position.x;
        _y += _position.y;
        Vector3 _new_position = new Vector3(_x, _y, 0);
        GameObject new_bunny = new GameObject();
        new_bunny.transform.position = _new_position;

        animals.Add(new Bunny(new_bunny, _tile));

        animals_copy = animals;
    }

    // Update the objects to reproduce, move, etc.
    void Update() {
        // check if spacebar has been clicked
        // this pauses the game
        if (Input.GetKeyDown(KeyCode.Space)) {
            paused = paused ? false : true;
        }
        if(paused) {
            return;
        }
        clock += 1;
        if (clock % 30 == 0) {
            Count_Display.text = string.Format("Counts: grasses {0} | trees {1} | bunnies {2} | foxes {3}",
                Plant.num_grasses, Plant.num_trees, Animal.num_bunnies, Animal.num_foxes);
        }

        // run through all the plants
        for (int plant_num = 0; plant_num < num_plants; plant_num++) {
            Dictionary<int, HashSet<Plant>> update = updaters[plant_num];
            if (update.ContainsKey(clock)) {
                // loop through all the grasses at this update point
                HashSet<Plant> plants = update[clock];
                foreach (Plant plant in plants) {
                    // only do stuff if this plant hasn't died
                    if (plant.obj != null) {
                        // should this plant die
                        if (clock - plant.bornAt >= plant.ticksToDie) {
                            //Debug.Log(string.Format("Deleting {0}", grass.obj.name));
                            plant.removePlant();
                        }
                        // should this plant reproduce
                        else if (clock - plant.lastRepAt >= plant.ticksToRep) {
                            // add the next time we should check this plant
                            if (!update.ContainsKey(clock + plant.ticksToRep))
                                update[clock + plant.ticksToRep] = new HashSet<Plant>();
                            update[clock + plant.ticksToRep].Add(plant);

                            // attempt to do the reproduction
                            Plant new_plant = plant.Reproduce(clock);
                            if (new_plant != null) {
                                if (!update.ContainsKey(clock + new_plant.ticksToRep))
                                    update[clock + new_plant.ticksToRep] = new HashSet<Plant>();
                                update[clock + new_plant.ticksToRep].Add(new_plant);

                                if (!update.ContainsKey(clock + new_plant.ticksToDie))
                                    update[clock + new_plant.ticksToDie] = new HashSet<Plant>();
                                update[clock + new_plant.ticksToDie].Add(new_plant);
                            }
                        }
                    }
                }
            }
        }

        // run through all the animals
        foreach(Animal animal in animals) {
            if(animal.obj == null) {
                animals_copy.Remove(animal);
                continue;
            }
            animal.update();
        }
        animals = animals_copy;
    }
}
