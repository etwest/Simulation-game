using UnityEngine;
using System;
using System.Collections.Generic;

// order here determines if one plant will be killed by
// another in order for the latter to reproduce
public enum Plant_type {
    Grass,
    Tree
}

public class Plant {
    public GameObject obj;
    public int bornAt;
    public int lastRepAt;
    public Tile tile;
    public int numKids = 0;
    public Plant_type type;
    public float size = 0f;

    private static System.Random rand = new System.Random();
    public static Stack<GameObject> recycle_grass = new Stack<GameObject>();
    public static Stack<GameObject> recycle_tree = new Stack<GameObject>();

    public static int num_grasses = 0;
    public static int num_trees = 0;
    public static int upper_grass = 0;
    public static int upper_trees = 0;

    // default reproductive and lifespan values
    // these will be edited depending upon the conditions
    // the editing formula is as follows we do direct multiplication for
    // lifespan and we multiply by the inverse for replication
    // * moisture / 200 (most dirt tiles will be less, some will be more)
    // * .5 for mountain and * .5 for desert (deserts will already be hit by moisture
    // next calculate how close we are to the nearest other plant
    // if less than 4 size away
    // * dist / 4 size (we punish them for being too close)
    // if more than 20 size from any other grass
    // * .75 (we punish them for being alone)
    public int ticksToRep;
    public int ticksToDie;
    public static Sprite grass_sprite;
    public static Sprite tree_sprite;
    public static int baseRepGrass;
    public static int baseDieGrass;
    public static int baseRepTree;
    public static int baseDieTree;
    public static int grass_cost;
    public static int tree_cost;

    // the size of the plants
    public static float grass_size = .01f;
    public static float tree_size = .04f;

    public Plant(GameObject _obj, Tile _tile, int clock, float closestPlant, Plant_type _type) {
        // modify the obj
        obj = _obj;
        obj.SetActive(true);
        type = _type;

        bornAt = clock;
        lastRepAt = bornAt;
        tile = _tile;

        switch (type) {
            case Plant_type.Grass:
                tile.grass_list.Add(this);
                size = grass_size;
                ticksToRep = baseRepGrass;
                ticksToDie = baseDieGrass;
                tile.cur_plant += grass_cost;
                break;
            case Plant_type.Tree:
                tile.tree_list.Add(this);
                ticksToRep = baseRepTree;
                ticksToDie = baseDieTree;
                size = tree_size;
                tile.cur_plant += tree_cost;
                break;
        }

        // modify ticksToRep and ticksToDie
        float moistureIndex = tile.weather / 200;
        moistureIndex = (moistureIndex < .5f) ? .5f : moistureIndex;
        float terrainIndex = (tile.type == Terrain.Dirt) ? 1f : 0.4f;
        // how close are we to the nearest hex in this tile?
        float distanceIndex = 1f;
        if (closestPlant < 5 * size)
            distanceIndex = closestPlant / (5 * size);
        else if (closestPlant > 20 * size)
            distanceIndex = 0.9f;

        // adjust the values
        ticksToRep = (int)(ticksToRep * (1 / (moistureIndex * terrainIndex * distanceIndex)));
        ticksToDie = (int)(ticksToDie * moistureIndex * terrainIndex * distanceIndex);

        // add random values to the ticksToDie and Rep to space out the updates
        // +/- 10% on each end
        //Debug.Log(string.Format("Created {0}. Time to Replicate = {1}", obj.name, ticksToRep));
        //Debug.Log(string.Format("Time to Die = {0}", ticksToDie));
        //Debug.Log(string.Format("Using factor: {0}", moistureIndex * terrainIndex * distanceIndex));
        int five_Rep = ticksToRep / 20 + 2;
        int five_Die = ticksToDie / 20 + 2;
        ticksToRep += rand.Next() % (five_Rep * 2 + 1) - five_Rep;
        ticksToDie += rand.Next() % (five_Die * 2 + 1) - five_Die;
        //Debug.Log(string.Format("Time to Replicate = {1}", obj.name, ticksToRep));
        //Debug.Log(string.Format("Time to Die = {0}", ticksToDie));
    }

    // deletes the gameObject associated with the specified plant
    public void removePlant() {
        switch (type) {
            case Plant_type.Grass:
                recycle_grass.Push(obj);
                tile.grass_list.Remove(this);
                tile.cur_plant -= grass_cost;
                num_grasses--;
                break;
            case Plant_type.Tree:
                recycle_tree.Push(obj);
                tile.tree_list.Remove(this);
                tile.cur_plant -= tree_cost;
                num_trees--;
                break;
        }
        obj.SetActive(false);
        obj = null;
    }

    // returns the offspring of a plant
    public Plant Reproduce(int clock) {
        //Debug.Log(string.Format("{0} reproducing", obj.name));
        lastRepAt = clock;
        // where will this new plant be? Dependent upon plant size
        float x_delta = (float)(rand.NextDouble() * size * 15) + (size * 2);
        float y_delta = (float)(rand.NextDouble() * size * 15) + (size * 2);

        // max values of deltas to ensure that we don't go more than one hex
        x_delta = (x_delta > .14f) ? .14f : x_delta;
        y_delta = (y_delta > .14f) ? .14f : y_delta;

        // flip the x y to negative on random chance
        if (rand.Next() % 2 == 1)
            x_delta *= -1;
        if (rand.Next() % 2 == 1)
            y_delta *= -1;

        Vector3 position = obj.transform.position + new Vector3(x_delta, y_delta, 0);
        float[] delta = { x_delta, y_delta };
        return CreatePlant(clock, position, delta, tile, type);
    }

    public static void Reset() {
        recycle_grass.Clear();
        recycle_tree.Clear();
        num_grasses = 0;
        num_trees = 0;
        upper_grass = 0;
        upper_trees = 0;
    }

    // creates a plant using plant constructor and preforms a variety of
    // checks to ensure that the plant is valid
    public static Plant CreatePlant(int clock, Vector3 position, float[] delta, Tile tile, Plant_type type) {
        Sprite sprite;
        float size;
        string name;
        Stack<GameObject> recycler;
        int cost;
        switch (type) {
            case Plant_type.Grass:
                sprite = grass_sprite;
                size = grass_size;
                recycler = recycle_grass;
                cost = grass_cost;
                name = string.Format("grass_{0}", upper_grass);
                break;
            case Plant_type.Tree:
                sprite = tree_sprite;
                size = tree_size;
                recycler = recycle_tree;
                cost = tree_cost;
                name = string.Format("tree_{0}", upper_trees);
                break;
            default:
                sprite = grass_sprite;
                size = grass_size;
                recycler = recycle_grass;
                cost = grass_cost;
                name = string.Format("grass_{0}", upper_grass);
                break;
        }

        GameObject new_plant_obj;
        // first check if there is a GameObject for recycling
        // if not create a new one and increment our counter
        if (recycler.Count > 0) {
            new_plant_obj = recycler.Pop();
        }
        else {
            switch (type) {
                case Plant_type.Grass:
                    upper_grass += 1;
                    break;
                case Plant_type.Tree:
                    upper_trees += 1;
                    break;
            }
            new_plant_obj = new GameObject();
            new_plant_obj.name = name;
            SpriteRenderer sr = new_plant_obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Plants";
        }

        new_plant_obj.transform.position = position;

        // 2. calculate whether we are in tile or one of its neighbors
        Tile on_tile = tile.TileClosestTo(new_plant_obj);

        // number of points we're short in order to make this plant
        int points_deficit = on_tile.cur_plant + cost - on_tile.plant_points;
        int can_replace = 0;

        if (type > Plant_type.Grass) {
            can_replace += on_tile.grass_list.Count * grass_cost;
        }
        if (type > Plant_type.Tree) {
            can_replace += on_tile.tree_list.Count * tree_cost;
        }

        //if (type == Plant_type.Tree) {
        //Debug.Log(string.Format("Creating plant on {0} with {1}/{2}, deficit {3} - found {4} to kill", on_tile.obj.name, on_tile.cur_plant, on_tile.plant_points, points_deficit, can_replace));
        //}
        // if wrong tile type then failed to create plant
        if (points_deficit - can_replace > 0 || on_tile.type == Terrain.Water || on_tile.type == Terrain.Border) {
            new_plant_obj.SetActive(false);
            recycler.Push(new_plant_obj); // for reuse later
            return null;
        }

        // 3. use the hex to get a list of objects on that hex
        // and then calculate the positions of the objects on
        // that hex and ensure we don't collide.
        // check grass
        float min_dist = 100000f;
        for (int i = 0; i < on_tile.grass_list.Count; i++) {
            float dist = Object_Manager.CalcDistance(on_tile.grass_list[i].obj, new_plant_obj) - grass_size;
            // if we are a more successful plant then we kill the plants that are too close to us
            if (type > Plant_type.Grass && dist <= size) {
                //Debug.Log(string.Format("deleting  grass {0} because it was {1} away which is less than {2}", on_tile.grass_list[i].obj.name, dist, size));
                on_tile.grass_list[i].removePlant();
                points_deficit -= grass_cost;
            }
            else if (dist < min_dist) min_dist = dist;
        }
        // check trees
        for (int i = 0; i < on_tile.tree_list.Count; i++) {
            float dist = Object_Manager.CalcDistance(on_tile.tree_list[i].obj, new_plant_obj) - tree_size;
            // if we are a more successful plant then we kill the plants that are too close to us
            if (type > Plant_type.Tree && dist <= size) {
                //Debug.Log(string.Format("deleting  tree {0} because it was {1} away which is less than {2}", on_tile.tree_list[i].obj.name, dist, size));
                on_tile.tree_list[i].removePlant();
                points_deficit -= tree_cost;
            }
            else if (dist < min_dist) min_dist = dist;
        }

        // If we couldn't find enough points for the plant or it is too close to
        // another then fail to make a plant
        if (min_dist <= size) {
            new_plant_obj.SetActive(false);
            recycler.Push(new_plant_obj); // for reuse later
            return null;
        }

        // delete the plants which we are replacing
        // this needs to be done in order of least point value to most
        if (points_deficit > 0) {
            for (int i = Enum.GetNames(typeof(Plant_type)).Length - 1; i >= 0; i--) {
                if (type > (Plant_type)i) {
                    int value = -1;
                    List<Plant> delete = null;
                    switch ((Plant_type)i) {
                        case Plant_type.Grass:
                            //Debug.Log("checking grass");
                            value = grass_cost;
                            delete = on_tile.grass_list;
                            break;
                        case Plant_type.Tree:
                            //Debug.Log("checking trees");
                            value = tree_cost;
                            delete = on_tile.tree_list;
                            break;
                    }
                    while (delete.Count > 0 && value <= points_deficit) {
                        //Debug.Log(string.Format("deleting {0}", delete[0].obj.name));
                        delete[0].removePlant();
                        points_deficit -= value;
                    }
                }
            }
        }

        // return a new plant object at the location
        // succeeded in creating plant
        switch (type) {
            case Plant_type.Grass:
                num_grasses++;
                break;
            case Plant_type.Tree:
                num_trees++;
                break;
        }
        return new Plant(new_plant_obj, on_tile, clock, min_dist, type);
    }
}
