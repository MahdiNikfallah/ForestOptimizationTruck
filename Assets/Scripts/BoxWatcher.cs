using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxWatcher : MonoBehaviour
{

    public bool isOnBoard = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == 7)
        {
            isOnBoard = false;
        }
    }
}
