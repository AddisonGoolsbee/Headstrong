using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public float headSnapDistance = 0.4f, bodySnapDistance = 0.3f;
    public float forcedStuckTime = 0.5f, immuneTime = 0.25f, readyTime = 0.3f;
    public string toLoad;

    Transform headMold, bodyMold;
    Transform body, head;
    public bool headStuck, bodyStuck;
    float headStuckTime = 0f, bodyStuckTime = 0f; //timer of how long head/body has been stuck
    float headResetTime = 0f, bodyResetTime = 0f; //timer of how long stuck immunity lasts for
    public bool headReady = false, bodyReady = false; //If head/body has been stuck for long enough
    FixedJoint2D headJoint, bodyJoint;
    Player player;
    

    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        Transform bodyObject = playerObject.transform.Find("Body");
        player = bodyObject.GetComponent<Player>();

        headMold = gameObject.transform.Find("Head Mold");
        bodyMold = gameObject.transform.Find("Body Mold");
        body = playerObject.transform.Find("Body");
        head = playerObject.transform.Find("Head");
        headJoint = headMold.GetComponent<FixedJoint2D>();
        bodyJoint = bodyMold.GetComponent<FixedJoint2D>();

       
    }

    void LateUpdate() //so moving is checked in the same frame, before determining unstuckness
    {
        CheckHead();


    }

    void CheckHead()
    {

        if (headStuck)
        {
            if (headStuckTime == 0f)
                headStuckTime = Time.time;
            else if (Time.time - headStuckTime > forcedStuckTime)
            {
                if (player.headMoving || Vector3.Distance(head.position, headMold.position) > 1.15f)
                {
                    headStuck = false;
                    headResetTime = Time.time;
                    headStuckTime = 0f;
                    headReady = false;
                }
                else if (Time.time - headStuckTime > readyTime)
                    headReady = true;
            }
        }
        else
        {
            headJoint.enabled = false;
            bool immunity = headResetTime == 0f || (Time.time - headResetTime > immuneTime);
            if (immunity && Vector2.Distance(headMold.position, head.position) < headSnapDistance)
            {
                headStuck = true;
                headJoint.enabled = true;
                
            }
        }



        if (bodyStuck)
        {
            if (bodyStuckTime == 0f)
                bodyStuckTime = Time.time;
            else if (Time.time - bodyStuckTime > forcedStuckTime)
            {
                if (player.bodyMoving)
                {
                    bodyStuck = false;
                    bodyResetTime = Time.time;
                    bodyStuckTime = 0f;
                    bodyReady = false;
                }
                else if (Time.time - bodyStuckTime > readyTime)
                    bodyReady = true;
            }   
        }
        else
        {
            bodyJoint.enabled = false;
            bool immunity = bodyResetTime == 0f || (Time.time - bodyResetTime > immuneTime);
            if (immunity && Vector2.Distance(bodyMold.position, body.position) < bodySnapDistance)
            {
                bodyStuck = true;
                bodyJoint.enabled = true;
            }
        }


        if (bodyReady && headReady)
            SceneManager.LoadScene(toLoad);
        
    }

}
