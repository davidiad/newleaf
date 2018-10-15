using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

[Serializable]
public class PersonInfo
{
    public int ID { get; set; }
    public string name { get; set; }
    public Role role { get; set; }
}

// Note: Person and PersonInfo currently have the same, serializable properties, but keeping both
// for now to keep this class's structure analogous to Model and Paintstroke

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
    public string currentName;

    public void Init()
    {
        personInfoList = new List<PersonInfo>();
        personObjList = new List<Person>();
        jsonKey = "people";
        UpdateName();
    }

    private void UpdateName()
    {
        string n = GameObject.FindWithTag("name").GetComponent<Text>().text;
        if (n != null)
        {
            currentName = n;
        }
    }

    // TODO: handle case where the current person's name is changed to another person

    public void OnNameChange() // TODO: rename to OnNameChange?
    {
        Clear();
        UpdateName();
        PersonInfo info = new PersonInfo();
        info.name = currentName; // link to value of input text box
        info.role = Role.Sender;
        info.ID = 0;

        personInfoList.Add(info);

        Person person = PersonFromInfo(info);

        personObjList.Add(person);
    }

    private Person PersonFromInfo(PersonInfo info)
    {
        Person person = new Person();
        person.name = info.name;
        person.role = info.role;
        person.ID = info.ID;
        return person;
    }

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
                Person person = PersonFromInfo(info);
                personObjList.Add(person);
            }
        }
    }

    public void Clear()
    {
        personInfoList.Clear();
        personObjList.Clear();
    }
}
