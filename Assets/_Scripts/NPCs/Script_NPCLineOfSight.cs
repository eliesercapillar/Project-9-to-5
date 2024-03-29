using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace NPC
{
    public class Script_NPCLineOfSight : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private Script_NPCAnimationManager _animationManager;
        [SerializeField] private Script_NPCMoodManager _moodManager;

        [Header("Player")]
        [SerializeField] private GameObject _player;
        [SerializeField] private LayerMask _playerHitboxLayerMask;

        [Header("Components")]
        [SerializeField] private MeshFilter _meshFilter;

        [Header("Field of View Properties")]
        private Vector3[] _meshVertices;
        private Vector2[] _meshUV;
        private int[] _meshTriangles;
        private float _startingAngle;
        private Vector3 _lookDirection;
        [SerializeField] private float _fov;
        [SerializeField] private int _numLOSRays;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _minLOSViewDistance;
        [SerializeField] private float _maxLOSViewDistance;
        private float _currViewDistance;

        private bool _isShrinking = false;
        private bool _isExpanding = false;

        // Getters / Setters
        public float FOV             {set {_fov = value;}}
        public float MinViewDistance {set {_minLOSViewDistance = value;}}
        public float MaxViewDistance {set {_maxLOSViewDistance = value;}}
    
        private void Start()
        {
            _currViewDistance = _maxLOSViewDistance;
            _meshVertices = new Vector3[_numLOSRays + 2];
            _meshUV = new Vector2[_numLOSRays + 2];
            _meshTriangles = new int[_numLOSRays * 3];
            if (_player == null) _player = GameObject.Find("Player");
        }

        private void FixedUpdate()
        {
            DrawFOVMesh();
            PollForPlayerInFOV();
        }

        private void PollForPlayerInFOV()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
            // Subtract a small buffer to offset measuring from center of sprite
            if (distanceToPlayer - 0.5f > _currViewDistance)
            {
                _moodManager.IsInLOS = false;
                return;   
            }
            //Debug.Log("Within currViewDistance");

            Vector3 playerDirection = (_player.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(_lookDirection, playerDirection);
            if (angle > _fov / 2f)
            {
                _moodManager.IsInLOS = false;
                return;   
            }
            //Debug.Log("Within Angle");
            
            RaycastHit2D rayHit = Physics2D.Raycast(transform.position, playerDirection, _currViewDistance, _playerHitboxLayerMask);
            if (rayHit.collider == null)
            {
                _moodManager.IsInLOS = false;
                return;   
            }
            //Debug.Log($"Ray hit something! It was {rayHit.collider.gameObject.name}");

            if (rayHit.collider.tag == "TAG_Player")
            {
                _moodManager.IsInLOS = true;
            }

        }

        private void DrawFOVMesh()
        {
            // Draw LOS Mesh
            float currentAngle = _startingAngle;
            float angleIncrement = _fov / _numLOSRays;

            _meshVertices[0] = Vector3.zero;

            int vertexIndex = 1;
            int triangleIndex = 0;

            for (int i = 0; i <= _numLOSRays; i++)
            {
                Vector3 vertex;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, HelperMethods.FloatToVectorAngle(currentAngle), _currViewDistance, _layerMask);

                if (hit.collider == null) 
                {
                    //Debug.DrawRay(transform.position, FloatToVectorAngle(currentAngle) * _viewDistance, Color.white);
                    vertex = Vector3.zero + HelperMethods.FloatToVectorAngle(currentAngle) * _currViewDistance;
                }
                else if (hit.collider.tag == "TAG_Obstacle")
                {
                    //Debug.DrawRay(transform.position, FloatToVectorAngle(currentAngle) * _viewDistance, Color.red);
                    float distanceToHit = Vector2.Distance(hit.point, transform.position);
                    vertex = Vector3.zero + HelperMethods.FloatToVectorAngle(currentAngle) * distanceToHit;
                }
                else
                {
                    Debug.Log("Hit something but not obstacle. What was hit was: " + hit.collider.gameObject.name);
                    vertex = Vector3.zero + HelperMethods.FloatToVectorAngle(currentAngle) * _currViewDistance;
                }
                _meshVertices[vertexIndex] = vertex;

                if (i != 0)
                {
                    _meshTriangles[triangleIndex++] = 0;
                    _meshTriangles[triangleIndex++] = vertexIndex - 1;
                    _meshTriangles[triangleIndex++] = vertexIndex;  
                }

                vertexIndex++;
                currentAngle -= angleIncrement; // Clockwise
            }

            if (_meshFilter.mesh == null)
            {
                _meshFilter.mesh = new Mesh
                {
                    vertices = _meshVertices,
                    uv = _meshUV,
                    triangles = _meshTriangles
                };
            }
            else
            {
                _meshFilter.mesh.vertices = _meshVertices;
                _meshFilter.mesh.uv = _meshUV;
                _meshFilter.mesh.triangles = _meshTriangles;
            }
        }

        public void SetRayDirection(Vector3 direction)
        {
            _lookDirection = direction;
            _startingAngle = HelperMethods.VectorToFloatAngle(direction) + _fov / 2;
        }

        public void ShrinkLOS()
        {
            // TODO: Smooth Shrink == Lerp?
            if (!_isShrinking && !_isExpanding) StartCoroutine(Shrink());

            IEnumerator Shrink()
            {
                _isShrinking = true;
                while (_currViewDistance > _minLOSViewDistance)
                {
                    _currViewDistance--;
                    yield return new WaitForSeconds(0.1f);
                }
                _isShrinking = false;
            }
        }

        public void ExpandLOS()
        {
            // TODO: Smooth Expand == Lerp?
            if (!_isExpanding && !_isShrinking) StartCoroutine(Expand());

            IEnumerator Expand()
            {
                _isExpanding = true;
                while (_currViewDistance < _maxLOSViewDistance)
                {
                    _currViewDistance++;
                    yield return new WaitForSeconds(0.1f);
                }
                _isExpanding = false;
            }
        }

    
    }
}
