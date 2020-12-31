using UnityEngine;
using System.Collections.Generic;

// This class is used to represent terrain tiles
// It keeps lists of the things upon it and pointers to its neighbors
// It also has a point value for the number of plants which can exist upon it
public class Tile {
    public GameObject obj;
    public Terrain type;
    public int weather;
    public int plant_points = 0;
    public int cur_plant = 0;
    public bool near_border = false;

    public List<Plant> grass_list = new List<Plant>();
    public List<Plant> tree_list = new List<Plant>();
    public List<Animal> animal_list = new List<Animal>();

    public Tile N = null;
    public Tile NW = null;
    public Tile SW = null;
    public Tile NE = null;
    public Tile SE = null;
    public Tile S = null;

    public Tile[] i_and_adj;

    // this function, given a game object returns the tile which is
    // closest to that game object out of this tile and it's neighbors
    public Tile TileClosestTo(GameObject item) {
        Vector3 item_pos = item.transform.position;
        Vector3 my_pos = obj.transform.position;
        // our distance to the item
        float dist = Object_Manager.CalcDistance(obj, item);

        float dist1;
        float dist2;
        Tile option1;
        Tile option2;

        // which tiles should we check
        if (item_pos.y > my_pos.y) {
            option1 = N;
            if (item_pos.x > my_pos.x) option2 = NE;
            else option2 = NW;
        }
        else {
            option1 = S;
            if (item_pos.x > my_pos.x) option2 = SE;
            else option2 = SW;
        }
        dist1 = Object_Manager.CalcDistance(option1.obj, item);
        dist2 = Object_Manager.CalcDistance(option2.obj, item);

        if (dist < dist1 && dist < dist2) return this;
        else if (dist1 < dist2) return option1;
        else return option2;
    }

    // returns the distance to the closest object which an object could collide
    // with
    // TODO Use collider objects instead
    public bool collision(GameObject from_obj) {
        PolygonCollider2D collider = from_obj.GetComponent<PolygonCollider2D>();
        // loop through the lists we have of what's on our tile
        // ignore grass because we don't collide with that
        foreach (Plant tree in tree_list) {
            if (tree.obj.GetComponent<PolygonCollider2D>().IsTouching(collider))
                return true;
        }
        foreach (Animal animal in animal_list) {
            if (animal.obj.GetComponent<PolygonCollider2D>().IsTouching(collider))
                return true;
        }
        return false;
    }

    // returns the nearest plant of a certain type
    // if a plant cannot be found then it returns null
    public Plant find_nearest(Plant_type type, GameObject toMe) {
        Plant ret = null;
        float dist = 100000;
        foreach (Tile tile in i_and_adj) {
            switch (type) {
                case Plant_type.Grass:
                    foreach (Plant grass in tile.grass_list) {
                        float temp = Object_Manager.CalcDistance(toMe, grass.obj);
                        if (temp < dist) {
                            dist = temp;
                            ret = grass;
                        }
                    }
                    break;
                case Plant_type.Tree:
                    foreach (Plant tree in tile.tree_list) {
                        float temp = Object_Manager.CalcDistance(toMe, tree.obj);
                        if (temp < dist) {
                            dist = temp;
                            ret = tree;
                        }
                    }
                    break;
            }
        }
        return ret;
    }

    public Animal find_nearest(Animal_type type, GameObject toMe) {
        Animal ret = null;
        float dist = 1 << 31;
        foreach (Tile tile in i_and_adj) {
            foreach (Animal animal in tile.animal_list) {
                if (animal.type == type) {
                    float temp = Object_Manager.CalcDistance(toMe, animal.obj);
                    if (temp < dist) {
                        dist = temp;
                        ret = animal;
                    }
                }
            }
        }
        return ret;
    }
}
