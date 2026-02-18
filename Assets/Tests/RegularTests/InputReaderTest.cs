using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Metroidvania.InputSystem;
using UnityEngine.InputSystem;

namespace Tests.Input
{
    /// <summary>
    /// Simple test to verify that jumping is working via the custom TestInputGenerator.
    /// </summary>
    public class JumpingTest
    {
        [UnityTest]
        public IEnumerator PerformJumpingTest()
        {
            // Arrange
            InputAction jumpAction = InputSystem.actions.FindAction("Gameplay/Jump");
            bool jumpTriggered = false;

            InputReader inputReader = InputReader.instance;
            inputReader.JumpEvent += () => 
            {
                Debug.Log("JumpEvent callback triggered!");
                jumpTriggered = true;
            };

            // Act
            while (!jumpTriggered)
            {
                // TestInputGenerator.PressJump();
                yield return null; // wait one frame
                // TestInputGenerator.ReleaseJump();
                // yield return null; // wait one frame
            }

            // Assert
            Assert.IsTrue(jumpTriggered, "Jump event was not triggered by Z key press");
            yield return null;
        }

        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test JumpingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }
    }
}
