using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private MovementController movementController;

    public SpriteRenderer sprite;
    public Animator animator;

    public GameObject startNode;

    public Vector2 startPos;

    public GameManager gameManager;

    public bool isDead = false;
    void Awake()
    {
        /*startPos = new Vector2(0.047f, -0.658f);*/
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        startPos = transform.position;
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        movementController = GetComponent<MovementController>();
        startNode = movementController.currentNode;

    }

    public void SetUp()
    {
        isDead = false;
        animator.SetBool("death", false);
        animator.SetBool("moving", false);

        movementController.currentNode = startNode;
        movementController.direction = "left";
        movementController.lastMovingDirrection = "left";
        sprite.flipX = false;
        transform.position = startPos;
        animator.speed = 1;
    }

    public void Stop()
    {
        animator.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            if (!isDead)
            {
                animator.speed = 0;
            }
            return;
        }
        animator.speed = 1;
        animator.SetBool("moving", true);
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movementController.SetDirection("left");
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movementController.SetDirection("right");
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            movementController.SetDirection("down");
        }
        else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            movementController.SetDirection("up");
        }

        bool flipX = false;
        bool flipY = false;
        if (movementController.lastMovingDirrection == "left")
        {
            animator.SetInteger("direction", 0);
        }
        else if(movementController.lastMovingDirrection == "right")
        {
            animator.SetInteger("direction", 0);
            flipX = true;
        }
        else if (movementController.lastMovingDirrection == "up")
        {
            animator.SetInteger("direction", 1);
        }
        else if (movementController.lastMovingDirrection == "down")
        {
            flipY = true;
            animator.SetInteger("direction", 1);
        }

        sprite.flipX = flipX;
        sprite.flipY = flipY;
    }

    public void Death()
    {
        isDead = true;
        animator.SetBool("moving", false);
        animator.speed = 1;
        animator.SetBool("death", true);
    }
}
