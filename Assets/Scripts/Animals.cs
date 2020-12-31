using UnityEngine;
using System.Collections;

public enum Animal_type {
    Bunny,
    Fox
}

// Basic class that every animal will inherit from
public class Animal {
    public Animal_type type;
    public static int num_bunnies = 0;
    public static int num_foxes = 0;

    // basic stats
    protected float health_max;
    protected float food_max;
    protected float food_drop;
    protected float hydro_max; // TODO implement drinking (rivers and lakes)
    protected float hydro_drop;

    protected float cur_health;
    protected float cur_food;
    protected float cur_hydro;

    protected float speed;
    protected float stamina; // TODO

    protected float threat;   // how much other animals are scared of this animal TODO
    protected float strength;

    protected float lifespan; // is this necessary? TODO

    protected int climate_pref_min; // the climate values within which this
    protected int climate_pref_max; // animal is comfortable

    // Game info
    public GameObject obj; // the game object which matches this animal
    private GameObject target;
    protected Vector3 towards_target = new Vector3(0, 0, 0);
    protected bool target_set = false;
    protected bool reach_target = false;
    protected Vector2 size;
    protected Tile on_tile;
    protected static System.Random rand = new System.Random();

    protected uint tick = 0;

    public Animal(float _health) {
        health_max = _health;
        food_max = _health / 2;
        hydro_max = _health / 2;
        cur_health = health_max;
        cur_food = food_max;
        cur_hydro = hydro_max;
        inc_count();
    }

    // this function is used if an animal needs to be removed from the
    // world either because of death or some other reason
    public void remove_Animal() {
        Debug.Log(string.Format("Animal: {0} has died", obj.name));
        Object.Destroy(obj);
        target = null;
        on_tile.animal_list.Remove(this);
        dec_count();
    }

    public virtual void dec_count() { }
    public virtual void inc_count() { }
    public virtual void update() { }

    // this function handles general updates for the animal
    // regardless of animal type
    // things like food and hydro drop go here
    // also moving towards a target or selecting a random target
    protected void basic_update() {
        // every 10 updates, check life conditions are and reevaluate target dir
        if (tick % 10 == 0) {
            effects();
            if (target_set) set_target_path();
            else setup_rmove();
        }

        // movement

        move_towards_target();

        // check if the animal is dead
        if (cur_health <= 0) {
            remove_Animal();
        }

        tick += 1;
    }


    protected void set_target(GameObject _target) {
        target = _target;
        target_set = true;
        reach_target = false;
        set_target_path();
    }

    protected void remove_target() {
        target = null;
        target_set = false;
    }

    // calculate the line we will move along in order to reach the
    // destination
    private void set_target_path() {
        // calculate the distance to the target
        Vector3 get2 = target.transform.position;
        Vector3 from = obj.transform.position;
        float x_transform = get2.x - from.x;
        float y_transform = get2.y - from.y;

        // calculate the x and y amounts moved each time based upon
        // the ratio of goals and the move value of the animal
        float angle = Mathf.Atan2(y_transform, x_transform);
        set_towards(angle);
    }

    // Given an angle in radians set the rotation and direction of movement
    private void set_towards(float angle) {
        float x_move = speed * Mathf.Cos(angle);
        float y_move = speed * Mathf.Sin(angle);

        towards_target.x = x_move;
        towards_target.y = y_move;
        //Debug.Log(string.Format("x move {0}, y move {1}, hyp {2}, speed {3}",
        //    x_move, y_move, Mathf.Sqrt(x_move * x_move + y_move * y_move), speed));

        obj.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    // Chose a random target direction
    // This function is used if there is literally nothing the animal would
    // rather be doing
    private void setup_rmove() {
        float angle = Random.Range(0.0f, 2 * Mathf.PI);
        set_towards(angle);
    }

    // helper function which moves the animal closer to its target
    // all animal movement is routed through this function
    // TODO agility should factor into our ability to change direction?
    // it basically becomes an acceleration stat? Work with vectors using the
    // rigidbodies?
    private void move_towards_target() {
        Vector3 cur_pos = obj.transform.position;
        Tile start_tile = on_tile;

        // edit position
        obj.transform.position = cur_pos + towards_target;

        // this function checks if the animal has crossed from one tile to another
        // 1. One tile to another, create a tile function which handles this
        on_tile = on_tile.TileClosestTo(obj);

        // 2. check if we have reached the target
        // TODO this needs work
        if (target_set && Object_Manager.CalcDistance(obj, target) < size.x / 2) {
            target_set = false;
            reach_target = true;
            target = null;
        }

        // 3. Collisions
        // checks if the animal has collided with an object
        // also checks if the animal has run into a border
        // if so it directs the animal to
        // TODO: Improve on this path finding
        if (on_tile.collision(obj) || on_tile.type == Terrain.Border || on_tile.type == Terrain.Water) {
            obj.transform.position = cur_pos; // undo the movement
            // current basic idea: just turn right, this code accomplishes that
            towards_target.x = towards_target.y;
            towards_target.y = towards_target.x * -1;
            on_tile = start_tile;
            move_towards_target(); // do the modified movement
        }
    }

    // this function checks the effects of low hydro/food
    // as well as lowering the current values of hydro/food
    private void effects() {
        // drop stats - larger effect if outside climate range
        int diff = climate_pref_max - climate_pref_min;
        if (on_tile.weather > climate_pref_max) {
            diff = on_tile.weather - climate_pref_max;

        }
        else if (on_tile.weather < climate_pref_min) {
            diff = climate_pref_min - on_tile.weather;
        }

        float climate_factor = 1 + (diff / (climate_pref_max - climate_pref_min));
        cur_food -= food_drop * climate_factor;
        cur_food = cur_food < 0 ? 0 : cur_food;
        cur_hydro -= hydro_drop * climate_factor;
        cur_hydro = cur_hydro < 0 ? 0 : cur_hydro;

        // drop health if hydro or food is too low
        if (cur_hydro < hydro_max * .1f) {
            if (cur_hydro == 0) {
                cur_health -= health_max * .1f;
            }
            else {
                cur_health -= health_max * .05f;
            }
        }

        if (cur_food < food_max * .1f) {
            if (cur_food == 0) {
                cur_health -= health_max * .1f;
            }
            else {
                cur_health -= health_max * .05f;
            }
        }

        // increase health if food and water are nearly full
        if (cur_food >= food_max * .8f && cur_hydro >= hydro_max * .8f && cur_health < health_max) {
            cur_health += health_max * .02f;
            cur_health = (cur_health > health_max) ? health_max : cur_health;
        }

        Debug.Log(string.Format("Done with effects on animal {0}, cur_food {1}, "
            + "cur_hydro {2}, cur_health {3}", obj.name, cur_food, cur_hydro,
            cur_health));
    }

    public void take_damage(float hit_amt) {
        cur_health -= hit_amt;
    }

    // TODO eating takes time
    // We should stay in this one spot to eat this plant until we're done
    // only thing that should drive us away is running from a fox or something
    public void eat_plant(Plant plant) {
        if (plant.type == Plant_type.Grass) {
            cur_food += Plant.grass_cost; // points used for grass are now in our tummy
            cur_food = cur_food > food_max ? food_max : cur_food;
            Debug.Log(string.Format("Eating plant : {0} new cur_food = {1}", plant.obj.name, cur_food));
            plant.removePlant();
        }
        else {
            Debug.Log("ERROR: should not be eating things other than grass yet!");
        }
    }

    // maybe this is the best way to go... maybe not
    protected void eat_animal(Animal animal) {

    }

    protected void attack_animal(Animal defender) {
        // default chance to hit : 60%
        // TODO maybe we want to add some sort of agility stat or something
        double attack_roll = rand.NextDouble();
        double defend_roll = rand.NextDouble();

        // calculate the relative strength of this animal versus the other
        float strength_ratio = defender.strength / strength;

        // check if attack succeeded
        if (attack_roll >= .4) {
            Debug.Log(string.Format("animal {0} attacked animal {1}",
                obj.name, defender.obj.name));


            float damage_them = strength / strength_ratio;

            defender.take_damage(damage_them);

            // TODO if they died then we then we monch
        }
        else {
            Debug.Log(string.Format("animal {0} failed to attack animal {1}",
                obj.name, defender.obj.name));
        }

        // TODO first check that they're still alive first
        // TODO we really need some function which implements animal death
        // check if they dealt damage back (possible they hurt us and we did nada)
        // TODO : have the agility like thing play a role here
        if (defend_roll >= .4) {
            float damage_us = strength_ratio * defender.strength;
            take_damage(damage_us);
            Debug.Log(string.Format("Defender dealt {0} damage",
                damage_us));
        }
        else {
            Debug.Log("Defender missed");
        }
    }
}
