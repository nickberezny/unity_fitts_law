using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class GetTargetData : MonoBehaviour
{

    public int number_of_targets = 16;

    private Transform target;
    private float distance;
    private float diameter;
    private float angularSize;
    private float pixelSize;
    private Vector3 scrPos;
    private Camera cam;

    private float[] sizes;
    private Vector3[] positions;


    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        StreamWriter writer = new StreamWriter("target.txt", false);

        sizes = new float[number_of_targets];
        positions = new Vector3[number_of_targets];

        for (int i=1;i<number_of_targets+1;i++)
        {
            target = GameObject.Find("Target" + i.ToString()).transform;
            sizes[i-1] = getSize(target);
            positions[i-1] = cam.WorldToScreenPoint(target.position);
            writer.WriteLine(sizes[i-1] + " " + positions[i-1].x + " "+ positions[i-1].y);

        }

        writer.Close();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float getSize(Transform target)
    {
        
        diameter = target.GetComponent<Collider>().bounds.extents.magnitude;

        distance = Vector3.Distance(target.position, Camera.main.transform.position);
        angularSize = (diameter / distance) * Mathf.Rad2Deg;
        pixelSize = ((angularSize * Screen.height) / Camera.main.fieldOfView);

        return pixelSize;
        //print(pixelSize);
    }

}
