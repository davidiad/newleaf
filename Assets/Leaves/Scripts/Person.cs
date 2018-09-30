using System.Collections.Generic;
using UnityEngine;

public enum Role
{
    Sender = 0,
    Receiver = 1
}

public struct Person
{
    public int ID { get; set; }
    public string Name { get; set; }
    public Role role { get; set;  }
}

