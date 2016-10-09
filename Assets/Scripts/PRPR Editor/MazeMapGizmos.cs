using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.PRPR_Editor
{
    public class MazeMapGizmos : SingletonMonoBehaviour<MazeMapGizmos>
    {
        public Vector3 pathDirection;
        public Vector3 pathPos;

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(pathPos, pathDirection);
            Gizmos.DrawSphere(pathPos, 0.1f);
        }
    }
}