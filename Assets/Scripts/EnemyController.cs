using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    public enum GhostNodeStateEnum {
        
        respawning,
        leftNode,
        rightNode,
        startNode,
        movingInNodes,
        centerNode
    }

    public GhostNodeStateEnum ghostNodeState;
    public GhostNodeStateEnum startGhostNodeState;
    public GhostNodeStateEnum respawnState;
    public enum GhostType
    {
        red, blue, pink, orange
    }

    public GhostType ghostType;
    

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public MovementController movementController;

    public GameObject startingNode;

    public bool readytoLeaveHome = false;

    public GameManager gameManager;

    public bool testRespawn = false;

    public bool isFrightened = false;

    public GameObject[] scatterNodes;
    public int scatterNodeIndex;

    public bool leftHomeBefore = false;

    public bool isVisible = true;

    public SpriteRenderer ghostSprite;
    public SpriteRenderer eyesSprite;

    public Animator animator;

    public Color color;


    void Awake()
    {
        animator = GetComponent<Animator>();
        ghostSprite = GetComponent<SpriteRenderer>();
       
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();
        if (ghostType == GhostType.red)
        {
            startGhostNodeState = GhostNodeStateEnum.startNode;
            respawnState = GhostNodeStateEnum.centerNode;
            startingNode = ghostNodeStart;
          
        }
        else if (ghostType == GhostType.pink)
        {
            startGhostNodeState = GhostNodeStateEnum.centerNode;
            respawnState = GhostNodeStateEnum.centerNode;
            startingNode = ghostNodeCenter;
        }
        else if (ghostType == GhostType.blue)
        {
            startGhostNodeState = GhostNodeStateEnum.leftNode;
            respawnState = GhostNodeStateEnum.leftNode;
            startingNode = ghostNodeLeft;
        }
        else if (ghostType == GhostType.orange)
        {
            startGhostNodeState = GhostNodeStateEnum.rightNode;
            respawnState = GhostNodeStateEnum.rightNode;
            startingNode = ghostNodeRight;
        }
        
       /* movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;*/
    }

    public void SetUp()
    {
        
        animator.SetBool("moving", false);
        ghostNodeState = startGhostNodeState;
        readytoLeaveHome = false;
        //Reset our ghosts back to their position
        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;

        movementController.direction = "";
        movementController.lastMovingDirrection = "";

        //Set their scatter node index back to 0
        scatterNodeIndex = 0;

        //Set isFrightened
        isFrightened = false;
        leftHomeBefore = false;
        //Set ready to leave to be false if they are blue or pink
        if (ghostType == GhostType.red)
        {
            readytoLeaveHome = true;
            leftHomeBefore = true;
        }
        else if (ghostType == GhostType.pink)
        {
            readytoLeaveHome = true;
        }
        SetVisible(true);
    }
    void Update()
    {
        if (ghostNodeState != GhostNodeStateEnum.movingInNodes || !gameManager.isPowerPelletRunning)
        {
            isFrightened = false;
        }

        //Show our sprite
        if (isVisible)
        {
            if (ghostNodeState!= GhostNodeStateEnum.respawning)
            {
                ghostSprite.enabled = true;
            }
            else
            {
                ghostSprite.enabled = false;
            }
             eyesSprite.enabled = true;
        }
        //Hide our sprite
        else{
            ghostSprite.enabled = false;
            eyesSprite.enabled = false;
        }
        if (isFrightened)
        {
            animator.SetBool("frightened", true);
            eyesSprite.enabled = false;
            ghostSprite.color = new Color(255, 255, 255, 255);
        }
        else
        {
            animator.SetBool("frightened", false);
            animator.SetBool("frightenedBlinking", false);
            ghostSprite.color = color;
        }

        if (!gameManager.gameIsRunning)
        {
            return;
        }

        if (gameManager.powerPelletTimer - gameManager.currentPowerPelletTime <= 3)
        {
            animator.SetBool("frightenedBlinking", true);
        }
        else
        {
            animator.SetBool("frightenedBlinking", false);
        }

        animator.SetBool("moving", true);

        if (testRespawn == true)
        {
            readytoLeaveHome = false;
            ghostNodeState = GhostNodeStateEnum.respawning;
            testRespawn = false;
        }
        if (movementController.currentNode.GetComponent<NodeController>().isSideNode)
        {
            movementController.SetSpeed(1);
        }
        else
        {
            if (isFrightened)
            {
                movementController.SetSpeed(1.8f);
            }
            else if (ghostNodeState == GhostNodeStateEnum.respawning)
            {
                movementController.SetSpeed(5f);
            }
            else
            {
                movementController.SetSpeed(1.5f);
            }
        }
    }

    public void SetFrightened(bool newIsFrightened)
    {
        isFrightened = newIsFrightened;
    }

    public void ReachedCenterOfNode(NodeController nodeController)
    {
        
        if (ghostNodeState == GhostNodeStateEnum.movingInNodes)
        {
            leftHomeBefore = true;
            //Scatter mode
            if (gameManager.currentGhostMode == GameManager.GhostMode.scatter)
            {
                DetermineGhostScatterModeDirection();
            }
            //Frightend mode
            else if (isFrightened)
            {
                string direction = GetRandomDirection();
                movementController.SetDirection(direction);
            }
            //Chase mode
            else
            {
                //Determine next gameNode to go to
                if (ghostType == GhostType.red)
                {
                    DetermineRedGhostDirection();
                }
                else if (ghostType == GhostType.pink)
                {
                    DeterminePinkGhostDirection();
                }
                else if (ghostType == GhostType.blue)
                {
                    DetermineBlueGhostDirection();
                }
                else if (ghostType == GhostType.orange)
                {
                    DetermineOrangeGhostDirection();
                }
            }

        }
        else if (ghostNodeState == GhostNodeStateEnum.respawning)
        {
            string direction = "";

            //We have reached our startnode, move to centernode
            if (transform.position.x == ghostNodeStart.transform.position.x && transform.position.y== ghostNodeStart.transform.position.y)
            {
                direction = "down";
            }
            //We have reched our centernode, either finish respawn, or move to left/right node
            else if (transform.position.x == ghostNodeCenter.transform.position.x && transform.position.y == ghostNodeCenter.transform.position.y)
            {
                if (respawnState == GhostNodeStateEnum.centerNode)
                {
                    ghostNodeState = respawnState;
                }
                else if (respawnState == GhostNodeStateEnum.leftNode)
                {
                    direction = "left";
                }
                else if (respawnState == GhostNodeStateEnum.rightNode)
                {
                    direction = "right";
                }
            }
            // If our respawn is either the left or right state, and we got to that node, leave home again
            else if ((transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y)
                || (transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y)
                )
            {
                ghostNodeState = respawnState;
            }
            // We are in the gameboard still, locate our startnode
            else
            {
                //Determine quickest dirction to home
                direction = GetClosetDirection(ghostNodeStart.transform.position);
            }
            
            movementController.SetDirection(direction);
        }

        else
        {
            // if we're able to leave home
            if (readytoLeaveHome)
            {
                // Even we're left or right, we go to center 
                if (ghostNodeState == GhostNodeStateEnum.leftNode)
                {
                    ghostNodeState = GhostNodeStateEnum.centerNode;
                    movementController.SetDirection("right");
                }
                else if (ghostNodeState == GhostNodeStateEnum.rightNode)
                {
                    ghostNodeState = GhostNodeStateEnum.centerNode;
                    movementController.SetDirection("left");
                }
                //if in center, go to startNode
                else if (ghostNodeState == GhostNodeStateEnum.centerNode)
                {
                    ghostNodeState = GhostNodeStateEnum.startNode;
                    movementController.SetDirection("up");
                }
                // if in startNode, determine next gameNode to go
                else if (ghostNodeState == GhostNodeStateEnum.startNode)
                {
                    ghostNodeState = GhostNodeStateEnum.movingInNodes;
                    movementController.SetDirection("left");
                }
            }
        }
    }

    private string GetRandomDirection()
    {
        List<string> possibleDirections = new List<string>();
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();
        if (nodeController.canMoveDown && movementController.lastMovingDirrection != "up" )
        {
            possibleDirections.Add("down");
        }
        if (nodeController.canMoveUp && movementController.lastMovingDirrection != "down")
        {
            possibleDirections.Add("up");
        }
        if (nodeController.canMoveRight && movementController.lastMovingDirrection != "left")
        {
            possibleDirections.Add("right");
        }
        if (nodeController.canMoveLeft && movementController.lastMovingDirrection != "right")
        {
            possibleDirections.Add("left");
        }
        if (!nodeController.canMoveDown && !nodeController.canMoveRight && !nodeController.canMoveUp && nodeController.isWarpRightNode && movementController.canWarp)
        {
     
            transform.position = movementController.currentNode.GetComponent<NodeController>().transform.position;
            possibleDirections.Add("right");
        }
        if (!nodeController.canMoveDown && !nodeController.canMoveLeft && !nodeController.canMoveUp && nodeController.isWarpLeftNode && movementController.canWarp)
        {
            transform.position = movementController.currentNode.GetComponent<NodeController>().transform.position;
            possibleDirections.Add("left");
        }

        string direction = "";
        int randomDirectionIndex = Random.Range(0, possibleDirections.Count - 1);
        direction = possibleDirections[(randomDirectionIndex)];
        return direction;
    }

    private void DetermineGhostScatterModeDirection()
    {
        //If we reach scatter node, add one to our scatter node index
        if (transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;
            if (scatterNodeIndex == scatterNodes.Length - 1)
            {
                scatterNodeIndex = 0;
            }
        }
        string direction = GetClosetDirection(scatterNodes[scatterNodeIndex].transform.position);
        movementController.SetDirection(direction);
    }

    private void DetermineRedGhostDirection()
    {
        string direction = GetClosetDirection(gameManager.pacman.transform.position);
        movementController.SetDirection(direction);
    }
    private void DeterminePinkGhostDirection()
    {
        string pacmanDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirrection;
        float distanceBetweenNode = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmanDirection == "left")
        {
            target.x  -= distanceBetweenNode * 2;
        }
        else if (pacmanDirection == "right")
        {
            target.x += distanceBetweenNode * 2;
        }
        else if (pacmanDirection == "up")
        {
            target.y += distanceBetweenNode * 2;
        }
        else if (pacmanDirection == "down")
        {
            target.y -= distanceBetweenNode * 2;
        }

        string direction = GetClosetDirection(target);
        movementController.SetDirection(direction);
    }
    private void DetermineBlueGhostDirection()
    {
        string pacmanDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirrection;
        float distanceBetweenNode = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmanDirection == "left")
        {
            target.x -= distanceBetweenNode * 2;
        }
        else if (pacmanDirection == "right")
        {
            target.x += distanceBetweenNode * 2;
        }
        else if (pacmanDirection == "up")
        {
            target.y += distanceBetweenNode * 2;
        }
        else if (pacmanDirection == "down")
        {
            target.y -= distanceBetweenNode * 2;
        }

        GameObject redGhost = gameManager.redGhost;
        float xDistance = target.x - redGhost.transform.position.x;
        float yDistance = target.y - redGhost.transform.position.y;

        Vector2 blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);
        string direction = GetClosetDirection(blueTarget);
        movementController.SetDirection(direction);
    }
    private void DetermineOrangeGhostDirection()
    {
        float distance = Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        float distanceBetweenNode = 0.35f;
        if (distance < 0)
        {
            distance *= -1;
        }

        //If we're 8 nodes of pacman, chase him using Red's logic 
        if (distance <= distanceBetweenNode * 8)
        {
            DetermineRedGhostDirection();
        }
        //Otherwise use scatter mode logic
        else
        {
            //Scatter mode
            DetermineGhostScatterModeDirection(); 
        }
    }

    string  GetClosetDirection(Vector2 target)
    {
        float shortestDistance = 0;
        string lastMovingDirection = movementController.lastMovingDirrection;
        string newDirection = "";
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        //If we can move up, and not reversing
        if (nodeController.canMoveUp && lastMovingDirection != "down")
        {
            //Get the node above us
            GameObject nodeUp = nodeController.nodeUp;
            //Get the distance between top node, and pacman
            float distance = Vector2.Distance(nodeUp.transform.position, target);
            if (distance < shortestDistance || shortestDistance ==0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        //If we can move down, and not reversing
        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            //Get the node below us
            GameObject nodeDown = nodeController.nodeDown;
            //Get the distance between above node, and pacman
            float distance = Vector2.Distance(nodeDown.transform.position, target);
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }

        //If we can move left, and not reversing
        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            //Get the node left to us
            GameObject nodeLeft = nodeController.nodeLeft;
            //Get the distance between left node, and pacman
            float distance = Vector2.Distance(nodeLeft.transform.position, target);
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }

        //If we can move right, and not reversing
        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            //Get the node right to us
            GameObject nodeRight = nodeController.nodeRight;
            //Get the distance between right node, and pacman
            float distance = Vector2.Distance(nodeRight.transform.position, target);
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }

        return newDirection;
    }
    public void SetVisible(bool newIsVisible)
    {
        isVisible = newIsVisible;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag =="Player" && ghostNodeState != GhostNodeStateEnum.respawning)
        {
            //get eaten
            if (isFrightened)
            {
                gameManager.GhostEaten();
                ghostNodeState = GhostNodeStateEnum.respawning;
            }
            //eat player
            else
            {
                StartCoroutine(gameManager.PlayerEaten());
            }
        }
    }
}
