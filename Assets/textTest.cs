using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class textTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string path = Application.dataPath + "/LogM.txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Hellooo");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
