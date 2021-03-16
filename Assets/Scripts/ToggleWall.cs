using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleWall : MonoBehaviour
{
    public GameObject wall;

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            wall.SetActive(!wall.activeSelf);
        }
    }
}
