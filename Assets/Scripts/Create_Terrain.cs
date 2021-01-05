#define PRINT_LAND
#undef PRINT_LAND
using System;
using System.Collections.Generic;
using UnityEngine;
using SimplexNoise;

public enum Terrain {
    Water,
    Desert,
    Dirt,
    Rock,
    Border
}

// This script will create the terrain for the game
// It generates random locations using simplex noise
public class Create_Terrain : MonoBehaviour {
    /* TODO:
     1. shallow water
     2. Exposed rock v Mountains.
        Maybe exposed rock comes in as low moisture but near water
        while desert is only if we are further away from water.
     3. Flat Dirt versus hills versus coast
     4. Find a way to encourage rare larger groups of water
        as opposed to many scattered groups
    */
    public Sprite Desert_Tile1;
    public Sprite Dirt_Tile1;
    public Sprite Rock_Tile1;
    public Sprite Water_Tile1;

    public int seed;

    public CameraMovement cameraMovement;
    public Object_Manager objectManager;
    public GameObject Outline;

    private float world_height_half;
    private float world_width_half;

    // the size of the tiles.
    // r is the length from middle to top
    // R is length from middle to side
    // this is a consequence of the number of the pixels
    // which make up the tile sprites and the scale of the tiles
    public static readonly float tile_scale = .5f;
    public static readonly float r = 0.26f * tile_scale;
    public static readonly float R = 0.30f * tile_scale;

    // the size of the world in tiles
    //      |
    //      |
    // <- width ->
    //      |
    //      | height
    // one row(one unit of height) is a set of hexes that
    // go up and down in a line. Each column is each hex
    // in that line.
    // Ex:
    // x x x x
    //  x x x
    // This is height 1, width 7

    // x x x
    //  x x x
    // This is height 1, width 6

    public int height = 60;
    public int width  = 100;
    public Tile[] dirt_tiles;
    public Tile[] rock_tiles;
    public Tile[] desert_tiles;
    public Tile[] water_tiles;

    readonly float pixel = .01f;

    // All the work to create the terrain will be
    // completed within this function once.
    public void createTerrain() {
        // setup the outline
        Outline.transform.localScale = new Vector3(tile_scale, tile_scale, 1);

        // setup terrain lists
        List<Tile> dirt_list   = new List<Tile>();
        List<Tile> rock_list   = new List<Tile>();
        List<Tile> desert_list = new List<Tile>();
        List<Tile> water_list  = new List<Tile>();

        // ensure that the world width and height are at least 6 and 4
        height = (height < 8) ? 8 : height;
        width  = (width < 12) ? 12 : width;
        // calculate the bounds of the world

        // calculation for width is fairly complicated
        // Every 2 tiles we move 3R (small lie).
        // To address this lie we consider what we end upon
        // end on either a upper hex or a lower 'nested' hex. In the case of a
        // upper hex this adds another 2R to total length.
        // In the case of a lower hex we will have already have accounted for
        // most of the distance but another half R will need to be added to
        // the total ( / 4 to the half)
        world_width_half = (width / 2 * R * 3 / 2) - (pixel / 4 * (width - 1));
        if (width % 2 == 0) {
            world_width_half += R / 4;
        }
        else {
            world_width_half += R;
        }
        // height is a fairly straightforward calculation as hexes are stacked
        // however we also have to consider that the bottom row will have
        // nested hexes which poke out the bottom by and additional
        // r adding r/2 to height_half
        world_height_half = (height * r) + (r / 2) - (pixel / 2 * (height - 1));
        cameraMovement.SetWidthHeight(world_width_half, world_height_half);

        // increse width and height by 2 so that we can add a border
        width += 2; height += 2;

        // need to account for the borders in these calculations
        // start at the bottom left corner
        float x = (-1 * (world_width_half + R * 3 / 2)) + R;
        // 2r to account for nested hex
        float y = (-1 * (world_height_half + 2 * r)) + (2 * r);
        float start_x = x;
        float start_y = y;

        // set up seed for simplex
        if (PlayerPrefs.GetString("use_seed") == "no") {
            System.Random rand = new System.Random();
            seed = rand.Next();
        }
        else {
            seed = PlayerPrefs.GetInt("seed_value");
        }

        // 2D array which represents our world for building pointers
        // between hexes, now iterate through these tiles and
        // point the neighbors at eachother.
        // we're starting at the bottom
        // we also add additional tiles which are our border around
        // the world. Nothing grows on these tiles and no one can move
        // there
        Tile[,] tiles_arr = new Tile[height + 2, width + 2];
        for (int row = 0; row < height + 2; row++) {
            for (int col = 0; col < width + 2; col++) {
                tiles_arr[row, col] = new Tile();
                Tile cur = tiles_arr[row, col];

                // not first row, set south and then set SW SE
                // if we are the upper one
                if (row > 0) {
                    cur.S = tiles_arr[row - 1, col];
                    tiles_arr[row - 1, col].N = cur;
                    if (col % 2 != 0) {
                        // if lower of row
                        if (col != width - 1) {
                            cur.SE = tiles_arr[row - 1, col + 1];
                            tiles_arr[row - 1, col + 1].NW = cur;
                        }
                        if (col > 0) {
                            cur.SW = tiles_arr[row - 1, col - 1];
                            tiles_arr[row - 1, col - 1].NE = cur;
                        }
                    }
                }

                // set pointers to the one before us
                if (col > 0) {
                    if (col % 2 == 0) {
                        // if upper of row
                        cur.SW = tiles_arr[row, col - 1];
                        tiles_arr[row, col - 1].NE = cur;
                    }
                    else {
                        // if lower of row
                        cur.NW = tiles_arr[row, col - 1];
                        tiles_arr[row, col - 1].SE = cur;
                    }
                }
            }
        }

        // TODO: scale value be based on width/height?
        float scale = .04f; // this scale determines how spread out the terrain is
        Noise.Seed = seed;
        float[,] terrain_map = Noise.Calc2D(width, height, scale);
        Noise.Seed = seed - 10;
        float[,] weather_map = Noise.Calc2D(width, height, scale);

        GameObject current;
        int num_types = Enum.GetNames(typeof(Terrain)).Length;

        // loop through each Tile we want to create
        for (int row = 0; row < height; row++) {
            for(int col = 0; col < width; col++) {
                // name and create the hex
                string name = string.Format("Tile_{0}-{1}", row, col);
                current = new GameObject(name);
                current.transform.position = new Vector3(x, y, 0);
                current.transform.localScale = new Vector3(tile_scale, tile_scale, 1);
                SpriteRenderer sr = current.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Terrain";
                Update_Outline updateOutline = current.AddComponent<Update_Outline>();
                

                // Set up the tile
                Tile cur_tile      = tiles_arr[row, col];
                cur_tile.obj       = current;
                cur_tile.i_and_adj = new Tile[7] { cur_tile, cur_tile.N, cur_tile.NE,
                    cur_tile.NW, cur_tile.S, cur_tile.SE, cur_tile.SW };

                // set up outline updater
                updateOutline.tile = cur_tile;
                updateOutline.outline = Outline;

                // if the tile is a border don't give it a sprite at all
                // and give it a the border type
                if (row == 0 || row == height - 1 || col == 0 || col == width - 1) {
                    cur_tile.type = Terrain.Border;
                }
                // assign a terrain sprite
                else {
                    // check if we are near a border, if so set the bool value
                    if (row == 1 || row == height - 2 || col == 1 || col == width - 2)
                        cur_tile.near_border = true;

                    // what are the noise values for this hex
                    float terrain_noise = terrain_map[col, row];
                    float weather_noise = weather_map[col, row];
                    cur_tile.weather = (int) weather_noise;
                    for (int i = 0; i < num_types; i++) {
                        // num types - 1 because we don't want to
                        // here we are checking if the terrain noise
                        // is less than the associated i value and if
                        // so assign the appropriate sprite
                        if (terrain_noise < (i + 1) * 256 / (num_types - 1)) {
                            switch ((Terrain)i) {
                                case Terrain.Desert:
                                case Terrain.Dirt:
                                    if (weather_noise > 256 * .25) {
                                        // dirt tile
                                        dirt_list.Add(cur_tile);
                                        sr.sprite = Dirt_Tile1;
                                        cur_tile.type = Terrain.Dirt;
                                        cur_tile.plant_points = (int) (25 * weather_noise / 200);
                                    }
                                    else {
                                        // desert tile
                                        desert_list.Add(cur_tile);
                                        sr.sprite = Desert_Tile1;
                                        cur_tile.type = Terrain.Desert;
                                        cur_tile.plant_points = (int) (4 * weather_noise / (256 * .25));
                                    }
                                    break;
                                case Terrain.Water:
                                    water_list.Add(cur_tile);
                                    sr.sprite = Water_Tile1;
                                    cur_tile.type = Terrain.Water;
                                    break;
                                case Terrain.Rock:
                                    rock_list.Add(cur_tile);
                                    sr.sprite = Rock_Tile1;
                                    cur_tile.type = Terrain.Rock;
                                    cur_tile.plant_points = (int) (10 * weather_noise / 256);
                                    break;
                                default:
                                    Debug.Log("Failed to assign a terrain!!");
                                    break;
                            }
                            break;
                        }
                    }
                }
                current.isStatic = true;

                // modify the x value
                // account for pixel overlap
                x += ((3 * R) - pixel) / 2;
                // modify the y value
                if (col % 2 == 0) {
                    // go SE
                    y -= ((2 * r) - pixel) / 2;
                }
                else {
                    // go NE
                    y += ((2 * r) - pixel) / 2;
                }
            }

            // done moving through this row so reset x
            x = start_x;
            // adjust y to new row start (same as moving North)
            y = start_y + (2 * r) - pixel;
            start_y = y;
        }

#if PRINT_LAND
        for (int row = 1; row < height - 1; row++) {
            for (int col = 1; col < width - 1; col++) {
                Tile cur_tile = tiles_arr[row, col];
                Debug.Log(string.Format("neighbors list for {0} ",
                    cur_tile.i_and_adj[0].obj.name));

                Debug.Log(string.Format("{0} {1} {2} {3} {4} {5}", cur_tile.i_and_adj[1].obj.name,
                    cur_tile.i_and_adj[2].obj.name, cur_tile.i_and_adj[3].obj.name,
                    cur_tile.i_and_adj[4].obj.name, cur_tile.i_and_adj[5].obj.name,
                    cur_tile.i_and_adj[6].obj.name));
            }
        }
#endif
        // set the dirt/desert/rock/water arrays
        dirt_tiles = dirt_list.ToArray();
        desert_tiles = desert_list.ToArray();
        rock_tiles   = rock_list.ToArray();
        water_tiles  = water_list.ToArray();
    }
}
