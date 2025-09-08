using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RNGTest : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField][SingletonReference] private WannaBeClassesManager wannaBeClasses;
        [SerializeField] private GameObject cubePrefab;

        public void Start()
        {
            RNG rng = wannaBeClasses.New<RNG>(nameof(RNG));
            rng.SetSeed(0uL);
            Debug.Log($"[JanSharpCommon] seed: 0x{rng.seed:X16}, lcg: 0x{rng.lcg:X16}, hash: 0x{rng.hash:X16}");
            rng.SetSeed((ulong)Random.Range(0, int.MaxValue));

            for (int i = 0; i < 16; i++)
            {
                double value = rng.GetDouble01();
                Debug.Log($"[JanSharpCommon] random double01 {i + 1}: {value:F10}");
            }

            for (int i = 0; i < 16; i++)
            {
                float value = rng.GetFloat01();
                Debug.Log($"[JanSharpCommon] random float01 {i + 1}: {value:F10}");
            }

            int count = 16;
            int[] shuffledArray = new int[count];
            for (int i = 0; i < count; i++)
                shuffledArray[i] = i;
            rng.ShuffleArray(shuffledArray, startIndex: 4, count: 8);
            for (int i = 0; i < count; i++)
                Debug.Log($"[JanSharpCommon] shuffledArray[{i}]: {shuffledArray[i]}");

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            for (int x = 0; x < 16; x++)
                for (int z = 0; z < 16; z++)
                {
                    sw.Start();
                    float y = rng.Range(1f, 5f);
                    Quaternion rotation = rng.GetQuaternion();
                    Color color = rng.GetColorHSV();
                    sw.Stop();
                    GameObject go = Instantiate(cubePrefab, this.transform);
                    Material material = go.GetComponent<Renderer>().material;
                    go.transform.SetPositionAndRotation(new Vector3(x + 8f, y, -z - 8f), rotation);
                    material.color = color;
                }
            Debug.Log($"[JanSharpCommon] rng for 256 randomly positioned, rotated and colored cubes took {sw.Elapsed.TotalMilliseconds}ms.");
        }
    }
}
