using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathfindingTester : MonoBehaviour
{
// The A* manager.
  private AStarManager AStarManager = new AStarManager();
  // Array of possible waypoints.
  List<GameObject> Waypoints = new List<GameObject>();
  // Array of waypoint map connections. Represents a path.
  List<Connection> ConnectionArray = new List<Connection>();
  // The start and end target point.
  public GameObject start;
  public GameObject end;
  // Debug line offset.
  Vector3 OffSet = new Vector3(0, 0.3f, 0);

  public float speed = 1.0f;
  private Transform target;
  private Animator anim;

  // Start is called before the first frame update
  void Start(){
      GameObject gameObject = GameObject.FindGameObjectWithTag("Character");
      anim = gameObject.GetComponent<Animator>();  
      // anim.SetBool("Run", true);
      if (start == null || end == null){
      Debug.Log("No start or end waypoints.");
      return;
    }
    
    // Find all the waypoints in the level.
    GameObject[] GameObjectsWithWaypointTag;
    GameObjectsWithWaypointTag = GameObject.FindGameObjectsWithTag("Waypoint");
    foreach (GameObject waypoint in GameObjectsWithWaypointTag){
      WaypointCON tmpWaypointCon = waypoint.GetComponent<WaypointCON>();
      if (tmpWaypointCon){
        Waypoints.Add(waypoint);
      }
    }
    // Go through the waypoints and create connections.
    foreach (GameObject waypoint in Waypoints){
      WaypointCON tmpWaypointCon = waypoint.GetComponent<WaypointCON>();
      // Loop through a waypoints connections.
      foreach (GameObject WaypointConNode in tmpWaypointCon.Connections){
        Connection aConnection = new Connection();
        aConnection.SetFromNode(waypoint);
        aConnection.SetToNode(WaypointConNode);
        AStarManager.AddConnection(aConnection);
      }
    }
    // Run A Star...
    ConnectionArray = AStarManager.PathfindAStar(start, end);
  }
  // Draws debug objects in the editor and during editor play (if option set).
  void OnDrawGizmos(){
    // Draw path.
    foreach (Connection aConnection in ConnectionArray){
      Gizmos.color = Color.white;
      Gizmos.DrawLine((aConnection.GetFromNode().transform.position + OffSet),
      (aConnection.GetToNode().transform.position + OffSet));
    }
  }

  bool reverse = false;
  private int targetIndex = 0;
  private float idle;
  private bool isLastNode = false;

  // Update is called once per frame
  void Update(){
    var targetPosition = ConnectionArray[targetIndex].GetFromNode().transform.position;
    targetPosition.y = 0;
    float distanceToTarget = CalculateDistance(targetPosition);

    
    // if object is back to initial position, don't update
    if(targetIndex == 0 & distanceToTarget < 10f & reverse == true){
      anim.SetBool("Idle", true);
      return;
    }

    // check if next node is the end node
    if(targetIndex == ConnectionArray.Count - 1){
      reverse = true;
      isLastNode = true;
    }
    
    
   
    // move towards current node when distance is greater than 0.0001
    if( distanceToTarget > 0.0001f){
        // delay for idle seconds
        if(idle > 0){
          idle -= Time.deltaTime;
        }else{
          anim.SetBool("Idle", false);
          MoveObject(targetPosition);
          RotateObject(targetPosition);
        }
    } else{
      // create a delay if last node is reached
      if(reverse == true){
          targetIndex--;
        } else {
          targetIndex++;
        }

        if(isLastNode){
          // start idle timer for 5 seconds
          idle = 5;
          anim.SetBool("Idle", true);
          isLastNode = false;
      }
      }

      // if(distanceToTarget < 5f & isLastNode){
      //   Debug.Log(isLastNode);
      //     // start idle timer for 5 seconds
      //     idle = 5;
      //     anim.SetBool("Idle", true);
      //     isLastNode = false;
      // }
    }

    void MoveObject(Vector3 targetPosition){
      var step =  speed * Time.deltaTime;

      Vector3 objectPos = transform.position;
      objectPos.y = 0;
      
      Vector3 newPosition = Vector3.MoveTowards(objectPos, targetPosition, step);
      newPosition.y = Terrain.activeTerrain.SampleHeight(transform.position);
      transform.position = newPosition;
    }

    void RotateObject(Vector3 targetPosition){
      var currentPosition = transform.position;
      float speed = 3f;
      currentPosition.y = 0;
      
      if((targetPosition - currentPosition) != Vector3.zero){
        var targetRotation = Quaternion.LookRotation(targetPosition - currentPosition);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
      }
    }
    
    float CalculateDistance(Vector3 targetPosition){
       var vectorToTarget = transform.position - targetPosition;
       vectorToTarget.y = 0;
       return vectorToTarget.magnitude;
    }
}

