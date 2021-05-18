using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdControl : MonoBehaviour
{

    public float gravity;
    public float jumpAmount;

    BirdGameManager gameManager;
    float speed = 0; // vertical speed
    bool jump = false;

    int index;
    bool dead = false;
    // Use this for initialization
    void Start()
    {
        gameManager = GameObject.Find("_SCRIPTS").GetComponent<BirdGameManager>();
    }

    private void FixedUpdate()
    {
        speed -= gravity;
        if (jump)
        {
            speed = jumpAmount;
            jump = false;
        }
        transform.position += new Vector3(0, speed, 0);
        transform.rotation = Quaternion.Euler(0, 0, speed * 100);
    }

    public void Jump()
    {
        jump = true;
    }

    public void Reset()
    {
        speed = 0;
        jump = false;
    }

    public void SetIndex(int id)
    {
        index = id;
    }

    public int GetIndex()
    {
        return index;
    }

    public void SetDead(bool dead)
    {
        this.dead = dead;
    }

    public bool GetDead()
    {
        return dead;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pipe")
        {
            gameManager.BirdFailed(index);
        }
    }
}
