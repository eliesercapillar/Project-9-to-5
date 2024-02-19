using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Toolbox;

namespace NPC
{
    public class Script_NPC : MonoBehaviour
    {
        [Header("Managers")]
        private GameManager _gameManager;
        [SerializeField] private Script_NPCAnimationManager _animationManager;

        [Header("NPC Components")]
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _moveSpeed;

        [Header("Pathfinding")]
        [SerializeField] private List<GameObject> _waypoints;   // A list of key waypoints this NPC will go to.
        [SerializeField] private List<Vector3> _pathToWaypoint; // A list of tiles that indicate a path to travel to get to a waypoint.
        [SerializeField] private GameObject _currentWaypoint;   // The current selected target waypoint to travel to.
        
        // State Flags
        private bool _waypointReached = true;
        private bool _isInteracting = false;
        private bool _isMoving = false;
        private bool _isWalkingRight = false;
        private bool _isWalkingUp = false;

        // Getters/Setters
        public List<GameObject> Waypoints { get { return _waypoints; } set { _waypoints = value; }}
        public bool WaypointReached       { get { return _waypointReached; } }
        public bool IsInteracting         { get { return _isInteracting; } set { _isInteracting = value; }}
        public bool IsWalkingRight        { get { return _isWalkingRight; } }
        public bool IsWalkingUp           { get { return _isWalkingUp; } }

        private void Start()
        {
            _gameManager = GameManager._instance;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.name == _currentWaypoint.name)
            {
                Debug.Log("Waypoint is reached.");
                _waypointReached = true;
                //InteractAtWaypoint();
            }
        }

        public IEnumerator StartPatrolling()
        {
            while (true)
            {
                if (_waypointReached)
                {
                    GetNewWaypoint();
                    GetPath();
                }
                yield return TraversePath();
            }
        }

        private void GetNewWaypoint()
        {
            GameObject newWaypoint = _waypoints.RandomElement();
            while (newWaypoint == _currentWaypoint)
            {
                newWaypoint = _waypoints.RandomElement();
            }
            _currentWaypoint = newWaypoint;
            _waypointReached = false;
        }

        private void GetPath()
        {
            _pathToWaypoint = AStar.FindPath(_gameManager.Tilemap, transform.position, _currentWaypoint.transform.position);
        }

        private IEnumerator TraversePath()
        {
            foreach (Vector3 destination in _pathToWaypoint)
            {
                yield return MoveToWaypoint(destination);
            }
        }

        private IEnumerator MoveToWaypoint(Vector3 destination)
        {
            float distance = Vector3.Distance(transform.position, destination);
            while (distance > 0.05)
            {
                if (_waypointReached) break;

                transform.position = Vector3.MoveTowards(transform.position, destination, _moveSpeed * Time.deltaTime);
                distance = Vector3.Distance(transform.position, destination);
                SetDirectionVariables(transform.position, destination);
                _isMoving = true;
                yield return null;
            }
        }
        
        private void SetDirectionVariables(Vector3 npcPos, Vector3 tilePos)
        {
            //Debug.Log("Determining Direction of Movement");
            float deltaX = tilePos.x - npcPos.x;
            float deltaY = tilePos.y - npcPos.y;

            // Debug.Log("DeltaX is: " + deltaX);
            // Debug.Log("DeltaY is: " + deltaY);
            _isWalkingRight = deltaX > 0.25;
            _isWalkingUp    = deltaY > 0.25;
        }

        private void InteractAtWaypoint()
        {
            _isInteracting = true;
            _isMoving = false;
        }
    }
}