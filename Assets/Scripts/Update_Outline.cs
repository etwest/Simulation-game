using UnityEngine;
using System.Collections;

// This script is attached to game tiles and moves the hex outline to said
// game tile if the mouse pointer is over that tile
public class Update_Outline : MonoBehaviour {
    public GameObject outline;
    public Tile tile;

    public void OnMouseEnter() {
        Debug.Log("Entered tile");
        outline.transform.position = tile.obj.transform.position;
        if (tile.type == Terrain.Border) outline.SetActive(false);
        else outline.SetActive(true);
    }
}
