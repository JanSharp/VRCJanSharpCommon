using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerCapsuleSizeProber : UdonSharpBehaviour
    {
        // Turns out eye height and the avatar do not matter, the capsule is always the exact same.
        // The local player capsule is this:
        // 1.65 height
        // 0.4 diameter
        // Determined through trial and error messing with the character controller in the inspector
        // trying to match the values gotten from within VRChat, those being:
        // capsule height: 1.320007
        // capsule width: 0.319946

        // It would appear the player capsule can actually pretty exactly fit into a 0.425 wide gap.
        // And it would appear it can just barely fit under 1.6625, while colliding with 1.65.
        // Those observations have been made using a test setup in the RP Menu dev scene.

        public Transform bottomSphere;
        public Transform topSphere;
        public Transform cylinder;
        public LayerMask localPlayerLayer;
        public TextMeshProUGUI probedOutputText;
        [HideInInspector][SerializeField][SingletonReference] private QuickDebugUI qd;

        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }

        private void Update()
        {
            bottomSphere.gameObject.SetActive(false);
            topSphere.gameObject.SetActive(false);
            cylinder.gameObject.SetActive(false);
            probedOutputText.text = "Unknown";

            Vector3 avatarRoot = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.AvatarRoot).position;
            if (!Physics.Raycast(avatarRoot + Vector3.down * 100f, Vector3.up, out RaycastHit hit, 110f, localPlayerLayer))
                return;
            Vector3 firstBottomHit = hit.point;
            qd.ShowForOneFrame(this, "firstBottomHit", firstBottomHit.ToString("f6"));
            if (!Physics.Raycast(avatarRoot + Vector3.up * 100f, Vector3.down, out hit, 110f, localPlayerLayer))
                return;
            Vector3 firstTopHit = hit.point;
            qd.ShowForOneFrame(this, "firstTopHit", firstTopHit.ToString("f6"));
            Vector3 firstCenter = (firstBottomHit + firstTopHit) / 2f;
            if (!Physics.Raycast(firstCenter + Vector3.left * 100f, Vector3.right, out hit, 110f, localPlayerLayer))
                return;
            Vector3 firstLeftHit = hit.point;
            qd.ShowForOneFrame(this, "firstLeftHit", firstLeftHit.ToString("f6"));
            if (!Physics.Raycast(firstCenter + Vector3.right * 100f, Vector3.left, out hit, 110f, localPlayerLayer))
                return;
            Vector3 firstRightHit = hit.point;
            qd.ShowForOneFrame(this, "firstRightHit", firstRightHit.ToString("f6"));
            Vector3 secondCenter = (firstLeftHit + firstRightHit) / 2f;
            if (!Physics.Raycast(secondCenter + Vector3.back * 100f, Vector3.forward, out hit, 110f, localPlayerLayer))
                return;
            Vector3 firstBackHit = hit.point;
            qd.ShowForOneFrame(this, "firstBackHit", firstBackHit.ToString("f6"));
            if (!Physics.Raycast(secondCenter + Vector3.forward * 100f, Vector3.back, out hit, 110f, localPlayerLayer))
                return;
            Vector3 firstFrontHit = hit.point;
            qd.ShowForOneFrame(this, "firstFrontHit", firstFrontHit.ToString("f6"));
            Vector3 thirdCenter = (firstBackHit + firstFrontHit) / 2f;
            if (!Physics.Raycast(thirdCenter + Vector3.down * 100f, Vector3.up, out hit, 110f, localPlayerLayer))
                return;
            Vector3 bottomCenter = hit.point;
            qd.ShowForOneFrame(this, "bottomCenter", bottomCenter.ToString("f6"));
            if (!Physics.Raycast(thirdCenter + Vector3.up * 100f, Vector3.down, out hit, 110f, localPlayerLayer))
                return;
            Vector3 topCenter = hit.point;
            qd.ShowForOneFrame(this, "topCenter", topCenter.ToString("f6"));
            float diameter = firstFrontHit.z - firstBackHit.z;
            float height = topCenter.y - bottomCenter.y;

            bottomSphere.localScale = Vector3.one * diameter;
            topSphere.localScale = Vector3.one * diameter;
            cylinder.localScale = new Vector3(diameter, height - diameter, diameter);

            bottomSphere.position = bottomCenter + Vector3.up * diameter / 2f;
            topSphere.position = topCenter + Vector3.down * diameter / 2f;
            cylinder.position = thirdCenter;

            bottomSphere.gameObject.SetActive(true);
            topSphere.gameObject.SetActive(true);
            cylinder.gameObject.SetActive(true);
            probedOutputText.text = $"eye height: {localPlayer.GetAvatarEyeHeightAsMeters():f6}\n"
                + $"avatarRoot.y: {avatarRoot.y:f6}\n"
                + $"bottomCenter.y: {bottomCenter.y:f6}\n"
                + $"capsule height: {height:f6}\n"
                + $"capsule diameter: {diameter:f6}\n"
                + $"guessed height: {height + (bottomCenter.y - avatarRoot.y) * 2f:f6}\n"
                + $"guessed diameter: {diameter + (bottomCenter.y - avatarRoot.y) * 2f:f6}\n";
        }
    }
}
