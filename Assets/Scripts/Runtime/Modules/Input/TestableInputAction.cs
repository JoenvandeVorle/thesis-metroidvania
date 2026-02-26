using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroidvania.InputSystem
{
    public class TestableInputAction
    {
        public InputAction InputAction { get; private set; }

        private bool isSimulatingPress = false; // Is the key being held down (simulated)
        private bool wasSimulatedPressPerformedThisFrame = false; // Was the key just pressed this frame (simulated)
        private float simulatedPressDuration = 0f;
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
            if (isSimulatingPress)
            {
                // if (InputAction.name == "Jump" && simulatedPressElapsedTime % 0.2f <= 0.01f)
                //     Debug.Log($"Simulating Jump Press: {simulatedPressElapsedTime}/{simulatedPressDuration} s");

                if (simulatedPressElapsedTime > 0f)
                    wasSimulatedPressPerformedThisFrame = false;

                simulatedPressElapsedTime += Time.deltaTime;
                if (simulatedPressElapsedTime >= simulatedPressDuration)
                {
                    SimulateRelease();
                    simulatedPressElapsedTime = 0f;
                }
            }
        }

        public void HoldFor(float duration)
        {
            simulatedPressDuration = duration;
            SimulatePress();
        }

        public void SimulatePress()
        {
            isSimulatingPress = true;
            wasSimulatedPressPerformedThisFrame = true;
        }

        public void SimulateRelease()
        {
            isSimulatingPress = false;
        }
    }
}
