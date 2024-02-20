using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC
{
    public class Script_NPCLineOfSight : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private Script_NPCActionsManager _actionsManager;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.tag == "TAG_PlayerHitbox")
            {
                _actionsManager.IsSus = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "TAG_PlayerHitbox")
            {
                _actionsManager.IsSus = false;
            }
        }
    }
}