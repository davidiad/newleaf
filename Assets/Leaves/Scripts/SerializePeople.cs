﻿using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class PersonInfo
{
    public int ID { get; set; }
    public string name { get; set; }
    public Role role { get; set; }
}

[Serializable]
public class PersonInfoArray
{
    public PersonInfo[] personInfos;
}

public class SerializePeople : ScriptableObject
{

    // vars are public to allow accessing from LeavesManager
    public String jsonKey { get; set; }
    public List<PersonInfo> personInfoList;
    public List<Person> personObjList;

    public void Init()
    {
        personInfoList = new List<PersonInfo>();
        personObjList = new List<Person>();
        jsonKey = "people";
    }

    // TODO: handle case where a person's name is changed to another person

    public void OnAddToScene()
    {
        PersonInfo info = new PersonInfo();

        // Get the name of the sender

        // put the transform info into model info object
        info.name = "David"; // link to value of text box
        info.role = Role.Sender;
        info.ID = 0;

        // add info to info list
        personInfoList.Add(info);

        //// Instantiate and add to scene
        //GameObject model = ModelFromInfo(info);

        //// add the game object to object list
        //modelObjList.Add(model);
    }

    //private GameObject ModelFromInfo(ModelInfo info)
    //{
    //    Vector3 pos = new Vector3(info.px, info.py, info.pz);
    //    Quaternion rot = new Quaternion(info.qx, info.qy, info.qz, info.qw);
    //    Vector3 localScale = new Vector3(0.05f, 0.05f, 0.05f);
    //    GameObject model = Instantiate(prefabs[info.modelIndex], pos, rot);

    //    return model;
    //}

    // convert array of person info to json
    public JObject ToJSON()
    {
        PersonInfoArray personInfoArray = new PersonInfoArray();
        personInfoArray.personInfos = new PersonInfo[personInfoList.Count];
        for (int i = 0; i < personInfoList.Count; i++)
        {
            personInfoArray.personInfos[i] = personInfoList[i];
        }

        return JObject.FromObject(personInfoArray);
    }

    public void LoadFromJSON(JToken mapMetadata)
    {
        Clear();

        if (mapMetadata is JObject && mapMetadata[jsonKey] is JObject)
        {
            PersonInfoArray personInfoArray = mapMetadata[jsonKey].ToObject<PersonInfoArray>();
            if (personInfoArray.personInfos == null)
            {
                Debug.Log("No People");
                return;
            }

            // populate the object and info Lists
            foreach (var info in personInfoArray.personInfos)
            {
                personInfoList.Add(info);
 //               Person person = PersonFromInfo(info);
 //               personObjList.Add(person);
            }
        }
    }

    public void Clear()
    {
        personInfoList.Clear();
    }

}
