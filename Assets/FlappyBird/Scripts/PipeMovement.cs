using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeMovement : MonoBehaviour {


    float speed = 0;
	// Use this for initialization
	void Start () {
		
	}

    private void FixedUpdate()
    {
        transform.position += new Vector3(speed, 0);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.x + 100f < 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
}
