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

        float wc = 2 * pi * 5; //TODO = update wc based on distance

        float[] hd, x;

        hd = new float[M];
        hd = GetWeights(wc, M);

        x = new float[M];

        float y = 0;
        y = FilterSignal(hd, x, M);

}

    private float[] GetWeights(float wc, int M)
    {

        float[] hd;
        hd = new float[M];

        //calc tap weights for LPF
        for (int i = 0; i < M; i++)
        {
            float n = i - M / 2;
            if (n == 0) hd[i] = 1 / pi * n;
            else hd[i] = Mathf.Sin(wc * n) / (pi * n);
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
