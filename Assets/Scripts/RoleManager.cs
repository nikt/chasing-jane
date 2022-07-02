using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Role
{
    Observer,
    Brooklyn,
    Hazelle,
    Jane,
    Zarathok
}

public class RoleManager : MonoBehaviour
{
    [SerializeField] TMP_Dropdown roleInput;

    void Awake()
    {
        string[] roleNames = Enum.GetNames(typeof(Role));
        List<string> names = new List<string>(roleNames);
        roleInput.AddOptions(names);
    }

    public static string GetNameForRole(Role role)
    {
        return Enum.GetNames(typeof(Role))[(int)role];
    }
}
