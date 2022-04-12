using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameManager gameManager;

    public GameObject currentNode;
    public float speed = 4f;

    public string direction = "";
    public string lastMovingDirrection = "";

    public bool canWarp;

    public bool isGhost = false;
     
    void Awake()
    {
        
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
      
    }

    
    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }
        NodeController currentNodeController = currentNode.GetComponent<NodeController>();
        transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position, speed * Time.deltaTime);

        bool reverseDirection = false;

        if (
            (direction == "left" && lastMovingDirrection == "right")
            || (direction == "right" && lastMovingDirrection == "left")
            || (direction == "up" && lastMovingDirrection == "down")
            || (direction == "down" && lastMovingDirrection == "up")
            )
        {
            reverseDirection = true;
        }

        // Figure out if we're at the center of our current node
        if ((transform.position.x == currentNode.transform.position.x && transform.position.y == currentNode.transform.position.y) || reverseDirection)
        {

            if (isGhost)
            {
                GetComponent<EnemyController>().ReachedCenterOfNode(currentNodeController);
            }
            //If we reached the center of the left warp, warp to the right warp
            if (currentNodeController.isWarpLeftNode && canWarp)
            {
                currentNode = gameManager.rightWarpNode;
                direction = "left";
                lastMovingDirrection = "left";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            //If we reached the center of the right warp, warp to the left warp
            else if (currentNodeController.isWarpRightNode && canWarp)
            {
                currentNode = gameManager.leftWarpNode;
                direction = "right";
                lastMovingDirrection = "right";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            // otherwise, find the next node
            else
            {
                // if we are not a ghost that is respawning, and we are not startnode, and we trying to move down, stop
                if (currentNodeController.isGhostStartingNode && direction == "down" 
                    && (!isGhost || GetComponent<EnemyController>().ghostNodeState != EnemyController.GhostNodeStateEnum.respawning))
                {
                    direction = lastMovingDirrection;
                }

                //Get the next node from our node controller using our current direction
                GameObject newNode = currentNodeController.GetNodeFromDirection(direction);
                // if we can go to the desired direction
                if (newNode != null)
                {
                    currentNode = newNode;
                    lastMovingDirrection = direction;
                }
                //We cant go to the desired direction, keep going in the last moving direction
                else
                {
                    direction = lastMovingDirrection;
                    newNode = currentNodeController.GetNodeFromDirection(direction);
                    if (newNode != null)
                    {
                        currentNode = newNode;
                    }
                }
            }

        }
        //Not in center of a node
        else
        {
            canWarp = true;
        }

    }

    public void SetSpeed(float newspeed)
    {
        speed = newspeed;
    }

    public void SetDirection(string newDirection)
    {
        direction = newDirection; 
    }
}
