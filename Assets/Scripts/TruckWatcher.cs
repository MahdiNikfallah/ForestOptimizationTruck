using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.IO;

public class TruckWatcher : MonoBehaviour
{

    public float timeToCheckMovement = 1f;
    public float horizontalMovementThreshold = 0.1f;
    public float verticalMovementThreshold = 0.1f;
    public float boxValue = 20f;
    public int onBoardBoxes;
    public float value;
    public float boxesValueMultiplication = 0.25f;
    public TreeStructure originTree;

    GameObject[] boxes;

    private Vector2 lastPosYC = new Vector2();
    private Vector2 lastPosXC = new Vector2();
    private float lastValueUpdatePosx = -10.14798f;
    public bool isMovedVertical = false;
    public bool isMovedHorizontal = false;
    private float maxNumOfBoxes = 6;

    private Coroutine xc;
    private Coroutine yc;

    public bool DistOrBox = false;
    public float thresholdValue = float.MaxValue;

    private void Awake()
    {
        thresholdValue = 4000;
    }
    // Start is called before the first frame update
    void Start()
    {
        boxes = GameObject.FindGameObjectsWithTag("Box");
        Debug.Log(boxes.Length);
        CameraFollow();
        lastValueUpdatePosx = GameObject.FindObjectOfType<FOAManager>().startPoint.transform.position.x;
        maxNumOfBoxes = GameObject.FindObjectOfType<FOAManager>().numOfBoxesUL;
        lastPosYC = gameObject.transform.position;
        onBoardBoxes = boxes.Length;
        yc = StartCoroutine(CheckStopY());
        xc = StartCoroutine(CheckStopX());
    }

    // Update is called once per physics update
    private void FixedUpdate()
    {
        if (FitnessFunction() > thresholdValue + 100)
        {
            Debug.Log("fitness: " + FitnessFunction() + "thresholdValue" + thresholdValue);
            StopCoroutine(xc);
            StopCoroutine(yc);
            value = FitnessFunction();
            SimFinished();
        }
        if (gameObject.transform.position.x - lastPosYC.x > horizontalMovementThreshold ||
            Mathf.Abs(gameObject.transform.position.y - lastPosYC.y) > verticalMovementThreshold)
        {
            lastPosYC = gameObject.transform.position;
            isMovedVertical = true;
        }
        if (gameObject.transform.position.x - lastPosXC.x > horizontalMovementThreshold)
        {
            lastPosXC = gameObject.transform.position;
            isMovedHorizontal = true;
        }

    }


    IEnumerator CheckStopY()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            yield return new WaitForSeconds(timeToCheckMovement);
            if (!isMovedVertical)
            {
                StopCoroutine(xc);
                value = FitnessFunction();
                SimFinished();
            }
            isMovedVertical = false;
        }
    }

    IEnumerator CheckStopX()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            yield return new WaitForSeconds(timeToCheckMovement * 2);
            if (!isMovedHorizontal)
            {
                StopCoroutine(yc);
                value = FitnessFunction();
                SimFinished();
            }
            isMovedHorizontal = false;
        }
    }

    //private void UpdateValue()
    //{
    //    float tempValue = (gameObject.transform.position.x - lastValueUpdatePosx) +
    //        (gameObject.transform.position.x - lastValueUpdatePosx) * (onBoardBoxes / maxNumOfBoxes) * boxesValueMultiplication;
    //    if (tempValue > 0)
    //        value += tempValue;
    //    lastValueUpdatePosx = gameObject.transform.position.x;
    //}

    private void CameraFollow()
    {
        FindObjectOfType<CinemachineVirtualCamera>().Follow = gameObject.transform;
    }

    private void SimFinished()
    {
        GetComponent<TreeStructure>().value = this.value;
        Destroy(gameObject);
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("BoxSet");
        foreach (var boxset in boxes)
        {
            Destroy(boxset);
        }
        Debug.Log("value : " + this.value);
        originTree.value = this.value;
        //Debug.Log("finished");
    }

    private float FitnessFunction()
    {
        float fitness = 0;
        if (!DistOrBox)
            foreach (GameObject box in boxes)
                fitness += box.transform.position.x;
        else
            fitness = transform.position.x;
        return fitness;
    }

    private void OnBoardCounter()
    {
        int temp = 0;
        foreach (GameObject box in boxes)
            if (box.GetComponent<BoxWatcher>().isOnBoard)
                temp++;
        onBoardBoxes = temp;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EndPoint")
        {
            StopCoroutine(xc);
            StopCoroutine(yc);
            if (!DistOrBox)
            {
                OnBoardCounter();
                if (onBoardBoxes == maxNumOfBoxes)
                {
                    FOAManager fOAManager = FindObjectOfType<FOAManager>();
                    File.AppendAllText(fOAManager.path, "Best Tree :  \n" +
                        "Round : " + fOAManager.round + "  " + fOAManager.phaseText.text + "  " +
                        fOAManager.iterationText.text + "\n" + JsonUtility.ToJson(GetComponent<TreeStructure>()) + "\n");
                    Application.Quit();
                }
                value = FitnessFunction();
                SimFinished();
            }
            else
            {
                FOAManager fOAManager = FindObjectOfType<FOAManager>();
                File.AppendAllText(fOAManager.path, "Best Tree :  \n" +
                    "Round : " + fOAManager.round + "  " + fOAManager.phaseText.text + "  " +
                    fOAManager.iterationText.text + "\n" + JsonUtility.ToJson(GetComponent<TreeStructure>()) + "\n");
                Application.Quit();
                value = FitnessFunction();
                SimFinished();
            }

        }
        if (collision.gameObject.tag == "Area")
        {
            if (GetComponent<AirResistance>().resistanceMultiplication !=
                collision.gameObject.GetComponent<AriResContainer>().airResistanceMultiplication)
            {
                GetComponent<AirResistance>().resistanceMultiplication =
                    collision.gameObject.GetComponent<AriResContainer>().airResistanceMultiplication;
            }
        }
    }
}
