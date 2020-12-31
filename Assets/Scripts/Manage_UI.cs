using UnityEngine;
using UnityEngine.SceneManagement;

public class Manage_UI : MonoBehaviour {
    public GameObject Canvas;
    public GameObject Header;
    public GameObject Count_Display;
    public UnityEngine.EventSystems.EventSystem EventSystem;

    // Use this for initialization
    void Start() {
        // adjust the positions of the menu buttons
        // this is in order to position them properly regardless of aspect ratio
        RectTransform canvas_pos = (RectTransform)Canvas.transform;
        RectTransform header_pos = (RectTransform)Header.transform;

        // adjust the scale of the header
        // we want the menu to take 1/25th of the menu
        float diff_y = canvas_pos.rect.height / 25 / header_pos.rect.height;
        float diff_x = diff_y * header_pos.localScale.x / header_pos.localScale.y;
        header_pos.localScale = new Vector3(diff_x, diff_y, 1);

        // adjust the position of the header
        float width = canvas_pos.rect.width / 2;
        float height = canvas_pos.rect.height / 2;
        float x = (width * -1) + (header_pos.rect.width * header_pos.localScale.x / 2);
        float y = height - (header_pos.rect.height * header_pos.localScale.y / 2);
        header_pos.localPosition = new Vector3(x, y, 0);

        Count_Display.SetActive(false);
    }

    public void click_Menu() {
        SceneManager.LoadScene("Main Menu");
    }

    public void click_Counts() {
        Debug.Log("Counts clicked");
        if (Count_Display.activeSelf) Count_Display.SetActive(false);
        else Count_Display.SetActive(true);

        // deselect the counts button so that pausing won't have
        // unintended side-effects
        EventSystem.SetSelectedGameObject(null);
    }
}
