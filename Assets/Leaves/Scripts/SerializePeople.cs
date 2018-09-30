using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class PersonInfo
{
    public int ID { get; set; }
    public string Name { get; set; }
    public Role Role { get; set; }
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

    //public void OnAddToScene()
    //{
    //    ModelInfo info = new ModelInfo();

    //    // get the object transform info to use
    //    Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.3f;
    //    Quaternion rot = Camera.main.transform.rotation;

    //    // put the transform info into model info object
    //    info.px = pos.x;
    //    info.py = pos.y;
    //    info.pz = pos.z;
    //    info.qx = rot.x;
    //    info.qy = rot.y;
    //    info.qz = rot.z;
    //    info.qw = rot.w;
    //    info.modelIndex = 0; // Default to 0 (just one model) for now

    //    // add info to info list
    //    modelInfoList.Add(info);

    //    // Instantiate and add to scene
    //    GameObject model = ModelFromInfo(info);

    //    // add the game object to object list
    //    modelObjList.Add(model);
    //}

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
