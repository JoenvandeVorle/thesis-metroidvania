using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Metroidvania.InputSystem;

namespace Tests.Input
{
    /// <summary>
    /// Simple test to verify that jumping is working via the custom InputGenerator.
    /// </summary>
    public class JumpingTest
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test JumpingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [UnityTest]
        public IEnumerator PerformJumpingTest()
        {
            // Arrange
            bool jumpTriggered = false;

            InputGenerator inputGenerator = InputGenerator.instance;
            inputGenerator.EnableDebugPrints();
            InputReader.instance.JumpEvent += () =>
            {
                jumpTriggered = true;
            };

            // Wait for one second to ensure stuff is initialized
            yield return TestWait.ForSeconds(1f);

            // Act
            inputGenerator.PressJumpFor(0.05f);
            yield return TestWait.ForSeconds(1f); // wait for animation

            inputGenerator.PressJumpFor(0.2f);
            yield return TestWait.ForSeconds(1f);

            inputGenerator.PressJumpFor(2f);
            yield return TestWait.ForSeconds(2f);

            // Assert
            Assert.IsTrue(jumpTriggered, "Jump event was not triggered" );
            yield return null;
        }
    }
}
