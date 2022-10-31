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

  // Start is called before the first frame update
  void Start(){
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
  int targetIndex = 0;

  // Update is called once per frame
  void Update(){
    // if object is back to initial position, don't update
    if(targetIndex < 0){
      return;
    }

    // check if object is at the end node
    if(targetIndex == ConnectionArray.Count - 1){
      reverse = true;
    }
    
    var targetPosition = ConnectionArray[targetIndex].GetFromNode().transform.position;
    targetPosition.y = 0;
    
    float distanceToTarget = CalculateDistance(targetPosition);
   
    // move towards current node when distance is greater than 0.01
    if( distanceToTarget > 0.001f){
      MoveObject(targetPosition);
      RotateObject(targetPosition);
    } else{
      if(reverse == false){
          targetIndex++;
        } else {
          targetIndex--;
        }
      }
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
      float speed = 2f;
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

