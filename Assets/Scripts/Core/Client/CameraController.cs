using Assets.Scripts.Common;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Core.Client
{
    public class CameraController : SingletonMonoBehaviour<CameraController>
    {
        public Transform target;
        public float posLerp;
        public float lookAtTime;
        public Vector3 posOffset;
        public AnimationCurve curve;
        private Tweener m_LookAtTwner;

        public void DoLook()
        {
            m_LookAtTwner = transform.DOLookAt(target.position, lookAtTime, AxisConstraint.None, target.forward).SetEase(curve);
        }

        public void Reset()
        {
            transform.position = Vector3.Lerp(transform.position, target.position + posOffset, Time.deltaTime * posLerp);
            var lookAtPos = Vector3.Normalize(target.position - transform.position);
            transform.forward = lookAtPos;
        }

        private void LateUpdate()
        {
            if (target)
            {
                transform.position = Vector3.Lerp(transform.position, target.position + posOffset, Time.deltaTime * posLerp);
                var lookAtPos = Vector3.Normalize(target.position - transform.position);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    DoLook();
                }
                else
                {
                    if (m_LookAtTwner != null)
                    {
                        if (m_LookAtTwner.IsComplete())
                        {
                            transform.forward = lookAtPos;
                        }
                    }
                    else
                    {
                        transform.forward = lookAtPos;
                    }
                }
            }
        }
    }
}