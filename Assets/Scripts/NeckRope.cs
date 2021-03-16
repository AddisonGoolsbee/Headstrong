using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeckRope : MonoBehaviour
{
    public GameObject[] targets; // the objects to draw the line between
                                 // Use this for initialization
    private LineRenderer l;
    void Start()
    {
        l = this.GetComponent<LineRenderer>();
        Vector2 p1 = targets[0].transform.position;
        l.SetPosition(0, p1);
    }
    
    // Update is called once per frame
    void Update()
    {
        bool headAttached = GameObject.Find("Body").GetComponent<Player>().headAttached;
        bool headAttaching = GameObject.Find("Body").GetComponent<Player>().headAttaching;
        if (!(headAttached && !headAttaching))
        {
            Vector2 p1 = new Vector2(targets[0].transform.localPosition.x - 0.7f, targets[0].transform.localPosition.y + 2.615f);
            Vector2 p2 = new Vector2(targets[1].transform.localPosition.x - 0.7f, targets[1].transform.localPosition.y + 1.8f);
            Vector2 lineVector = p2 - p1;
            l.SetPosition(0, p1);
            l.SetPosition(1, p2);
        }
        else
        {
            l.SetPosition(0, Vector3.down*100);
            l.SetPosition(1, Vector3.down*100);
        }
        
    }
}
