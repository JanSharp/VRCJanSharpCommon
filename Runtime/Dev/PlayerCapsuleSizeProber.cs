using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

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

        public Transform bottomSphere;
        public Transform topSphere;
        public Transform cylinder;
        public LayerMask localPlayerLayer;
        public TextMeshProUGUI probedOutputText;
        [SingletonReference] public QuickDebugUI qd;
        private Vector3 firstBottomHit;
        private Vector3 firstTopHit;
        private Vector3 firstLeftHit;
        private Vector3 firstRightHit;
        private Vector3 firstBackHit;
        private Vector3 firstFrontHit;
        private Vector3 bottomCenter;
        private Vector3 topCenter;

        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            qd.Add(this, "firstBottomHit", nameof(UpdateFirstBottomHit));
            qd.Add(this, "firstTopHit", nameof(UpdateFirstTopHit));
            qd.Add(this, "firstLeftHit", nameof(UpdateFirstLeftHit));
            qd.Add(this, "firstRightHit", nameof(UpdateFirstRightHit));
            qd.Add(this, "firstBackHit", nameof(UpdateFirstBackHit));
            qd.Add(this, "firstFrontHit", nameof(UpdateFirstFrontHit));
            qd.Add(this, "bottomCenter", nameof(UpdateBottomCenter));
            qd.Add(this, "topCenter", nameof(UpdateTopCenter));
        }

        public void UpdateFirstBottomHit() => qd.DisplayValue = firstBottomHit.ToString();
        public void UpdateFirstTopHit() => qd.DisplayValue = firstTopHit.ToString();
        public void UpdateFirstLeftHit() => qd.DisplayValue = firstLeftHit.ToString();
        public void UpdateFirstRightHit() => qd.DisplayValue = firstRightHit.ToString();
        public void UpdateFirstBackHit() => qd.DisplayValue = firstBackHit.ToString();
        public void UpdateFirstFrontHit() => qd.DisplayValue = firstFrontHit.ToString();
        public void UpdateBottomCenter() => qd.DisplayValue = bottomCenter.ToString();
        public void UpdateTopCenter() => qd.DisplayValue = topCenter.ToString();

        private void Update()
        {
            bottomSphere.gameObject.SetActive(false);
            topSphere.gameObject.SetActive(false);
            cylinder.gameObject.SetActive(false);
            probedOutputText.text = "Unknown";

            Vector3 avatarRoot = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.AvatarRoot).position;
            if (!Physics.Raycast(avatarRoot + Vector3.down * 100f, Vector3.up, out RaycastHit hit, 110f, localPlayerLayer))
                return;
            firstBottomHit = hit.point;
            if (!Physics.Raycast(avatarRoot + Vector3.up * 100f, Vector3.down, out hit, 110f, localPlayerLayer))
                return;
            firstTopHit = hit.point;
            Vector3 firstCenter = (firstBottomHit + firstTopHit) / 2f;
            if (!Physics.Raycast(firstCenter + Vector3.left * 100f, Vector3.right, out hit, 110f, localPlayerLayer))
                return;
            firstLeftHit = hit.point;
            if (!Physics.Raycast(firstCenter + Vector3.right * 100f, Vector3.left, out hit, 110f, localPlayerLayer))
                return;
            firstRightHit = hit.point;
            Vector3 secondCenter = (firstLeftHit + firstRightHit) / 2f;
            if (!Physics.Raycast(secondCenter + Vector3.back * 100f, Vector3.forward, out hit, 110f, localPlayerLayer))
                return;
            firstBackHit = hit.point;
            if (!Physics.Raycast(secondCenter + Vector3.forward * 100f, Vector3.back, out hit, 110f, localPlayerLayer))
                return;
            firstFrontHit = hit.point;
            Vector3 thirdCenter = (firstBackHit + firstFrontHit) / 2f;
            if (!Physics.Raycast(thirdCenter + Vector3.down * 100f, Vector3.up, out hit, 110f, localPlayerLayer))
                return;
            bottomCenter = hit.point;
            if (!Physics.Raycast(thirdCenter + Vector3.up * 100f, Vector3.down, out hit, 110f, localPlayerLayer))
                return;
            topCenter = hit.point;
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
