﻿using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class MazeAgent2 : Agent
{
    public float[] searchArea;
    public float[] peopleArea;
    public LayerMask rayLayerMask = Physics.DefaultRaycastLayers;
    public Vector3[] angles = {new Vector3(0f,0f,1f), new Vector3(0f,0f,-1f), new Vector3(1f,0f,0f), new Vector3(-1f,0f,0f), new Vector3(1f,0f,1f), new Vector3(-1f,0f,1f),new Vector3(1f,0f,-1f), new Vector3(-1f,0f,-1f)};
    public float[] agentSearchArea;
    public float[] agentPeopleArea;

    public Rigidbody agentRigidbody; 
    public Transform mazeSquares;
    public Transform people;
    public CreateMaze mazeScript;
    public int agent;
    private bool first = true;
    public enum mazeTypeEnum {One, Ten, Random};
    public mazeTypeEnum mazeType;

    void Start()
    {
        
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        /*
        for(int i = 0; i < searchArea.GetLength(0); i++)
        {

            sensor.AddObservation(searchArea[i]);
            if(searchArea[i] == 0f)
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
            else
            {
                sensor.AddObservation(mazeSquares.GetChild(i).transform.localPosition.x);
                sensor.AddObservation(mazeSquares.GetChild(i).transform.localPosition.z);
                for(int j = 1; j < mazeSquares.GetChild(i).childCount; j++)
                {
                    if(mazeSquares.GetChild(i).GetChild(j).tag ==  "Wall")
                    {
                        sensor.AddObservation(0f);
                    }
                    else
                    {
                        sensor.AddObservation(1f);
                    }
                }
            }
        }
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        */

        try
        {
            int xcoord = Mathf.FloorToInt(transform.localPosition.x)+10;
            int zcoord = Mathf.FloorToInt(transform.localPosition.z)+10;
            string s = "Maze Cell ";
            s += xcoord.ToString();
            s += ", ";
            s += zcoord.ToString();
            GameObject square = mazeSquares.transform.Find(s).gameObject; 

            for(int i = 1; i<5; i++){
                if(square.transform.GetChild(i).eulerAngles == new Vector3(0f,0f,0f)){
                    if(square.transform.GetChild(i).name.Contains("Maze Wall")){
                        sensor.AddObservation(1f);
                    } else {
                        sensor.AddObservation(0f);
                    }
                }
                if(square.transform.GetChild(i).eulerAngles == new Vector3(0f,90f,0f)){
                    if(square.transform.GetChild(i).name.Contains("Maze Wall")){
                        sensor.AddObservation(1f);
                    } else {
                        sensor.AddObservation(0f);
                    }
                }
                if(square.transform.GetChild(i).eulerAngles == new Vector3(0f,180f,0f)){
                    if(square.transform.GetChild(i).name.Contains("Maze Wall")){
                        sensor.AddObservation(1f);
                    } else {
                        sensor.AddObservation(0f);
                    }
                }
                if(square.transform.GetChild(i).eulerAngles == new Vector3(0f,270f,0f)){
                    if(square.transform.GetChild(i).name.Contains("Maze Wall")){
                        sensor.AddObservation(1f);
                    } else {
                        sensor.AddObservation(0f);
                    }
                }
            }
        }
        catch (MissingReferenceException)
        {
            print("MissRefExc");
            mazeSquares = mazeScript.maze.transform;
            people = mazeScript.people.transform;
        }

        

        /* string str = square.name;
        str += " ";
        foreach( var f in wallSetup )
        {
            str += f.ToString();
        }
        Debug.Log( str ); */

        int counter1 = 0;
        for(int i = 0; i <searchArea.GetLength(0); i++)
        {
            if(searchArea[i] == 1f)
            {
                counter1++;
            }
        }
        sensor.AddObservation(counter1);
        int counter = 0;
        for(int i = 0; i <peopleArea.GetLength(0); i++)
        {
            if(peopleArea[i] == 1f)
            {
                counter++;
            }
        }
        //a
        sensor.AddObservation(agent);
        sensor.AddObservation(counter);
        sensor.AddObservation(transform.InverseTransformDirection(agentRigidbody.velocity));
        //mazeScript.UpdateGraphics((counter1/4).ToString(), agent);
    }

    public override void AgentAction(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var forwardAxis = Mathf.FloorToInt(act[0]);
        switch (forwardAxis)
        {
            case 0:
                dirToGo = Vector3.zero;
                break;
            case 1:
                dirToGo = Vector3.forward;
                break;
            case 2:
                dirToGo = -Vector3.forward;
                break;
            case 3:
                dirToGo = Vector3.right;
                break;
            case 4:
                dirToGo = -Vector3.right;
                break;
        }
        agentRigidbody.AddForce(dirToGo * 0.5f, ForceMode.VelocityChange);
        for(int i = 0; i < angles.GetLength(0); i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position,transform.TransformDirection(angles[i]) , out hit, 5f , rayLayerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(angles[i]) * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");
                if(hit.collider.tag == "NotFound")
                {   
            //print("trigger");
                    if(searchArea[hit.collider.transform.parent.GetSiblingIndex()] == 0f)
                    {
                        FoundSquare(hit.collider.transform.parent.GetSiblingIndex());
                    }
                }
                if(hit.collider.tag == "Person")
                {
                    if(peopleArea[hit.collider.transform.GetSiblingIndex()] == 0f)
                    {
                        FoundPerson(hit.collider.transform.GetSiblingIndex());
                    }
                }
            }
        }
        if(CheckSearchArea()){Finished();}
        AddReward(-0.0001f);
    }
    public override void AgentReset()
    {
        searchArea = new float[400];
        peopleArea = new float[5];
        switch(mazeType)
        {
            case mazeTypeEnum.One:
            mazeScript.MakeMaze();
            break;
            case mazeTypeEnum.Ten:
            mazeScript.Make10Maze();
            break;
            case mazeTypeEnum.Random:
            mazeScript.MakeRandomMaze();
            break;
        }
        mazeSquares = mazeScript.maze.transform;
        people = mazeScript.people.transform;
        gameObject.transform.localPosition = new Vector3(-9.5f, 0.5f, -9.5f);
        agentRigidbody.velocity = new Vector3(0f, 0f, 0f);
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

    }
    public bool CheckSearchArea()
    {
        for( int i = 0; i <searchArea.GetLength(0);i++)
        {
            if(searchArea[i] == 0f){return false;}
        }
        return true;
    }

    public void OnCollisionEnter(Collision collisionInfo)
    {
        if(collisionInfo.collider.tag == "Agent")
        {
            agentSearchArea = collisionInfo.collider.gameObject.GetComponent<MazeAgent2>().searchArea;
            for( int i = 0; i <agentSearchArea.GetLength(0);i++)
            {
                if(agentSearchArea[i] == 1f && searchArea[i] == 0f)
                {
                    FoundSquare(i);
                }
            }
            agentPeopleArea = collisionInfo.collider.gameObject.GetComponent<MazeAgent2>().peopleArea;
            for( int i = 0; i <agentPeopleArea.GetLength(0);i++)
            {
                if(agentPeopleArea[i] == 1f && peopleArea[i] == 0f)
                {
                    FoundPerson(i);
                }
            }         
        }
    }

    public override float[] Heuristic()
    {
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { 3 };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { 1 };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new float[] { 4 };
        }
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { 2 };
        }
        return new float[] { 0 };
    }

    public virtual void FoundSquare(int position) 
    {
        try
        {
            searchArea[position] = 1f;
            Transform quad = mazeSquares.GetChild(position).GetChild(0);
            MazeCell cell = quad.parent.GetComponent<MazeCell>();
            
            if(quad.GetComponent<MeshRenderer>().material.color == Color.red)
            {
                quad.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
            else if(quad.GetComponent<MeshRenderer>().material.color == Color.yellow)
            {
                quad.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                quad.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            int layer = quad.gameObject.layer;
            if (layer == 9)
            {
                quad.gameObject.layer = 9 + agent;
            }
            else if(layer == 10 || layer == 11 || layer == 12)
            {
                quad.gameObject.layer = (layer + 9 + agent);
            }
            else
            {
                quad.gameObject.layer = 0;
            }
        }
        catch (MissingReferenceException)
        {
            print("MissRefExc");
            mazeSquares = mazeScript.maze.transform;
            people = mazeScript.people.transform;
        }

        float reward = 0.05f;
        AddReward(reward);
    }
    public void FoundPerson(int position)
    {
        try{
            peopleArea[position] = 1f;
            Transform person = people.GetChild(position);
            
            if(person.GetComponent<MeshRenderer>().material.color == Color.red)
            {
                person.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
            else if(person.GetComponent<MeshRenderer>().material.color == Color.yellow)
            {
                person.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                person.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            int layer = person.gameObject.layer;
            if (layer == 9)
            {
                person.gameObject.layer = 9 + agent;
            }
            else if(layer == 10 || layer == 11 || layer == 12)
            {
                person.gameObject.layer = (layer + 9 + agent);
            }
            else
            {
                person.gameObject.layer = 0;
            }
        }
        catch (MissingReferenceException)
        {
            print("MissRefExc");
            mazeSquares = mazeScript.maze.transform;
            people = mazeScript.people.transform;
        }
        float reward = 1f;
        AddReward(reward);
    }

    public virtual void DestroyMiniMap(){}

    public virtual void Finished()
    {
        GameObject parent = transform.parent.gameObject;
        Component[] CoopScript = parent.GetComponentsInChildren<MazeAgent2>();
        foreach(MazeAgent2 script in CoopScript)
        {
            script.FinishedCalled(searchArea);
        }
    }

    public void FinishedCalled(float[] finishedSearchArea)
    {
        /*
        for( int i = 0; i < finishedSearchArea.GetLength(0);i++)
        {
            if(finishedSearchArea[i] == 1f && searchArea[i] == 0f)
            {
                FoundSquare(i);
                //searchArea[i,j] = 1f;
                //AddReward(1f);
            }
        }
        */
    AddReward(2f);
    Done();
    }

}