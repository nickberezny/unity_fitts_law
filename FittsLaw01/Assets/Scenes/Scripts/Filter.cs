using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filter : MonoBehaviour
{

    public int M = 100; //filter order
    public float pi = Mathf.PI;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //calculate wc
        //calculate h
        //get new data
        //calculate y

        float k = 1; //TODO = update wc based on distance
        float d = 1; //TODO = get distance to target
        float v = 1; //TODO = get velocity 

        float[] hd, x;
        x = new float[M];
        float y = 0;

        hd = new float[M];

        //k = UpdatFilterParam();
        hd = GetWeights(k, M);
        y = FilterSignal(hd, x, M);

}

    private float[] GetWeights(float k, int M)
    {

        float sum = 0;
        float[] hd;
        hd = new float[M];

        //calc tap weights for LPF
        for (int i = 0; i < M; i++)
        {
            //hd[i] = 1 / (i ^ k);
            sum += hd[i];
        }
        for (int i = 0; i < M; i++)
        {
            hd[i] = hd[i] / sum;
        }


        return hd;
    }

    private void GetData()
    {
        //TODO
        //get new data from input device, update matrix
        //return x;
    }

    private float FilterSignal(float[] hd, float[] x, int M)
    {
        //get filtered y = hd*x

        float y = 0;
        for (int i = 0; i < M; i++)
        {
            y += hd[i] * x[i];
        }
        return y;
    }
}
