using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TreeStructure : MonoBehaviour
{
    public int age;
    public int body = 0;
    public int numOfBoxes;
    public float weightOfBoxes;
    public int numOfWheels;
    public float[] wheelsPos;
    public float[] wheelsSpeed;
    public float[] wheelsSize;
    public float value;

    public TreeStructure()
    {
        wheelsPos = new float[6];
        wheelsSpeed = new float[6];
        wheelsSize = new float[6];
        value = -1;
    }

    public void Clone(TreeStructure tree)
    {
        this.age = tree.age;
        this.body = tree.body;
        this.numOfBoxes = tree.numOfBoxes;
        this.weightOfBoxes = tree.weightOfBoxes;
        this.numOfWheels = tree.numOfWheels;
        for (int i = 0; i < wheelsPos.Length; i++)
        {
            this.wheelsPos[i] = tree.wheelsPos[i];
            this.wheelsSpeed[i] = tree.wheelsSpeed[i];
            this.wheelsSize[i] = tree.wheelsSize[i];
        }
        this.value = tree.value;
    }
}
