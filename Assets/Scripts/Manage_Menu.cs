using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Manage_Menu : MonoBehaviour {
    public RectTransform UI_Box;
    public RectTransform canvas;
    
    public InputField seed_value;
    public Toggle seed_check;

    // Use this for initialization
    void Start() {
        if (PlayerPrefs.GetString("use_seed") == "yes") {
            seed_check.isOn = true;
        }
        else {
            seed_check.isOn = false;
        }
        seed_value.text = PlayerPrefs.GetInt("seed_value").ToString();

        // change the size of the UI_Box to fit the resolution of the screen
        // want the height to be half of the screen
        float diff = canvas.rect.height / 2 / UI_Box.rect.height;
        UI_Box.localScale = new Vector3(diff, diff, 1);
    }

    public void click_Start() {
        Debug.Log("Starting new game!");
        if (seed_check.isOn) {
            string seed = seed_value.text;
            if (seed != "") {
                PlayerPrefs.SetString("use_seed", "yes");
                PlayerPrefs.SetInt("seed_value", int.Parse(seed));
                Debug.Log(string.Format("using user defined seed: {0}", seed));
            }
        }
        else {
            PlayerPrefs.SetString("use_seed", "no");
        }
        SceneManager.LoadScene("Game");
    }
}
