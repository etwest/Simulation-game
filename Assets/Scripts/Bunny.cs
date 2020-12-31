using UnityEngine;

public class Bunny : Animal {
    // every 10 ticks drop by food_drop and start with 5 / .01/tick = 500 ticks
    public static int ticks_without_food = 1000; // TODO - these values probably need work
    public static int ticks_without_water = 500;
    public static int default_health = 10;
    public static Vector2[] collider_points =
        new Vector2[9] {
        new Vector2(-0.35f, 0.02f), new Vector2(-0.35f, -0.01279485f),
        new Vector2(-0.109432f, -0.008384347f), new Vector2(0.09558952f, -0.06993431f),
        new Vector2(0.132358f, -0.16f), new Vector2(0.3469821f, 0.003393054f),
        new Vector2(0.140767f, 0.1633931f), new Vector2(0.09211147f, 0.07689703f),
        new Vector2(-0.1085211f, 0.02122277f)
        };
    
    public static Sprite sprite;
    public static float scale = .15f;
    public Plant plant_to_eat;
    public bool moving_to_tile = false;

    public Bunny(GameObject _obj, Tile start_tile) : base(default_health) {
        // set basic characteristics
        food_drop = food_max / ticks_without_food * 10;
        hydro_drop = 0f; //TODO
        speed = .002f;
        stamina = 5;
        threat = 0;
        strength = 1;
        lifespan = 100; //TODO
        climate_pref_max = 200;
        climate_pref_min = 100;

        // setup gameobject and sprite
        obj = _obj;
        obj.name = string.Format("Bunny_{0}", num_bunnies);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Animals";

        // temporary scale-down measure to get the bunny to the right size
        obj.transform.localScale *= scale;

        // add collider
        PolygonCollider2D collider = obj.AddComponent<PolygonCollider2D>();
        // TODO adjust points on the collider
        collider.SetPath(0, collider_points);

        // basic Animal parameters
        on_tile = start_tile;
        type = Animal_type.Bunny;
        size = sprite.bounds.size * scale;
        Debug.Log(string.Format("Size of bunny {0}, {1}", size.x, size.y));
    }

    public override void inc_count() {
        num_bunnies++;
    }

    public override void dec_count() {
        num_bunnies--;
    }

    public override void update() {
        // check if we're trying to eat a plant which has died
        if(plant_to_eat != null && plant_to_eat.obj == null) {
            plant_to_eat = null;
            remove_target();
        }

        // check if we have a target set, if not then determine a new target
        // based upon some conditions, if we don't meet
        if (!target_set || moving_to_tile) {
            if (cur_hydro <= hydro_max / 2) {
                // go towards a source of drinking water TODO
            }
            else if (cur_food <= food_max / 2) {
                // go towards grass to monch
                plant_to_eat = on_tile.find_nearest(Plant_type.Grass, obj);
                if (plant_to_eat != null) {
                    set_target(plant_to_eat.obj);
                    //Debug.Log(string.Format("{0} Looked for a plant to eat, found {1}",
                    //    obj.name, plant_to_eat.obj.name));
                }
            }
            else {
                GameObject tile = best_tile();
                if (tile != on_tile.obj) {
                    //Debug.Log(string.Format("{0} found better tile {1}", obj.name, tile.name));
                    set_target(best_tile());
                    moving_to_tile = true;
                }
            }
        }

        basic_update();

        // check if we've reached our target (Maybe put this here ... do we want to give the jump to carnivores/attackers?)
        // pretty sure it doesn't actually matter either here or at the top of the function
        if (reach_target && plant_to_eat != null) {
            eat_plant(plant_to_eat);
            plant_to_eat = null;
            remove_target();
        }
    }

    // return the best tile nearby
    public GameObject best_tile() {
        int max_point = -100000;
        GameObject ret = on_tile.obj;
        foreach(Tile tile in on_tile.i_and_adj) {
            int points = tile.grass_list.Count * 10;
            if (tile.weather > climate_pref_max) {
                points -= tile.weather - climate_pref_max;
            }
            else if (tile.weather < climate_pref_min) {
                points -= climate_pref_min - tile.weather;
            }

            if (points > max_point) {
                max_point = points;
                ret = tile.obj;
            }
        }
        return ret;
    }
}
