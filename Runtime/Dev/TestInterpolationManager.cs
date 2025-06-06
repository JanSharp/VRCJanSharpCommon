using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TestInterpolationManager : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField][SingletonReference] private InterpolationManager manager;

        public GameObject testPrefab;

        private void Start()
        {
            Position();
            Rotation();
            // Combo();
        }

        private Transform positionInst;
        private Vector3 basePos = Vector3.right * 10f;
        private void Position()
        {
            positionInst = Instantiate(testPrefab).transform;
            positionInst.localPosition = basePos;
            Function1();
        }
        public void Function1()
        {
            manager.LerpLocalPosition(positionInst, basePos + Vector3.forward * 5f, 1f, this, nameof(Function2), null);
        }
        public void Function2()
        {
            manager.LerpLocalPosition(positionInst, basePos, 0.2f, this, nameof(Function1), null);
        }

        private Transform rotationInst;
        private void Rotation()
        {
            rotationInst = Instantiate(testPrefab).transform;
            rotationInst.localPosition = Vector3.right * 12.5f;
            Function3();
        }
        public void Function3()
        {
            manager.LerpLocalRotation(rotationInst, Quaternion.AngleAxis(135f, Vector3.up), 1f, this, nameof(Function4), null);
        }
        public void Function4()
        {
            manager.LerpLocalRotation(rotationInst, Quaternion.identity, 0.2f, this, nameof(Function3), null);
        }

        // private Transform comboInst;
        // private void Combo()
        // {
        //     comboInst = Instantiate(testPrefab).transform;
        //     comboInst.localPosition = Vector3.right * 15f;
        //     Function5();
        // }
        // public void Function5()
        // {
        //     manager.LerpLocalScale(comboInst, Vector3.one * 2f, 0.5f, this, nameof(Function6), null);
        // }
        // public void Function6()
        // {
        //     manager.LerpLocalRotation(comboInst, Quaternion.AngleAxis(90f, Vector3.right), 0.25f, this, nameof(Function7), null);
        // }
        // public void Function7()
        // {
        //     manager.LerpLocalPositionAndRotation(comboInst, new Vector3(15f, 0f, 2f), Quaternion.AngleAxis(-90f, Vector3.right), 0.75f, this, nameof(Function8), null);
        // }
        // public void Function8()
        // {
        //     manager.LerpLocalPositionAndRotationAndScale(comboInst, Vector3.right * 15f, Quaternion.identity, Vector3.one, 2f, this, nameof(Function5), null);
        // }
    }
}
