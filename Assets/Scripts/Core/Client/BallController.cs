using Assets.Scripts.Common;
using Assets.Scripts.Core.Client.Structure;
using Assets.Scripts.Tool;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Client
{
    public class BallController : SingletonMonoBehaviour<BallController>
    {
        public GameObject ball;
        public TextAsset xmlAsset;
        private float m_ReactSpeed;
        private List<FlexGOInfo> m_FlexInfoList;
        private MazeInfo m_MazeInfo;
        private int m_Idx = 0;
        private Tweener m_MoveTweener;
        private Transform[] m_Flexs;
        private AudioClip m_Clip;

        private void Awake()
        {
            var mazeInfo = XmlUtil.Deserialize(typeof(MazeInfo), xmlAsset.text) as MazeInfo;
            m_FlexInfoList = FlexFactory.CreateInfoList(mazeInfo.flexNodeList, Vector3.zero, Vector3.forward, 1.0f);
            m_ReactSpeed = mazeInfo.scale / mazeInfo.reactTime;
            m_Clip = ResourcesLoader.Load(mazeInfo.musicPath) as AudioClip;
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = m_Clip;
            audioSource.Play();
        }

        private void Start()
        {
            Move2NextPos();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Move2NextPos();
            }
        }

        private void Move2NextPos()
        {
            if (m_MoveTweener != null)
            {
                m_MoveTweener.Kill();
            }
            if (m_Idx > m_FlexInfoList.Count - 1)
            {
                return;
            }
            var flexNode = m_FlexInfoList[m_Idx++];
            var dir = flexNode.fwd;
            if (dir == ball.transform.forward)
            {
                Move2NextPos();
                return;
            }
            ball.transform.forward = dir;
            m_MoveTweener = ball.transform.DOMove(ball.transform.position + dir * 1000.0f, 1000.0f / m_ReactSpeed).SetEase(Ease.Linear);
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.tag == "Wall")
            {
                if (m_MoveTweener != null)
                {
                    m_MoveTweener.Kill();
                    m_MoveTweener = null;
                    transform.position = Vector3.up;
                    transform.forward = Vector3.up;
                    DOTweenUtil.Delay(() =>
                    {
                        gameObject.GetComponent<AudioSource>().Stop();
                        gameObject.GetComponent<AudioSource>().Play();
                        m_Idx = 0;
                        Move2NextPos();
                    }, 1.0f);
                }
            }
        }
    }
}