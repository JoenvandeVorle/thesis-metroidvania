using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroidvania.InputSystem
{
    public class TestableInputAction
    {
        public InputAction InputAction { get; private set; }
        private bool isSimulatingPress = false; // Is the key being held down (simulated)
        private bool wasSimulatedPressPerformedThisFrame = false; // Was the key just pressed this frame (simulated)
        private float simulatedHoldDuration = 0f;
        private float simulatedPressElapsedTime = 0f;

        public TestableInputAction(InputAction inputAction)
        {
            InputAction = inputAction;
        }

        public bool IsPressed()
        {
            return InputAction.IsPressed() || isSimulatingPress;
        }

        public bool WasPerformedThisFrame()
        {
            return InputAction.WasPerformedThisFrame() || wasSimulatedPressPerformedThisFrame;
        }

        public void Update()
        {
            if (simulatedPressElapsedTime > 0f)
                wasSimulatedPressPerformedThisFrame = false;

            if (isSimulatingPress)
            {
                simulatedPressElapsedTime += Time.deltaTime;
                if (simulatedPressElapsedTime >= simulatedHoldDuration && simulatedHoldDuration > 0f)
                {
                    ReleaseFromHolding();
                }
            }
        }

        public void HoldFor(float duration)
        {
            if (isSimulatingPress)
                return;

            simulatedHoldDuration = duration;
            isSimulatingPress = true;
            wasSimulatedPressPerformedThisFrame = true;
        }

        public void SimulatePress()
        {
            simulatedHoldDuration = -1f;
            isSimulatingPress = true;
            wasSimulatedPressPerformedThisFrame = true;
        }

        public void SimulateRelease()
        {
            if (simulatedHoldDuration > 0f)
                return;

            isSimulatingPress = false;
        }

        private void ReleaseFromHolding()
        {
            if (simulatedHoldDuration > 0f)
            {
                isSimulatingPress = false;
                simulatedPressElapsedTime = 0f;
                simulatedHoldDuration = -1f;
            }
        }
    }
}
