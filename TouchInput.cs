using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchInput : MonoBehaviour
{
    float x1;
    public float x;
    public GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        //get screen center
        int center = Screen.width / 2;
        //refrence touch
        Touch touch;

        //if screen is being activly touched
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Moved)
        {
            //find first touch
            touch = Input.GetTouch(0);

            //calculate vector from 0,0 to center,0
            Vector2 n = new Vector2(center, 0);

            //find touch x position from default 0,0
            Vector2 p = new Vector2(touch.position.x, 0);

            //calculate distance between center and touch position
            x1 = Vector2.Distance(n, p) / center;

            //calculate move direction and set it.
            if (touch.position.x < center)
            {
                x1 *= -1;
                x1 = Mathf.Clamp(x1, -1, 0);
                x = x1;
            }
            if (touch.position.x > center)
            {
                x1 *= 1;
                x1 = Mathf.Clamp(x1, 0, 1);
                x = x1;
            }
        }
        if (Input.touchCount == 0 || Input.touches[0].phase == TouchPhase.Ended)
        {
            x1 *= 0f;
            x = x1;
            /*
            Vector3 defaultPosition = new Vector3(0f, 0.5f, -33.63f);
            float speed = 1f;
            float step = speed * Time.deltaTime;
            if (player.transform.position.x > 0f)
            {
                transform.position = Vector3.MoveTowards(player.transform.position, defaultPosition, step);
            }
            */
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Vector2 n = new Vector2(center, 0);
            Vector2 p = new Vector2(Input.mousePosition.x, 0);
            x1 = Vector2.Distance(n, p) / center;
            if(Input.mousePosition.x < center)
            {
                x1 = x1 * -1;
                x = x1;
            }
            if (Input.mousePosition.x > center)
            {
                x1 = x1 * 1;
                x = x1;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 defaultPosition = new Vector3(0f, 0.5f, -33.63f);
            float speed = 1f;
            float step = speed * Time.deltaTime;
            if (player.transform.position.x > 0f)
            {
                transform.position = Vector3.MoveTowards(player.transform.position, defaultPosition, step);
            }
        }
        #endif
    }
}
