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

    // these public elements denote us and our neighbors
    public Tile N = null;
    public Tile NW = null;
    public Tile SW = null;
    public Tile NE = null;
    public Tile SE = null;
    public Tile S = null;
    public Tile[] i_and_adj;

    // static items. Size of a tile and 2D arrangement of tiles
    public static float r;
    public static float R;
    public static Tile[,] tiles_array;

    private static float pixel = .01f;

    // this function, given a game object returns the tile which is
    public Tile TileClosestTo(GameObject item) {
        return TileClosestTo(item.transform.position);
    }

    public static Tile TileClosestTo(Vector3 position) {
        // get the bottom left corner. This is tiles_array[0,0]
        Vector3 start = tiles_array[0,0].obj.transform.position;
        float start_x = start.x - R + pixel;
        float start_y = start.y - r;
        // recall the image of tile layout as described in Create_Terrain
        // x x x x
        //  x x x
        // This is one row. Therefore, rows seperated by 2r. Columns seperated
        // by 3R / 2 again see Create_Terrain for more explanation
        float x_diff = position.x - start_x;
        float y_diff = position.y - start_y;

        // get row/col of the closest tile in the tiles_array
        float x_col_ratio = x_diff / (3 * R - pixel) * 2;
        float y_row_ratio = y_diff / (2 * r - pixel);

        int x_col = (int) x_col_ratio;

        // Edge case catcher: if we are in the furthest right portion of
        // the hex then we register as the next col over (ie x is +1)
        // Check will catch an error about half the time its called
        float x_left_over = x_col_ratio - x_col;
        if (x_left_over <= .25f) {
            //Debug.Log("Checking edge case");
            // To address when the edge case is occurring in the lower part of a row
            // do this with local to avoid messing up later calculations
            float y_modified = y_row_ratio + (1 - x_col % 2) / (2 - pixel);

            float y_left_over = y_modified - (int)y_modified;
            if ( (.25f - x_left_over) * 2 >= Mathf.Abs(y_left_over - .5f))
                x_col--;
        }
        // If odd column number then shift y_diff down to account for nesting
        y_row_ratio += (x_col % 2) / (2 - pixel);
        

        int y_row = (int) y_row_ratio;

        //Debug.Log(string.Format("x_diff {0} y_diff {1} => y_row {2} x_col {3}", x_diff, y_diff, y_row, x_col));

        // force the columns to be within the boundries of the gameboard
        x_col = (x_col < 0) ? 0 : x_col;
        x_col = (x_col > tiles_array.GetUpperBound(1)) ? tiles_array.GetUpperBound(1) : x_col;
        y_row = (y_row < 0) ? 0 : y_row;
        y_row = (y_row > tiles_array.GetUpperBound(0)) ? tiles_array.GetUpperBound(0) : y_row;

        //Debug.Log(string.Format("Found closest tile to {0}, {1} x {2} y {3}", position.x, position.y, x_col, y_row));
        return tiles_array[y_row, x_col];
    }

    // Checks the collider components of nearby objects
    // and returns if there was a collision
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
