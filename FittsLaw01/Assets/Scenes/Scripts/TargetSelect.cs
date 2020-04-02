using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TargetSelect : MonoBehaviour
{

    private float timer = 0.0f;
    private GameObject current_target;
    private bool select = false;
    private int current_target_number;
    private int total_targets;

    public int max_selections = 10;
    public int number_of_targets = 16;

    List<int> numList;

    private StreamWriter writer_all;
    private StreamWriter writer;

    // Start is called before the first frame update
    void Start()
    {
        current_target_number = 0;
        numList = new List<int>();
        writer_all = new StreamWriter("Assets/Data/all_data.txt", true);
        writer = new StreamWriter("Assets/Data/data.txt", true);
        print("start");

        MakeNewList();
        total_targets = 0;
        GetNewTarget();

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime; //track time
        select = false;
        GameObject go;

        //if selection made
        if (Input.GetMouseButtonDown(0))
        {
            go = GetSelectedObject();
            if (go != null) IsTargetSelected(go, current_target);
        }

        SaveData(timer, select);
    }

    //------------------------------------------------------------------------

    private void SaveData(float timer, bool select)
    {
        //save x,y,t, and if target selected to txt file 
        writer_all.WriteLine(timer.ToString() + " " + select.ToString() + " " + Input.mousePosition.x + " " + Input.mousePosition.y + " " + Input.mousePosition.z);
        
        return;
    }

    private GameObject GetSelectedObject()
    {
        //create ray in direction of camera
        print("Click!");
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if hit object, return it
        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            if (hit.transform) return hit.transform.gameObject;
            else return null;
        }
        else
        {
            return null;
        }
    }

    private bool IsTargetSelected(GameObject selected, GameObject target)
    {
        if (selected == target)
        {
            //if the correct target has been selected, calculate 
            GetNewTarget();
            select = true;
            print("Success!");
        }
        return (selected == target);
    }

    private void GetNewTarget()
    {


        writer.WriteLine(current_target_number + " " + timer.ToString() + " " + Input.mousePosition.x + " " + Input.mousePosition.y + " " + Input.mousePosition.z);
        //change colour back to original
        if (current_target_number > 0) current_target.GetComponent<Renderer>().material.color = new Color(0.359f, 0.568f, 0.802f);

        //get new target
        total_targets += 1;

        if (total_targets > max_selections)
        {
            //TODO: exit game, run plotting scripts 

            writer_all.Close();
            writer.Close();
            print("Done!");
            Application.Quit();
        }

        if(numList.Count == 0)
        {
            MakeNewList();
        }

        int index = Random.Range(0, numList.Count);
        print(index);
        current_target_number = numList[index];
        print(current_target_number);
        numList.RemoveAt(index);

        current_target = GameObject.Find("Target" + (current_target_number).ToString());

        //highlight new target
        current_target.GetComponent<Renderer>().material.color = new Color(0, 1.0f, 0);

        //TODO calculate ID and save

        return;
    }

    void MakeNewList()
    {
        int j = 1;
        while (j <= number_of_targets)
        {
            numList.Add(j++);
        }
    }
}

