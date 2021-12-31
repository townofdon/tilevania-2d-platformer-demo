using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppIntegrity
{
    // Start is called before the first frame update
    public static void AssertPresent(object value)
    {
        if (value == null) Debug.LogError("FAILED NULL CHECK: " + value.GetType());
    }
}
