using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrucks : MonoBehaviour
{
    public GameObject[] bodies;
    public GameObject[] boxSets;
    public GameObject startPoint;
    public GameObject wheel;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateTruck(TreeStructure tree)
    {
        GameObject truck = Instantiate(bodies[tree.body], new Vector3(0, 0, 0), Quaternion.identity);
        var TempTree = truck.GetComponent<TreeStructure>();
        TempTree.age = tree.age;
        TempTree.body = tree.body;
        TempTree.numOfBoxes = tree.numOfBoxes;
        TempTree.numOfWheels = tree.numOfWheels;
        TempTree.weightOfBoxes = tree.weightOfBoxes;
        TempTree.wheelsPos = tree.wheelsPos;
        TempTree.wheelsSize = tree.wheelsSize;
        TempTree.wheelsSpeed = tree.wheelsSpeed;

        Vector2 wheelAPoint = GameObject.FindGameObjectWithTag("TireAPoint").transform.position;
        Vector2 wheelBPoint = GameObject.FindGameObjectWithTag("TireBPoint").transform.position;

        float tiresMaxDistance = wheelBPoint.x - wheelAPoint.x;

        for(int i = 0; i < TempTree.numOfWheels; i++)
        {
            var tempWheel = Instantiate(wheel, new Vector2(wheelAPoint.x, TempTree.wheelsPos[i] * tiresMaxDistance + wheelAPoint.x),
                Quaternion.identity);
            tempWheel.transform.localScale = new Vector3(TempTree.wheelsSize[i], TempTree.wheelsSize[i],
                TempTree.wheelsSize[i]);
            Debug.Log(gameObject.GetComponent<WheelJoint2D>().motor.motorSpeed);
            JointMotor2D tempMotor = new JointMotor2D();
            tempMotor.motorSpeed = TempTree.wheelsSpeed[i];
            tempMotor.maxMotorTorque = 5000;
            tempWheel.GetComponent<WheelJoint2D>().motor = tempMotor;
            tempWheel.transform.parent = truck.transform;
        }

        Instantiate(boxSets[TempTree.numOfBoxes], GameObject.FindGameObjectWithTag("BoxesCenter").transform.position,
            Quaternion.identity);

        var boxes = GameObject.FindGameObjectsWithTag("Box");

        foreach(GameObject tempBox in boxes)
        {
            tempBox.GetComponent<Rigidbody2D>().mass = TempTree.weightOfBoxes;
        }

    }

}