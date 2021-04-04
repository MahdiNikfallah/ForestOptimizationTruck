using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using UnityEditor;

public class FOAManager : MonoBehaviour
{
    public bool replay;
    //The algorithm attributes
    #region FOA attribiutes
    //The initial size of forest
    public int initialSize = 30;

    //The maximum number of trees
    public int areaLimit = 30;

    //The life time of each tree
    public int lifeTime = 6;

    //Local seeding changes
    public int LSC = 2;

    //Global seeding changes
    public int GSC = 2;

    //The percentage of candidate population for global seeding
    public float transferRate = 0.1f;
    #endregion

    //Upper limits and Lower limits of the attributes
    #region attributes limitation

    public int bodyLL = 0;
    public int bodyUL = 0;

    public int numOfWheelsLL = 2;
    public int numOfWheelsUL = 6;

    public float weightOfBoxesLL = 1f;
    public float weightOfBoxesUL = 6f;

    public int numOfBoxesLL = 0;
    public int numOfBoxesUL = 5;

    public float wheelsPosLL = 0f;
    public float wheelsPosUL = 1f;

    public float wheelsSpeedLL = 100;
    public float wheelsSpeedUL = 800;

    public float wheelsSizeLL = 0.1f;
    public float wheelsSizeUL = 1f;

    public int numOfWheelsLCR = 1;

    public float weightOfBoxesLCR = 0.4f;

    public int numOfBoxesLCR = 1;

    public float wheelPosLCR = 0.75f;

    public float wheelSpeedLCR = 50f;

    public float wheelSizeLCR = 0.1f;

    public int bodyLCR = 0;

    //float WheelSpeed

    #endregion

    public int round = 1;

    SortedList<float, TreeStructure> truckTrees = new SortedList<float, TreeStructure>();

    List<TreeStructure> candidatePopulation = new List<TreeStructure>();

    public int levelOfDependabality = 1;

    public GameObject[] bodies;
    public GameObject[] boxSets;
    public GameObject startPoint;
    public GameObject wheel;

    public Text roundText;
    public Text phaseText;
    public Text iterationText;
    public Text populationText;
    public Text fitnessText;
    public Text bestFitText;

    TreeStructure follow;
    bool followBool = false;

    float lastBestFit;


    public string path;

    // Start is called before the first frame update
    void Start()
    {
        path = Application.dataPath + "/TreeLogs.txt";


        if (replay)
            StartCoroutine(Replay());
        else
            StartCoroutine(ForestInitialization());

    }

    // Update is called once per physics update
    private void FixedUpdate()
    {
        if (truckTrees.Count != 0)
        {
            if (truckTrees.ElementAt(truckTrees.Count - 1).Value.value != lastBestFit)
            {
                Debug.Log("hey");
                WriteSingleTree(JsonUtility.ToJson(truckTrees.ElementAt(truckTrees.Count - 1).Value));
            }
        }
        if (truckTrees.Count != 0)
            bestFitText.text = "Best Fit : " + truckTrees.ElementAt(truckTrees.Count - 1).Value.value.ToString();
        roundText.text = "Round : " + round;
        if (truckTrees.Count != 0)
            lastBestFit = truckTrees.ElementAt(truckTrees.Count - 1).Value.value;
        populationText.text = "Population : " + truckTrees.Count;
    }

    //private void WriteToFile(TreeStructure tree)
    //{
    //    string path = "Assets/Resources/test.txt";

    //    //Write some text to the test.txt file
    //    StreamWriter writer = new StreamWriter(path, true);
    //    writer.WriteLine("Phase : " + phaseText.text + "iteration" + iterationText.text + "\n"
    //        + JsonUtility.ToJson(tree));
    //    writer.Close();

    //    //Re-import the file to update the reference in the editor
    //    AssetDatabase.ImportAsset(path);
    //}

    //private void WriteToFile(string message)
    //{
    //    string path = "Assets/Resources/test.txt";

    //    //Write some text to the test.txt file
    //    StreamWriter writer = new StreamWriter(path, true);
    //    writer.WriteLine(message);
    //    writer.Close();

    //    //Re-import the file to update the reference in the editor
    //    AssetDatabase.ImportAsset(path);
    //}
    private void WriteSingleTree(string content)
    {

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Round : " + round + "  " + phaseText.text + "  " +
                 iterationText.text + "\n" + content + "\n");
        }
        else
        {
            File.AppendAllText(path, "Round : " + round + "  " + phaseText.text + "  " +
                 iterationText.text + "\n" + content + "\n");
        }
    }

    private void WriteTreeList(string content)
    {
        File.AppendAllText(path, content + "\n");
    }

    private IEnumerator Replay()
    {
        string path = "Assets/Resources/test.txt";
        string bestTreeJson = "";
        StreamReader reader = new StreamReader(path);
        TreeStructure bestTree = new TreeStructure();

        while (!reader.EndOfStream)
        {
            Debug.Log("hah");
            if (reader.Peek() == '{')
            {
                bestTreeJson = reader.ReadLine();
                JsonUtility.FromJsonOverwrite(bestTreeJson, bestTree);
                bestTree.value = -1;
                GenerateTruck(bestTree);
                yield return new WaitUntil(() => bestTree.value != -1);
                float finalValue = float.MaxValue;
                float[] values = new float[levelOfDependabality];
                for (int k = 0; k < levelOfDependabality; k++)
                {
                    if (k > 0)
                    {
                        if (values[k - 1] < finalValue)
                            finalValue = values[k - 1];
                        Debug.Log("finval" + values[k]);
                        bestTree.value = -1;
                        GenerateTruck(bestTree, finalValue);
                        yield return new WaitUntil(() => bestTree.value != -1);
                        values[k] = bestTree.value;
                    }
                    else
                    {
                        bestTree.value = -1;
                        GenerateTruck(bestTree);
                        yield return new WaitUntil(() => bestTree.value != -1);
                        values[k] = bestTree.value;
                        Debug.Log("finval" + values[k]);
                    }
                    yield return new WaitForSeconds(1);
                }
            }
            else
            {
                reader.ReadLine();
            }
        }
        Debug.Log(bestTreeJson);
        reader.Close();
    }



    IEnumerator ForestInitialization()
    {
        int iteration = 1;
        for (int i = 0; i < initialSize; i++)
        {
            iterationText.text = "Iteration : " + iteration;
            phaseText.text = "Phase : Initialization";
            iteration++;
            TreeStructure truck = new TreeStructure();
            truck.age = 0;
            truck.body = GenerateValidRandInt(bodyLL, bodyUL + 1);
            //truck.body = UnityEngine.Random.Range(bodyLL, bodyUL + 1);
            truck.numOfBoxes = UnityEngine.Random.Range(numOfBoxesLL, numOfBoxesUL + 1);
            truck.numOfWheels = UnityEngine.Random.Range(numOfWheelsLL, numOfWheelsUL + 1);
            truck.weightOfBoxes = UnityEngine.Random.Range(weightOfBoxesLL, weightOfBoxesUL);
            for (int j = 0; j < numOfWheelsUL; j++)
            {
                truck.wheelsPos[j] = UnityEngine.Random.Range(wheelsPosLL, wheelsPosUL);
                truck.wheelsSpeed[j] = UnityEngine.Random.Range(wheelsSpeedLL, wheelsSpeedUL);
                truck.wheelsSize[j] = UnityEngine.Random.Range(wheelsSizeLL, wheelsSizeUL);
            }
            float finalValue = float.MaxValue;
            float[] values = new float[levelOfDependabality];
            for (int k = 0; k < levelOfDependabality; k++)
            {
                if (k > 0)
                {
                    if (values[k - 1] < finalValue)
                        finalValue = values[k - 1];
                    Debug.Log("finval" + values[k]);
                    truck.value = -1;
                    GenerateTruck(truck, finalValue);
                    yield return new WaitUntil(() => truck.value != -1);
                    values[k] = truck.value;
                }
                else
                {
                    truck.value = -1;
                    GenerateTruck(truck);
                    yield return new WaitUntil(() => truck.value != -1);
                    values[k] = truck.value;
                    Debug.Log("finval" + values[k]);
                }
            }
            finalValue = float.MaxValue;
            for (int z = 0; z < values.Length; z++)
                if (values[z] < finalValue)
                    finalValue = values[z];
            truck.value = finalValue;
            if (truckTrees.Count > areaLimit)
                throw new System.Exception("initialization size is bigger than forest limit");
            while (truckTrees.ContainsKey(truck.value))
                truck.value += 0.000001f;
            truckTrees.Add(truck.value, truck);
            fitnessText.text = "Fitness : " + truck.value;
        }
        WriteTreeList("Round : " + round + phaseText.text);
        for (int i = 0; i < truckTrees.Count; i++)
            WriteTreeList(JsonUtility.ToJson(truckTrees.ElementAt(i).Value));
        StartCoroutine(LocalSeeding());
    }

    IEnumerator LocalSeeding()
    {
        int iteration = 1;
        List<TreeStructure> tempTreeList = new List<TreeStructure>(truckTrees.Values.ToList());
        WriteTreeList("temptree : Round : " + round + phaseText.text);
        for (int i = 0; i < truckTrees.Count; i++)
            WriteTreeList(JsonUtility.ToJson(tempTreeList.ElementAt(i)));
        for (int j = 0; j < tempTreeList.Count; j++)
        {
            Debug.Log("temptree" + tempTreeList.Count);
            tempTreeList[j].age++;
            if (tempTreeList[j].age == 1)
            {
                for (int i = 0; i < LSC; i++)
                {
                    TreeStructure tree = new TreeStructure();
                    tree.Clone(tempTreeList[j]);
                    tree.age = 0;
                    phaseText.text = "Phase : Local Seeding";
                    iteration++;
                    iterationText.text = "Iteration : " + iteration;
                    yield return new WaitForSeconds(1);
                    int UL = 3 + tree.numOfWheels * 3;
                    UL++;
                    int randomNode = UnityEngine.Random.Range(0, UL);
                    int intRand;
                    while (randomNode == UL)
                    {
                        Debug.Log("while5");
                        randomNode = UnityEngine.Random.Range(0, UL);
                    }
                    #region variable changing
                    switch (randomNode)
                    {
                        case 0:
                            tree.numOfBoxes += GenerateValidRandInt(-numOfBoxesLCR, numOfBoxesLCR + 1);
                            tree.numOfBoxes = TrimValue<int>(tree.numOfBoxes, numOfBoxesLL, numOfBoxesUL);
                            break;
                        case 1:
                            tree.body += GenerateValidRandInt(-bodyLCR, bodyLCR + 1);
                            tree.body = TrimValue<int>(tree.body, bodyLL, bodyUL);
                            break;
                        case 2:
                            tree.weightOfBoxes += GenerateValidRandFloat(-weightOfBoxesLCR, weightOfBoxesLCR);
                            tree.weightOfBoxes = TrimValue<float>(tree.weightOfBoxes, weightOfBoxesLL, weightOfBoxesUL);
                            break;
                        #endregion
                        case 3:
                            int lastNumOfWheels = tree.numOfWheels;
                            tree.numOfWheels += GenerateValidRandInt(-numOfWheelsLCR, numOfWheelsLCR + 1);
                            tree.numOfWheels = TrimValue<int>(tree.numOfWheels, numOfWheelsLL, numOfWheelsUL);
                            if (tree.numOfWheels > lastNumOfWheels)
                            {
                                for (int z = 0; z < (tree.numOfWheels - lastNumOfWheels); z++)
                                {
                                    tree.wheelsPos[lastNumOfWheels + z] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                                    tree.wheelsSize[lastNumOfWheels + z] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                                    tree.wheelsSpeed[lastNumOfWheels + z] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                                }
                            }
                            break;
                        #region variable changing
                        case 4:
                            tree.wheelsPos[0] += GenerateValidRandFloat(-wheelPosLCR, wheelPosLCR);
                            tree.wheelsPos[0] = TrimValue<float>(tree.wheelsPos[0], wheelsPosLL, wheelsPosUL);
                            break;
                        case 5:
                            tree.wheelsSpeed[0] += GenerateValidRandFloat(-wheelSpeedLCR, wheelSpeedLCR);
                            tree.wheelsSpeed[0] = TrimValue<float>(tree.wheelsSpeed[0], wheelsSpeedLL, wheelsSpeedUL);
                            break;
                        case 6:
                            tree.wheelsSize[0] += GenerateValidRandFloat(-wheelSizeLCR, wheelSizeLCR);
                            tree.wheelsSize[0] = TrimValue<float>(tree.wheelsSize[0], wheelsSizeLL, wheelsSizeUL);
                            break;
                        case 7:
                            tree.wheelsPos[1] += GenerateValidRandFloat(-wheelPosLCR, wheelPosLCR);
                            tree.wheelsPos[1] = TrimValue<float>(tree.wheelsPos[1], wheelsPosLL, wheelsPosUL);
                            break;
                        case 8:
                            tree.wheelsSpeed[1] += GenerateValidRandFloat(-wheelSpeedLCR, wheelSpeedLCR);
                            tree.wheelsSpeed[1] = TrimValue<float>(tree.wheelsSpeed[1], wheelsSpeedLL, wheelsSpeedUL);
                            break;
                        case 9:
                            tree.wheelsSize[1] += GenerateValidRandFloat(-wheelSizeLCR, wheelSizeLCR);
                            tree.wheelsSize[1] = TrimValue<float>(tree.wheelsSize[1], wheelsSizeLL, wheelsSizeUL);
                            break;
                        case 10:
                            tree.wheelsPos[2] += GenerateValidRandFloat(-wheelPosLCR, wheelPosLCR);
                            tree.wheelsPos[2] = TrimValue<float>(tree.wheelsPos[2], wheelsPosLL, wheelsPosUL);
                            break;
                        case 11:
                            tree.wheelsSpeed[2] += GenerateValidRandFloat(-wheelSpeedLCR, wheelSpeedLCR);
                            tree.wheelsSpeed[2] = TrimValue<float>(tree.wheelsSpeed[2], wheelsSpeedLL, wheelsSpeedUL);
                            break;
                        case 12:
                            tree.wheelsSize[2] += GenerateValidRandFloat(-wheelSizeLCR, wheelSizeLCR);
                            tree.wheelsSize[2] = TrimValue<float>(tree.wheelsSize[2], wheelsSizeLL, wheelsSizeUL);
                            break;
                        case 13:
                            tree.wheelsPos[3] += GenerateValidRandFloat(-wheelPosLCR, wheelPosLCR);
                            tree.wheelsPos[3] = TrimValue<float>(tree.wheelsPos[3], wheelsPosLL, wheelsPosUL);
                            break;
                        case 14:
                            tree.wheelsSpeed[3] += GenerateValidRandFloat(-wheelSpeedLCR, wheelSpeedLCR);
                            tree.wheelsSpeed[3] = TrimValue<float>(tree.wheelsSpeed[3], wheelsSpeedLL, wheelsSpeedUL);
                            break;
                        case 15:
                            tree.wheelsSize[3] += GenerateValidRandFloat(-wheelSizeLCR, wheelSizeLCR);
                            tree.wheelsSize[3] = TrimValue<float>(tree.wheelsSize[3], wheelsSizeLL, wheelsSizeUL);
                            break;
                        case 16:
                            tree.wheelsPos[4] += GenerateValidRandFloat(-wheelPosLCR, wheelPosLCR);
                            tree.wheelsPos[4] = TrimValue<float>(tree.wheelsPos[4], wheelsPosLL, wheelsPosUL);
                            break;
                        case 17:
                            tree.wheelsSpeed[4] += GenerateValidRandFloat(-wheelSpeedLCR, wheelSpeedLCR);
                            tree.wheelsSpeed[4] = TrimValue<float>(tree.wheelsSpeed[4], wheelsSpeedLL, wheelsSpeedUL);
                            break;
                        case 18:
                            tree.wheelsSize[4] += GenerateValidRandFloat(-wheelSizeLCR, wheelSizeLCR);
                            tree.wheelsSize[4] = TrimValue<float>(tree.wheelsSize[4], wheelsSizeLL, wheelsSizeUL);
                            break;
                        case 19:
                            tree.wheelsPos[5] += GenerateValidRandFloat(-wheelPosLCR, wheelPosLCR);
                            tree.wheelsPos[5] = TrimValue<float>(tree.wheelsPos[5], wheelsPosLL, wheelsPosUL);
                            break;
                        case 20:
                            tree.wheelsSpeed[5] += GenerateValidRandFloat(-wheelSpeedLCR, wheelSpeedLCR);
                            tree.wheelsSpeed[5] = TrimValue<float>(tree.wheelsSpeed[5], wheelsSpeedLL, wheelsSpeedUL);
                            break;
                        case 21:
                            tree.wheelsSize[5] += GenerateValidRandFloat(-wheelSizeLCR, wheelSizeLCR);
                            tree.wheelsSize[5] = TrimValue<float>(tree.wheelsSize[5], wheelsSizeLL, wheelsSizeUL);
                            break;
                            #endregion
                    }
                    tree.value = -1;
                    float finalValue = float.MaxValue;
                    float[] values = new float[levelOfDependabality];
                    for (int k = 0; k < levelOfDependabality; k++)
                    {
                        if (k > 0)
                        {
                            if (values[k - 1] < finalValue)
                                finalValue = values[k - 1];
                            Debug.Log("finval" + values[k]);
                            tree.value = -1;
                            GenerateTruck(tree, finalValue);
                            yield return new WaitUntil(() => tree.value != -1);
                            values[k] = tree.value;
                        }
                        else
                        {
                            tree.value = -1;
                            GenerateTruck(tree);
                            yield return new WaitUntil(() => tree.value != -1);
                            values[k] = tree.value;
                            Debug.Log("finval" + values[k]);
                        }
                    }
                    finalValue = float.MaxValue;
                    for (int z = 0; z < levelOfDependabality; z++)
                        if (values[z] < finalValue)
                            finalValue = values[z];
                    tree.value = finalValue;
                    while (truckTrees.ContainsKey(tree.value))
                    {
                        Debug.Log("while6");
                        tree.value += 0.000001f;
                    }
                    truckTrees.Add(tree.value, tree);
                    fitnessText.text = "Fitness : " + tree.value;
                }
            }
        }
        WriteTreeList("Round : " + round + phaseText.text);
        for (int i = 0; i < truckTrees.Count; i++)
            WriteTreeList(JsonUtility.ToJson(truckTrees.ElementAt(i).Value));
        PopulationLimiting();
    }

    private void PopulationLimiting()
    {
        phaseText.text = "Population Limiting";
        candidatePopulation = new List<TreeStructure>();
        List<TreeStructure> tempTreeList = new List<TreeStructure>(truckTrees.Values.ToList());
        for (int j = 0; j < tempTreeList.Count; j++)
        {
            TreeStructure tree = tempTreeList[j];
            if (tree.age > lifeTime)
            {
                candidatePopulation.Add(tree);
                truckTrees.Remove(tree.value);
            }
        }
        while (truckTrees.Count > areaLimit)
        {
            candidatePopulation.Add(truckTrees.ElementAt(0).Value);
            truckTrees.RemoveAt(0);
        }
        WriteTreeList("Round : " + round + phaseText.text);
        for (int i = 0; i < truckTrees.Count; i++)
            WriteTreeList(JsonUtility.ToJson(truckTrees.ElementAt(i).Value));
        StartCoroutine(GlobalSeeding());
    }

    IEnumerator GlobalSeeding()
    {
        int iteration = 0;
        phaseText.text = "Global Seeding";
        int numOfGlobalSeeds = (int)(transferRate * candidatePopulation.Count);
        for (int i = 0; i < numOfGlobalSeeds; i++)
        {
            iterationText.text = "Iteration : " + iteration;
            iteration++;
            int rand = UnityEngine.Random.Range(0, candidatePopulation.Count);
            TreeStructure tree = new TreeStructure();

            tree.Clone(candidatePopulation[rand]);
            candidatePopulation.RemoveAt(rand);
            List<int> globalRands = new List<int>();
            for (int j = 0; j < GSC; j++)
            {
                int UL = 3 + tree.numOfWheels * 3;
                UL++;
                int random = UnityEngine.Random.Range(0, UL);
                while (random == UL || globalRands.Contains(random))
                {
                    random = UnityEngine.Random.Range(0, UL);
                }
                globalRands.Add(random);
            }
            if (globalRands.Contains(3))
            {
                int lastNumOfWheels = tree.numOfWheels;
                tree.numOfWheels = GenerateValidRandInt(numOfWheelsLL, numOfWheelsUL + 1);
                if (tree.numOfWheels > lastNumOfWheels)
                {
                    for (int j = 0; j < (tree.numOfWheels - lastNumOfWheels); j++)
                    {
                        tree.wheelsPos[lastNumOfWheels + j] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                        tree.wheelsSize[lastNumOfWheels + j] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                        tree.wheelsSpeed[lastNumOfWheels + j] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                    }
                }
                globalRands = new List<int>();
                for (int j = 0; j < GSC - 1; j++)
                {
                    int UL = 3 + tree.numOfWheels * 3;
                    UL++;
                    int random = UnityEngine.Random.Range(0, UL);
                    while (random == UL || random == 3 || globalRands.Contains(random))
                    {
                        random = UnityEngine.Random.Range(0, UL);
                    }
                    globalRands.Add(random);
                }
                GlobalRandsMapper(globalRands, tree);
            }
            else
                GlobalRandsMapper(globalRands, tree);

            float finalValue = float.MaxValue;
            float[] values = new float[levelOfDependabality];
            for (int k = 0; k < levelOfDependabality; k++)
            {
                if (k > 0)
                {
                    if (values[k - 1] < finalValue)
                        finalValue = values[k - 1];
                    Debug.Log("finval" + values[k]);
                    tree.value = -1;
                    GenerateTruck(tree, finalValue);
                    yield return new WaitUntil(() => tree.value != -1);
                    values[k] = tree.value;
                }
                else
                {
                    tree.value = -1;
                    GenerateTruck(tree);
                    yield return new WaitUntil(() => tree.value != -1);
                    values[k] = tree.value;
                    Debug.Log("finval" + values[k]);
                }
            }
            finalValue = float.MaxValue;
            for (int z = 0; z < levelOfDependabality; z++)
                if (values[z] < finalValue)
                    finalValue = values[z];
            tree.value = finalValue;
            while (truckTrees.ContainsKey(tree.value))
            {
                tree.value += 0.000001f;
            }
            tree.age = 0;
            truckTrees.Add(tree.value, tree);
            fitnessText.text = "Fitness : " + tree.value;
        }
        WriteTreeList("Round : " + round + phaseText.text);
        for (int i = 0; i < truckTrees.Count; i++)
            WriteTreeList(JsonUtility.ToJson(truckTrees.ElementAt(i).Value));
        UpdateTheBestTree();
    }

    private void UpdateTheBestTree()
    {
        truckTrees.ElementAt(truckTrees.Count - 1).Value.age = 0;
        Debug.Log("Round1Finished");
        StartCoroutine(LocalSeeding());
        WriteTreeList("Round : " + round + " Jungle \n");
        for (int i = 0; i < truckTrees.Count; i++)
            WriteTreeList(JsonUtility.ToJson(truckTrees.ElementAt(i).Value));
        round++;
    }

    public void GenerateTruck(TreeStructure tree)
    {
        GameObject truck = Instantiate(bodies[tree.body], startPoint.transform.position, Quaternion.identity);
        truck.GetComponent<TruckWatcher>().originTree = tree;
        var tempTree = truck.GetComponent<TreeStructure>();
        tempTree.Clone(tree);



        Vector2 wheelAPointW = GameObject.FindGameObjectWithTag("TireAPoint").transform.position;
        Vector2 wheelBPointW = GameObject.FindGameObjectWithTag("TireBPoint").transform.position;

        Vector2 wheelAPointL = GameObject.FindGameObjectWithTag("TireAPoint").transform.localPosition;
        Vector2 wheelBPointL = GameObject.FindGameObjectWithTag("TireBPoint").transform.localPosition;

        float tiresMaxDistanceW = wheelBPointW.x - wheelAPointW.x;
        float tiresMaxDistanceL = wheelBPointL.x - wheelAPointL.x;


        for (int i = 0; i < tempTree.numOfWheels; i++)
        {
            var tempWheel = Instantiate(wheel, new Vector2(tempTree.wheelsPos[i] * tiresMaxDistanceW + wheelAPointW.x, wheelAPointW.y),
                Quaternion.identity);
            tempWheel.transform.localScale = new Vector3(tempTree.wheelsSize[i], tempTree.wheelsSize[i],
                tempTree.wheelsSize[i]);
            WheelJoint2D tempWheelJoint = tempWheel.GetComponent<WheelJoint2D>();
            tempWheelJoint.enableCollision = false;
            tempWheelJoint.connectedBody = truck.GetComponent<Rigidbody2D>();
            tempWheelJoint.anchor = new Vector2(0, 0);
            tempWheelJoint.connectedAnchor = new Vector2(tempTree.wheelsPos[i] * tiresMaxDistanceL + wheelAPointL.x, wheelAPointL.y);
            JointSuspension2D tempJointSuspension = new JointSuspension2D
            {
                dampingRatio = 1,
                frequency = 30,
                angle = 90,
            };
            tempWheelJoint.useMotor = true;
            JointMotor2D tempMotor = new JointMotor2D
            {
                motorSpeed = tempTree.wheelsSpeed[i],
                maxMotorTorque = 5000
            };
            tempWheelJoint.motor = tempMotor;
            tempWheel.transform.SetParent(truck.transform);
        }

        Instantiate(boxSets[tempTree.numOfBoxes], GameObject.FindGameObjectWithTag("BoxesCenter").transform.position,
            Quaternion.identity);

        var boxes = GameObject.FindGameObjectsWithTag("Box");

        foreach (GameObject tempBox in boxes)
        {
            tempBox.GetComponent<Rigidbody2D>().mass = tempTree.weightOfBoxes;
        }

    }

    public void GenerateTruck(TreeStructure tree, float threshValue)
    {
        GameObject truck = Instantiate(bodies[tree.body], startPoint.transform.position, Quaternion.identity);
        truck.GetComponent<TruckWatcher>().originTree = tree;
        truck.GetComponent<TruckWatcher>().thresholdValue = threshValue;
        var tempTree = truck.GetComponent<TreeStructure>();
        tempTree.Clone(tree);



        Vector2 wheelAPointW = GameObject.FindGameObjectWithTag("TireAPoint").transform.position;
        Vector2 wheelBPointW = GameObject.FindGameObjectWithTag("TireBPoint").transform.position;

        Vector2 wheelAPointL = GameObject.FindGameObjectWithTag("TireAPoint").transform.localPosition;
        Vector2 wheelBPointL = GameObject.FindGameObjectWithTag("TireBPoint").transform.localPosition;

        float tiresMaxDistanceW = wheelBPointW.x - wheelAPointW.x;
        float tiresMaxDistanceL = wheelBPointL.x - wheelAPointL.x;


        for (int i = 0; i < tempTree.numOfWheels; i++)
        {
            var tempWheel = Instantiate(wheel, new Vector2(tempTree.wheelsPos[i] * tiresMaxDistanceW + wheelAPointW.x, wheelAPointW.y),
                Quaternion.identity);
            tempWheel.transform.localScale = new Vector3(tempTree.wheelsSize[i], tempTree.wheelsSize[i],
                tempTree.wheelsSize[i]);
            WheelJoint2D tempWheelJoint = tempWheel.GetComponent<WheelJoint2D>();
            tempWheelJoint.enableCollision = false;
            tempWheelJoint.connectedBody = truck.GetComponent<Rigidbody2D>();
            tempWheelJoint.anchor = new Vector2(0, 0);
            tempWheelJoint.connectedAnchor = new Vector2(tempTree.wheelsPos[i] * tiresMaxDistanceL + wheelAPointL.x, wheelAPointL.y);
            JointSuspension2D tempJointSuspension = new JointSuspension2D
            {
                dampingRatio = 1,
                frequency = 30,
                angle = 90,
            };
            tempWheelJoint.useMotor = true;
            JointMotor2D tempMotor = new JointMotor2D
            {
                motorSpeed = tempTree.wheelsSpeed[i],
                maxMotorTorque = 5000
            };
            tempWheelJoint.motor = tempMotor;
            tempWheel.transform.SetParent(truck.transform);
        }

        Instantiate(boxSets[tempTree.numOfBoxes], GameObject.FindGameObjectWithTag("BoxesCenter").transform.position,
            Quaternion.identity);

        var boxes = GameObject.FindGameObjectsWithTag("Box");

        foreach (GameObject tempBox in boxes)
        {
            tempBox.GetComponent<Rigidbody2D>().mass = tempTree.weightOfBoxes;
        }

    }


    private int GenerateValidRandInt(int randLL, int randUL)
    {
        if (Mathf.Abs(randUL - randLL) <= 1)
        {
            return randLL;
        }
        int intRand = UnityEngine.Random.Range(randLL, randUL);
        while (intRand == randUL)
        {
            intRand = UnityEngine.Random.Range(randLL, randUL);
        }
        return intRand;
    }

    private float GenerateValidRandFloat(float randLL, float randUL)
    {
        float floatRand = UnityEngine.Random.Range(randLL, randUL);
        return floatRand;
    }

    private T TrimValue<T>(T var, T trimLL, T trimUL) where T : System.IComparable<T>
    {
        if (var.CompareTo(trimLL) < 0)
            var = trimLL;
        else if (var.CompareTo(trimUL) > 0)
            var = trimUL;
        return var;
    }

    private void GlobalRandsMapper(List<int> list, TreeStructure tree)
    {
        for (int z = 0; z < list.Count; z++)
        {
            switch (list[z])
            {

                case 0:
                    tree.numOfBoxes = GenerateValidRandInt(numOfBoxesLL, numOfBoxesUL + 1);
                    break;
                case 1:
                    tree.body = GenerateValidRandInt(bodyLL, bodyUL + 1);
                    break;
                case 2:
                    tree.weightOfBoxes = GenerateValidRandFloat(weightOfBoxesLL, weightOfBoxesUL);
                    break;
                case 3:
                    tree.numOfWheels = GenerateValidRandInt(numOfWheelsLL, numOfWheelsUL + 1);
                    break;
                case 4:
                    tree.wheelsPos[0] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                    break;
                case 5:
                    tree.wheelsSpeed[0] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                    break;
                case 6:
                    tree.wheelsSize[0] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                    break;
                case 7:
                    tree.wheelsPos[1] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                    break;
                case 8:
                    tree.wheelsSpeed[1] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                    break;
                case 9:
                    tree.wheelsSize[1] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                    break;
                case 10:
                    tree.wheelsPos[2] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                    break;
                case 11:
                    tree.wheelsSpeed[2] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                    break;
                case 12:
                    tree.wheelsSize[2] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                    break;
                case 13:
                    tree.wheelsPos[3] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                    break;
                case 14:
                    tree.wheelsSpeed[3] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                    break;
                case 15:
                    tree.wheelsSize[3] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                    break;
                case 16:
                    tree.wheelsPos[4] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                    break;
                case 17:
                    tree.wheelsSpeed[4] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                    break;
                case 18:
                    tree.wheelsSize[4] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                    break;
                case 19:
                    tree.wheelsPos[5] = GenerateValidRandFloat(wheelsPosLL, wheelsPosUL);
                    break;
                case 20:
                    tree.wheelsSpeed[5] = GenerateValidRandFloat(wheelsSpeedLL, wheelsSpeedUL);
                    break;
                case 21:
                    tree.wheelsSize[5] = GenerateValidRandFloat(wheelsSizeLL, wheelsSizeUL);
                    break;
            }
        }
    }

    //private void OnDestroy()
    //{
    //    WriteToFile("---------------------Trees---------------------");
    //    for (int i = 0; i < truckTrees.Count; i++)
    //    {
    //        WriteToFile(truckTrees.ElementAt(i).Value);
    //    }
    //    Debug.Log("bomb");
    //}

}