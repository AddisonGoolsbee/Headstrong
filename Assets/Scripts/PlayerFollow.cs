using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    private Transform player;
    public float speed = 5.0f; //lerp speed
    public float offsetx = 0f, offsety = 0f; //to uncenter camera in base state
    public float ground = -4f, ceiling, rightWall, leftWall; //location of edge of current camera bounds
    public float wallGive = 3f; //distance the camera goes into floor/wall/ceiling

    private float halfHeight, halfWidth;
    private float targetX, targetY;

    private void Start()
    {
        Camera cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        player = GameObject.Find("Body").transform;
    }

    void Update()
    {
        //calculate bound positions
        float leftWallPos = leftWall + halfWidth - wallGive;
        float rightWallPos = rightWall - halfWidth + wallGive;
        float floorPos = ground + halfHeight - wallGive;
        float ceilingPos = ceiling - halfHeight + wallGive;

        //x target
        if(player.position.x < leftWallPos)
            targetX = leftWallPos;
        else if (player.position.x > rightWallPos)
            targetX = rightWallPos;
        else
            targetX = player.position.x + offsetx;

        //y target
        if (player.position.y < floorPos)
            targetY = floorPos;
        else if (player.position.y > ceilingPos)
            targetY = ceilingPos;
        else
            targetY = player.position.y + offsety;

        float interpolation = speed * Time.deltaTime;
        Vector3 position = transform.position;

        position.x = Mathf.Lerp(position.x, targetX, interpolation);
        position.y = Mathf.Lerp(position.y, targetY, interpolation);

        transform.position = position;
    }
}
