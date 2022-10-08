using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using extOSC;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] Utensils;
    private GameObject StonesOrigin;

    //counter for each utensil instances
    private int[] counter = new int[8];

    // for tracking objects that are currently touching the table
    List<GameObject> currentCollisions = new List<GameObject>();

    private void Start()
    {
        StonesOrigin = GameObject.Find("TableOrigin/StonesOrigin");

        for (int i = 0; i < 8; i++)
        {
            counter[i] = 0;
        }

        //SpawnAllUtensil();
    }
    


    // ------------spawn functions------------
    public void SpawnUtensil(int n)
    {
        GameObject myUtensil = Instantiate(Utensils[n]) as GameObject;
        myUtensil.transform.parent = StonesOrigin.transform;
        myUtensil.name = Utensils[n].name + counter[n].ToString();
        myUtensil.transform.localPosition = myUtensil.transform.position;
        myUtensil.transform.localRotation = myUtensil.transform.rotation;
        counter[n]++;
    }

    public void SpawnUtensilWithTransform(int n, Vector3 pos, Quaternion rot)
    {
        GameObject myUtensil = Instantiate(Utensils[n]) as GameObject;
        myUtensil.transform.parent = StonesOrigin.transform;
        myUtensil.name = Utensils[n].name + counter[n].ToString();
        myUtensil.transform.localPosition = pos;
        myUtensil.transform.localRotation = rot;
        counter[n]++;
    }
    
    public void SpawnAllUtensil()
    {
        for (int i = 0; i < 8; i++)
        {
            SpawnUtensil(i);
        }
    }
    // ------------spawn functions------------


    // ------------utensil group functions------------
    public void ClearAllUtensil()
    {
        foreach (GameObject gObject in currentCollisions)
        {
            gObject.GetComponent<DifferenceCalculator>().DestroyObjectInstance();
        }
        currentCollisions.Clear();
    }
    
    public void SaveObjectTransforms(string setNum)
    {
        string path = Path.Combine(Application.persistentDataPath, setNum + "_SavedObjectData.txt");
        using (TextWriter writer = File.CreateText(path))
        {
            foreach (GameObject gObject in currentCollisions)
            {
                writer.WriteLineAsync(gObject.tag + ";" + gObject.transform.localPosition.ToString("f4") + ";" + gObject.transform.localRotation.ToString("f4"));
            }
        }
        Debug.Log(path);
    }

    public void LoadSavedObjectTransforms(string setNum)
    {
        string path = Path.Combine(Application.persistentDataPath, setNum + "_SavedObjectData.txt");
        using (TextReader tr = File.OpenText(path))
        {
            string[] dataArray = tr.ReadToEnd().Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string data in dataArray)
            {
                string[] objInfo = data.Split(';');
                
                if (ConvertUtensilNameToInt(objInfo[0]) == -1) continue;
                SpawnUtensilWithTransform(ConvertUtensilNameToInt(objInfo[0]), ConvertStringPos(objInfo[1]), ConvertStringRot(objInfo[2]));

            }
        }

    }
    // ------------utensil group functions------------

    
    // ------------get an array/list of GameObjects that is currently touching the table------------
    public GameObject[] GetCollidingGameObjectsArray()
    {
        GameObject[] arrayCollisions = currentCollisions.ToArray();
        return arrayCollisions;
    }
    public List<GameObject> GetCollidingGameObjectsList()
    {
        return currentCollisions;
    }


    // ------------private functions for ObjectSpawner use------------
    private void OnCollisionEnter(Collision col)
    {
        currentCollisions.Add(col.gameObject);
    }

    private void OnCollisionExit(Collision col)
    {
        currentCollisions.Remove(col.gameObject);
    }

    private int ConvertUtensilNameToInt(string name)
    {
        switch (name)
        {
            case "Spoon":
                return 0;
            case "Fork":
                return 1;
            case "Cup":
                return 2;
            case "Dish":
                return 3;
            case "Knife":
                return 4;
            case "Minispoon":
                return 5;
            case "Bottle":
                return 6;
            case "Glass":
                return 7;
            default:
                return -1;

        }
    }

    private Vector3 ConvertStringPos(string str)
    {
        str = str.Trim('(', ')', ' ');
        string[] pos = str.Split(',');
        return new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
    }

    private Quaternion ConvertStringRot(string str)
    {
        str = str.Trim('(', ')', ' ');
        string[] rot = str.Split(',');
        return new Quaternion(float.Parse(rot[0]), float.Parse(rot[1]), float.Parse(rot[2]), float.Parse(rot[3]));
    }
    // ------------private functions for ObjectSpawner use------------
}
