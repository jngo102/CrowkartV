using PowerslideKartPhysics;
using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

namespace CrowkartVRMod
{
    internal class KartInputVR : KartInput
    {
        private CircularDrive _drive;
        private const float _accelMultiplier = 2;
        private const float _steerMultiplier = 0.25f;

        public override void Awake()
        {
            base.Awake();

            SteamVR_Actions.buggy.Activate();
            SteamVR_Actions.platformer.Activate();
        }

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => FindObjectOfType<CircularDrive>() == null);
            _drive = FindObjectOfType<CircularDrive>();
        }

        public override void Update()
        {
            if (caster != null &&
                Mathf.Abs(SteamVR_Actions.buggy_Throttle.GetAxis(SteamVR_Input_Sources.RightHand)) > Mathf.Epsilon)
            {
                caster.Cast();
            }

            drifting = Mathf.Abs(SteamVR_Actions.buggy_Throttle.GetAxis(SteamVR_Input_Sources.LeftHand)) > Mathf.Epsilon;
            if (Mathf.Abs(SteamVR_Actions.platformer_Move.GetAxis(SteamVR_Input_Sources.LeftHand).y) > Mathf.Epsilon)
            {
                if (SteamVR_Actions.platformer_Move.GetAxis(SteamVR_Input_Sources.LeftHand).y > 0)
                {
                    targetAccel = SteamVR_Actions.platformer_Move.GetAxis(SteamVR_Input_Sources.LeftHand).y * _accelMultiplier;
                }
                else if (SteamVR_Actions.platformer_Move.GetAxis(SteamVR_Input_Sources.LeftHand).y < 0)
                {
                    targetBrake = SteamVR_Actions.platformer_Move.GetAxis(SteamVR_Input_Sources.LeftHand).y * _accelMultiplier;
                }
            }
            else
            {
                targetAccel = targetBrake = 0;
            }

            if (_drive != null)
            {
                targetSteer = _drive.outAngle % 360 * Mathf.Deg2Rad * _steerMultiplier;
            }

            base.Update();
        }
    }
}
