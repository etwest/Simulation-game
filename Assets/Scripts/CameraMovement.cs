// TODO:
// 1. Reevaluate the bounds and the position of the UI items upon the resizing
//    of the camera resolution.
//          - we currently have an issue where if the camera is resized during
//            play and the width becomes larger than the height or vice versa
//            then the mechanism we have for limiting the movement of the camera
//            breaks

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject Camera_Obj;
    public Camera Camera;
    public Manage_UI UI_info;
    public float Move_Delta;
    public float Zoom_Delta;

    private float max_camera_scale = 20;
    private float min_camera_scale = 2;

    private float scale_orig = 0f;
    private float moveBy;
    private float zoomBy;

    private float world_width_half;
    private float world_height_half;

    private float header_height;
    private RectTransform rt_header;
    private RectTransform rt_canvas;

    public void SetWidthHeight(float width, float height) {
        world_width_half = width;
        world_height_half = height;
        // set the upper bound of the scale as the smaller
        // of height and width
        // the reason for + header/2 is that we want the height to be
        // the sum after incorporating the header
        if (width / Camera.aspect <= height + header_height / 2) max_camera_scale = width / Camera.aspect;
        else max_camera_scale = height + header_height / 2;

        if (Camera.orthographicSize > max_camera_scale && max_camera_scale > min_camera_scale) {
            Camera.orthographicSize = max_camera_scale;
        }

        header_height = rt_header.rect.height * rt_header.localScale.y * rt_canvas.localScale.y;
    }

    void Start() {
        scale_orig = Camera.orthographicSize;
        moveBy = Move_Delta;
        zoomBy = Zoom_Delta;
        rt_header = (RectTransform) UI_info.Header.transform;
        rt_canvas = (RectTransform) UI_info.Canvas.transform;
        // calculate the height of the header in world coordinates
        // header y -> canvas y -> world y
        header_height = rt_header.rect.height * rt_header.localScale.y * rt_canvas.localScale.y;
    }

    // Update is called once per frame
    // this function will deal with moving the Camera around
    // the world when the user presses various buttons
    // Will also handle Zooming
    void Update() {
        bool moved = false;
        // TODO: Right click drag?

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) { // UP
            Vector3 move = new Vector3(0f, moveBy, 0f);
            Camera_Obj.transform.position += move;
            moved = true;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) { // DOWN
            Vector3 move = new Vector3(0f, -moveBy, 0f);
            Camera_Obj.transform.position += move;
            moved = true;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { // LEFT
            Vector3 move = new Vector3(-moveBy,0f, 0f);
            Camera_Obj.transform.position += move;
            moved = true;
        }
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { // RIGHT
            Vector3 move = new Vector3(moveBy, 0f, 0f);
            Camera_Obj.transform.position += move;
            moved = true;
        }

        // we should move less quickly when zoomed in
        // this will adjust our moveBy by dividing by the proportion
        // zoomed we are. We set this here when scrolling
        float scroll = Input.mouseScrollDelta.y;

        // zoom in
        if(scroll > 0 && Camera.orthographicSize > min_camera_scale) {
            Camera.orthographicSize -= zoomBy;

            // reset moveBy
            float zoomChange = Camera.orthographicSize / scale_orig;
            moveBy = Move_Delta * zoomChange;

            // change Zoom_Delta
            zoomBy = Zoom_Delta * zoomChange;

            if (Camera.orthographicSize < min_camera_scale) {
                Camera.orthographicSize = min_camera_scale;
            }
            moved = true;
        }

        // zoom out
        else if (scroll < 0 && Camera.orthographicSize < max_camera_scale) {
            Camera.orthographicSize += zoomBy;

            // reset moveBy
            float zoomChange = Camera.orthographicSize / scale_orig;
            moveBy = Move_Delta * zoomChange;

            // change Zoom_Delta
            zoomBy = Zoom_Delta * zoomChange;

            if (Camera.orthographicSize > max_camera_scale) {
                Camera.orthographicSize = max_camera_scale;
            }
            moved = true;
        }

        if (moved) {
            header_height = rt_header.rect.height * rt_header.localScale.y * rt_canvas.localScale.y;
            // check that the camera is within the bounds of the
            // world and if it is not adjust it's position
            Vector3 position = Camera_Obj.transform.position;
            float scale = Camera.orthographicSize;
            float camera_width = Camera.aspect * scale;
            
            if (position[0] - camera_width < -1 * world_width_half) {
                // too far left
                //Debug.Log("Moved right");
                position[0] = -1 * world_width_half + camera_width;
            }
            if (position[0] + camera_width > world_width_half) {
                // too far right
                //Debug.Log("Moved left");
                position[0] = world_width_half - camera_width;
            }

            // being too far left/right not mutually exclusive with
            // too far up/down
            if (position[1] - scale < -1 * world_height_half) {
                // too far down
                //Debug.Log("Moved up");
                position[1] = -1 * world_height_half + scale;
            }
            // for too far up we now subtract out the height of the header
            // from our position to account for the fact that we're allowed
            // to be higher up now that the header is around
            else if (position[1] + scale - header_height > world_height_half) {
                // too far up
                //Debug.Log("Moved down");
                position[1] = world_height_half - scale + header_height;
            }

            // Move the camera back to within the bounds of the world
            Camera_Obj.transform.position = position;
        }
    }
}
