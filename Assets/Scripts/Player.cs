using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    
    public GameObject head;
    public GameObject playerPrefab;
    public float headGravity = 3.5f, headlessGravity = 2.0f, gravityOfHead = 7f, snapDistance = 1f, groundCheckHeight = 2f;
    public float floorFriction = 0.95f, headlessSpeed = 10.3f, headSpeed = 5.0f, movementSpeed, attachSpeed = 40.0f, reverseAttachSpeed = 20f;
    public float headJumpHeight = 9.0f, headlessJumpHeight = 11.5f, groundRadius = 0.2f;
    public float boostMultiplierx = 2f, boostMultipliery = 1.6f, noBoostDistance = 1f;
    public float attachedHeadMass = 0.1f, headMass = 1.5f, detatchPush = 5f, reverseBoostTolerancy = 1f;

    public Transform groundCheck;
    public LayerMask whatIsGround;
    public Rigidbody2D rb, headrb;
    public bool bodyMoving, headMoving; //Check if head/body should be stuck in door
    public bool headAttached = true, headAttaching = false, reverseAttaching = false, facingRight = true;
    public Vector3 spawnPos;

    bool grounded = true;
    BoxCollider2D bc, headbc;
    DistanceJoint2D headJoint;
    FixedJoint2D fj;
    float initialHeadPosX, initialHeadPosY, jumpHeight;
    Vector2 movement;
    Vector3 prevPosition;
    bool justBoosted;
    


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        headrb = head.GetComponent<Rigidbody2D>();
        headJoint = head.GetComponent<DistanceJoint2D>();
        fj = head.GetComponent<FixedJoint2D>();
        bc = GetComponent<BoxCollider2D>();
        headbc = head.GetComponent<BoxCollider2D>();
        prevPosition = Vector3.up*1000;

        jumpHeight = headJumpHeight;
        movementSpeed = headSpeed;
        rb.gravityScale = headGravity;
    }

    void Update()
    {
        if (justBoosted)
        {
            bodyMoving = true;
            headMoving = false;
            justBoosted = false;
        }
        else
            bodyMoving = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Space);

        float direction = Input.GetAxis("Horizontal");
        if (direction < 0)
            facingRight = false;
        else if(direction > 0)
            facingRight = true;

        movement = Vector2.right * direction * movementSpeed;

        //Jump
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
            bodyMoving = true;
        }
            

        //Attach/detach head
        if (Input.GetKeyDown(KeyCode.F))
        {
            HeadInteract();
        }
    }


    private void FixedUpdate()
    {
        //Ground check
        grounded = IsGrounded();

        //Using translate so floor friction works
        transform.Translate(movement);

        //head attach behavior
        if (headAttaching)
            Attach();

        //floor friction while boosting
        if (grounded && rb.velocity.x != 0 && !reverseAttaching) 
            rb.velocity = new Vector2(rb.velocity.x * floorFriction, rb.velocity.y);
    }


    //Attach behavior
    void Attach()
    {
        //End of attach + boost
        if (Vector3.Distance(head.transform.position, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z)) < snapDistance)
        {
            headrb.mass = attachedHeadMass;

            //player boost
            Vector2 boostVector = GetBoost();

            //Y velocity finesse
            if (boostVector.y * rb.velocity.y > 0)
                if (Mathf.Abs(boostVector.y) > Mathf.Abs(rb.velocity.y))
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                else
                    boostVector.y *= 0.5f;
            else
            {
                if (Mathf.Abs(boostVector.y) < Mathf.Abs(rb.velocity.y))
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 3);
                } else
                    rb.velocity = new Vector2(rb.velocity.x, 0);
            }
            
            rb.AddForce(boostVector, ForceMode2D.Impulse);

            headAttaching = false;
            reverseAttaching = false;
            movementSpeed = headSpeed;
            rb.gravityScale = headGravity;
            jumpHeight = headJumpHeight;
            headrb.gravityScale = gravityOfHead;
            Physics2D.IgnoreCollision(headbc, bc, false);
            fj.enabled = true;
            prevPosition = Vector3.up * 1000;
            justBoosted = true;
        }

        //head-body attaching
        Vector2 headToBody = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z) - head.transform.position;
        headToBody.Normalize();
        headrb.velocity = headToBody * attachSpeed;

        //body-head attaching if head gets stuck
        if(Vector2.Distance(head.transform.position, prevPosition) < reverseBoostTolerancy) //if head stuck, move player as well
        {
            reverseAttaching = true;
            rb.gravityScale = 0f;
            Vector2 bodyToHead = head.transform.position - new Vector3(transform.position.x, transform.position.y + 0.75f, transform.position.z);
            bodyToHead.Normalize();
            rb.velocity = bodyToHead * reverseAttachSpeed;
        }
        prevPosition = head.transform.position;
    }


    //Detach/reattach behavior
    private void HeadInteract() 
    {
        if (headAttached) //detach head
        {
            headAttached = false;

            if (headAttaching) //head is dropped mid-attach
            {
                headrb.velocity = Vector2.zero;
                Physics2D.IgnoreCollision(headbc, bc, false);

                //stop body momentum if cut short while reverse attaching
                if (reverseAttaching)
                {
                    float yvel = rb.velocity.y;
                    reverseAttaching = false;
                    if (yvel > 0f)
                        yvel = 0f;
                    rb.velocity = new Vector2(0f, yvel);
                    rb.gravityScale = headlessGravity;
                }

                headAttaching = false;
            }
            else //head is droped normally
            {
                //detach in front of player
                Vector2 detatchVector = new Vector2(detatchPush, 0);
                if (!facingRight)
                    detatchVector *= -1;
                headrb.MovePosition(new Vector2(headrb.position.x, headrb.position.y - 0.2f) + detatchVector);
            }

            head.layer = 0;
            headrb.mass = headMass;
            headJoint.enabled = true;
            fj.enabled = false;
            jumpHeight = headlessJumpHeight;
            movementSpeed = headlessSpeed;
            rb.gravityScale = headlessGravity;
            headrb.gravityScale = gravityOfHead;

        } else //reattach head
        {
            headAttached = true;
            headMoving = true;

            head.layer = LayerMask.NameToLayer("Player");
            
            headJoint.enabled = false;
            headAttaching = true;
            initialHeadPosX = head.transform.position.x;
            initialHeadPosY = head.transform.position.y;
            headrb.gravityScale = 0.0f;
            Physics2D.IgnoreCollision(headbc, bc);
        }
    }


    //Calculate boost vector on attach end
    private Vector2 GetBoost()
    {
        
        Vector2 boostDirection = new Vector2(transform.position.x - head.transform.position.x, head.transform.position.y - transform.position.y);
        boostDirection.Normalize(); //Get unit vector for direction, this is faster than doing it in one line

        //No boost (small nudge) for small attach distance
        if(Vector2.Distance(new Vector2(initialHeadPosX,initialHeadPosY), new Vector2(transform.position.x, transform.position.y+1f)) < noBoostDistance)
            return Vector2.zero;

        float boostX = boostMultiplierx * Mathf.Abs(initialHeadPosX - transform.position.x);
        float boostY = boostMultipliery * Mathf.Abs(initialHeadPosY - (transform.position.y+1f));

        Vector2 boostVector = new Vector2(boostX * boostDirection.x, boostY * boostDirection.y);
        return boostVector;
    }

    private bool IsGrounded()
    {
        //smaller than player width
        RaycastHit2D closeGround = Physics2D.BoxCast(bc.bounds.center, new Vector3(bc.bounds.size.x-0.3f, bc.bounds.size.y, 0), 0f, Vector2.down, groundCheckHeight, whatIsGround);

        //wideGround is player width, wall check is also player width, but higher. If wallCheck is false but wideGround is true, you're on the edge
        RaycastHit2D wideGround = Physics2D.BoxCast(bc.bounds.center, new Vector3(bc.bounds.size.x, bc.bounds.size.y, 0), 0f, Vector2.down, groundCheckHeight, whatIsGround);
        RaycastHit2D wallCheck = Physics2D.BoxCast(bc.bounds.center, new Vector3(bc.bounds.size.x, bc.bounds.size.y, 0), 0f, Vector2.up, groundCheckHeight, whatIsGround);

        return closeGround.collider != null || (wideGround.collider != null && wallCheck.collider == null);
    }

    public void Die()
    {
        //Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        //Destroy(gameObject.transform.parent.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        if(collision.tag == "Spike")
        {
            Die();
        }
    }
}