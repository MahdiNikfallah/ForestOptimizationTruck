using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public float timeScale = 1;
    // Start is called before the first frame update
    void Awake()
    {
        Time.timeScale = timeScale;
        
    }

    // Update is called once per physics frame
    private void FixedUpdate()
    {
    }
}
