using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using Metroidvania.InputSystem;
using System;

namespace Tests.Input
{
    /// <summary>
    /// Using this test, we can verify that input system does not work as intended 
    /// in the test environment
    /// </summary>
    public class InputSystemDoesNotWorkTest : InputTestFixture
    {
        public override void Setup()
        {
            base.Setup();
            Debug.Log("Starting test InputSystemDoesNotWorkTest");
            SceneManager.LoadScene("InputSystemTestScene");
        }

        public override void TearDown()
        {
            Debug.Log("Finished test InputSystemDoesNotWorkTest");
            base.TearDown();
        }

        [UnityTest]
        // Written following https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Testing.html
        public IEnumerator JumpInput_TriggersJumpEvent()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            
            InputReader InputReader = InputReader.instance;
            InputAction jumpEvent = InputReader.inputActions.Gameplay.Jump;
            InputReader.EnableGameplayInput();

            bool jumpTriggered = false;
            InputReader.JumpEvent += () => 
            {
                Debug.Log("JumpEvent callback triggered!");
                jumpTriggered = true;
            };

            Press(keyboard.zKey);
            InputSystem.Update(); // Just doing this to be sure
            
            // Wait for one second
            yield return TestWait.ForSeconds(1f);

            // Check if character is jumping
            Debug.Log($"Jump Action State: {jumpEvent.phase}, IsPressed: {jumpEvent.IsPressed()}");

            Release(keyboard.zKey);
            InputSystem.Update();
            yield return null;

            // If it worked, this would be true, but it's not.
            Assert.IsFalse(jumpTriggered, "Jump event was not triggered by Z key press");
        }
    }
}
