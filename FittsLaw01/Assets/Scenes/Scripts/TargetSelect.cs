using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.IO;
using System;
using System.Linq;

public class TargetSelect : MonoBehaviour
{

    private float timer = 0.0f;
    private GameObject current_target;
    private bool select = false;
    private int current_target_number;
    private int total_targets;

    public int max_selections = 80;
    public int number_of_targets = 8;
    public GameObject targetPrefab;
    public OVRCameraRig cameraRig;

    List<int> numList;

    private StreamWriter writer_all;
    private StreamWriter writer;

    static private int windowSize = 20;
    static private int numberFilters = 20;

    private float[][] weights = new float[numberFilters][];
    private float[] xData = new float[windowSize];
    private float[] yData = new float[windowSize];

    private float[] distance = new float[20];
    private float[] deltaX = new float[windowSize];
    private float[] deltaY = new float[windowSize];
    private float previousX = 0.0f;
    private float previousY = 0.0f;

    private Vector3 touchPosition;
    private Quaternion touchRotation;
    private Vector3 headsetPosition;
    private Quaternion headsetRotation;
    private float xf;
    private float yf;
    private Vector3 temp;

    private int[] targetOrder = { 1, 5, 2, 6, 3, 7, 4, 8, 5, 1, 6, 2, 7, 3, 8, 4 };
    private float[][] expOrder = new float[][]
    {
        new float[] { 0.5f, 2f, 1f,    1f },
        new float[] { 5f, 3f, 4f,    4f }
    };



    private int[] filtOrder = {1, 0, 0, 1, 1, 0, 0, 1 };

    private int currentExp = 0;
    private int currentFilt = 0;
    private int running = 0;

    private Camera cam;
    StreamWriter writerTargets;


    // Start is called before the first frame update
    void Start()
    {
        current_target_number = -1;
        numList = new List<int>();
        writer_all = new StreamWriter("all_data.txt", false);
        writer = new StreamWriter("data.txt", false);
        writerTargets = new StreamWriter("target.txt", false);
        print("start");

        total_targets = 0;
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();

        //set filter weights 
        for(int i = 0; i<numberFilters; i++)
        {
            //Set filter weights for k = 0.5 to numFIlters*0.25 (e.g. 5.25)
            weights[i] =  new float[windowSize];
            float k = 0.5f + (float)i * 0.6f;
            SetFilterWeights(k,windowSize,i);
        }
        GenerateTargets(expOrder[1][currentExp], expOrder[0][currentExp], 0);
        currentExp += 1;
        running = 0;

        cam = GetComponent<Camera>();


        writer.WriteLine("Fitts Law Experiment");
        writer.WriteLine("Subject N: CABD");
        writer.WriteLine("FOV: " + Camera.main.fieldOfView.ToString());
        writer.WriteLine("Width: " + Screen.width.ToString());
        writer.WriteLine("Height: " + Screen.height.ToString());
        writer.WriteLine("XRHieght: " + XRSettings.eyeTextureHeight.ToString());
        writer.WriteLine("XRWidth: " + XRSettings.eyeTextureWidth.ToString());
        writer.WriteLine("Target#" + " " + "Filter?" + " " + "Time" + " " + "Touch Pos" + " " + "Touch Rot" + " " + "Head Pos" + " " + "Head Rot" + " " + "xfilter" + " " + "yfilter" + " " + "zfilter");


    }

    void StartTrial()
    {
        current_target_number = -1;
        total_targets = 0;
        DestroyTargets();

        int dist = 0;
        if (currentExp > 2) dist = 1;
        GenerateTargets(expOrder[1][currentExp], expOrder[0][currentExp],dist);
        currentExp += 1;
        running = 0;
    }



    // Update is called once per frame
    void Update()
    {

        //print("Width " + XRSettings.eyeTextureWidth.ToString() + XRSettings.eyeTextureHeight.ToString() + " " + XRSettings.eyeTextureResolutionScale.ToString());

        timer += Time.deltaTime; //track time
        select = false;
        GameObject go;

        headsetPosition = cameraRig.centerEyeAnchor.position;
        touchPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) + new Vector3(headsetPosition.x, headsetPosition.y, headsetPosition.z+0.5f); //headsetPosition;
        headsetRotation = cameraRig.centerEyeAnchor.rotation;
        touchRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);

        //Vector3 temp = touchRotation * (new Vector3(0, 30, 100));
        temp = touchRotation * (new Vector3(0, 0, 100));

        float[] xData_s = xData;
        xData[0] = (float)temp.x;
        Array.Copy(xData_s, 0, xData, 1, windowSize - 1);

        float[] yData_s = yData;
        yData[0] = (float)temp.y;
        Array.Copy(yData_s, 0, yData, 1, windowSize - 1);

        float[] deltaX_s = deltaX;
        deltaX[0] = temp.x - previousX;
        Array.Copy(deltaX_s, 0, deltaX, 1, windowSize-1);

        float[] deltaY_s = deltaY;
        deltaY[0] = temp.y - previousY;
        Array.Copy(deltaY_s, 0, deltaY, 1, windowSize-1);

        

        previousX = temp.x;
        previousY = temp.y;

        float deltaX_sum = 0.0f;
        float deltaY_sum = 0.0f;

       for(int i = 0; i< windowSize; i++)
        {
            deltaX_sum = deltaX_sum + deltaX[i];
            deltaY_sum = deltaY_sum + deltaY[i];
        }

        deltaX_sum = Mathf.Abs(deltaX_sum);
        deltaY_sum = Mathf.Abs(deltaY_sum);

        print(Time.deltaTime.ToString() + " : " + deltaX_sum.ToString());

        float delta_total = Mathf.Pow(deltaX_sum + deltaY_sum, 2);

        float effectiveDistance = distance.Max();
        if (effectiveDistance > 100 || effectiveDistance == 0.0f)
        {
            effectiveDistance = 10.0f;
        }



        float filterMetric = Mathf.Pow(deltaX_sum + deltaY_sum, 2);
        //float filterMetric = 0;

        float aa = 0f;
        float bb =  900f + 10f*effectiveDistance;

        //int filterIndexX = 0;

        /*
        if (effectiveDistance <= 30f)
        {
            aa = 0.0f;// effectiveDistance / 20f;
            bb = 100f*effectiveDistance;
        }
        else
        {
            aa = 1.5f + (effectiveDistance - 30f);
            bb = 3.0f + 2f * (effectiveDistance - 30f);
        }
       

        if(delta_total >= aa && delta_total < bb)
        {
            filterIndexX = Mathf.RoundToInt((delta_total - aa) * 19f / (bb - aa));
        }
        else if (delta_total >= bb)
        {
            filterIndexX = 19;
        }

         */
        

        int filterIndexX = Mathf.RoundToInt(map(filterMetric, 0, 500, 0, 19));
        int filterIndexY = filterIndexX;

        

        if (filterIndexX > 19) filterIndexX = 19;
        if (filterIndexX < 0) filterIndexX = 0;
        if (filterIndexY > 19) filterIndexY = 19;
        if (filterIndexY < 0) filterIndexY = 0;

        xf = temp.x;
        yf = temp.y;

        //print("index: " + filterIndexX.ToString());

        if (filtOrder[currentFilt] == 1)
        {
            xf = FilterData(xData, weights[filterIndexX], windowSize);
            yf = FilterData(yData, weights[filterIndexY], windowSize); ; //FilterData(xData, weights[1], windowSize);
        }
    
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0,touchPosition);
        lineRenderer.SetPosition(1, new Vector3(xf,yf,temp.z));
        
        float[] distance_s = distance;
        distance[0] = GetDistance(touchPosition, new Vector3(xf, yf, temp.z) - touchPosition);
        if (distance[0] > 100f)
        {
            distance[0] = 10f;
            if(distance.Max() > 20f) distance[0] = distance.Max() - 5.0f;
        }//cheating?
        Array.Copy(distance_s, 0, distance, 1, 19);



        //wait for click to start
        if (OVRInput.GetDown(OVRInput.RawButton.A) && running == 0)
        {
            print("Started Trial");
            running = 1;
            //writer.WriteLine("New Trial " + timer.ToString() + " " + Input.mousePosition.x + " " + Input.mousePosition.y + " " + Input.mousePosition.z);
            GetNewTarget();
        }

            //if selection made
            if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            go = GetSelectedObject(touchPosition, new Vector3(xf, yf, temp.z) - touchPosition); 
            if (go != null) IsTargetSelected(go, current_target);
        }

        SaveData(timer, select);
    }

    //------------------------------------------------------------------------

    private void SaveData(float timer, bool select)
    {
        //save x,y,t, and if target selected to txt file 
        writer_all.WriteLine(current_target_number + " " + filtOrder[currentFilt].ToString() + " " + timer.ToString() + " " + touchPosition.ToString("F4") + " " + touchRotation.ToString("F4") + " " + headsetPosition.ToString("F4") + " " + headsetPosition.ToString("F4") + " " + xf.ToString() + " " + yf.ToString() + " " + temp.z.ToString());


        return;
    }

    private GameObject GetSelectedObject(Vector3 origin, Vector3 forward)
    {
        //create ray in direction of camera
        print("Click!");
        RaycastHit hit;
        Ray ray = new Ray(origin, forward);

        Debug.DrawRay(origin, forward, Color.green);

        //if hit object, return it
        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            if (hit.transform)  return hit.transform.gameObject;
            else return null;
        }
        else
        {
            return null;
        }
    }

    private float GetDistance(Vector3 origin, Vector3 forward)
    {
        RaycastHit hit;
        Ray ray = new Ray(origin, forward);
        Physics.Raycast(ray, out hit, 1000.0f);
        return hit.distance;
    }

    private bool IsTargetSelected(GameObject selected, GameObject target)
    {
        if (selected == target)
        {
            //if the correct target has been selected, calculate 
            GetNewTarget();
            select = true;
            print("Success tARGET!");
        }
        return (selected == target);
    }

    private void GetNewTarget()
    {


        writer.WriteLine(current_target_number.ToString() + " " + filtOrder[currentFilt].ToString() +  " " + timer.ToString() + " " + touchPosition.ToString("F4") + " " + touchRotation.ToString("F4") + " " + headsetPosition.ToString("F4") + " " + headsetRotation.ToString("F4") + " " + xf.ToString() + " " + yf.ToString() + " " + temp.z.ToString());
        print(touchRotation.ToString("G4"));
        //change colour back to original
        if (current_target_number > -1) current_target.GetComponent<Renderer>().material.color = new Color(0.359f, 0.568f, 0.802f);

        //get new target
        total_targets += 1;

        if (total_targets == 1 +  max_selections / 2) currentFilt += 1;
        if (total_targets > max_selections && currentExp < 4)
        {
            currentFilt += 1;
            StartTrial();
            return;
        }
        else if(total_targets > max_selections && currentExp > 3.5)
        {
            writer_all.Close();
            writer.Close();
            print("Done!");

            Application.Quit();
            return;
        }

        current_target_number += 1; 
        if (current_target_number > 15) current_target_number = 0;

        current_target = GameObject.Find("Target" + targetOrder[current_target_number].ToString());

        //highlight new target
        current_target.GetComponent<Renderer>().material.color = new Color(0, 1.0f, 0);

        //TODO calculate ID and save

        return;
    }


    void GenerateTargets(float R, float W, int dist)
    {
        float r = R / Mathf.Sqrt(2);
        float[] depth  = { 0, 30, -5, 15, 80, 25, 50, 20 };
      

        for (int i = 1; i <= 8; i++)
        {
            float theta = (i - 1) * Mathf.PI / 4;
            Vector3 StartPoint = Vector3.zero;
            StartPoint.Set(R * Mathf.Cos(theta), 30.0f+ R * Mathf.Sin(theta), 10.0f + (float)dist*depth[i-1]);
            GameObject go = GameObject.Instantiate(targetPrefab, StartPoint, Quaternion.AngleAxis(90, Vector3.right)); 
            go.name = "Target" + i.ToString();
            go.transform.localScale = W * go.transform.localScale; 
        }
    }

    void DestroyTargets()
    {
        for (int i = 1; i <= 8; i++)
        {
            Destroy(GameObject.Find("Target" + i.ToString()));
            
        }
    }

    float FilterData(float[] x, float[] h, int window)
    {
        float y = 0;
        for(int i = 0; i< window; i++)
        {
            y = y + h[i] * x[i];

        }

        return y;
    }

    void SetFilterWeights(float k, int window, int index)
    {
        float weightSum = 0;
        for(int i = 0; i< window; i++)
        {
            weights[index][i] = 1.0f / Mathf.Pow((float)(i+1) , k);
            weightSum = weightSum + weights[index][i];
        }
        for (int i = 0; i < window; i++)
        {
            weights[index][i] = weights[index][i] / weightSum;
        }
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    /*
    float getSize(Transform target)
    {

        diameter = target.GetComponent<Collider>().bounds.extents.magnitude;

        distanceCam = Vector3.Distance(target.position, Camera.main.transform.position);
        angularSize = (diameter / distanceCam) * Mathf.Rad2Deg;
        pixelSize = ((angularSize * Screen.height) / Camera.main.fieldOfView);

        return pixelSize;
    }
    */

}

