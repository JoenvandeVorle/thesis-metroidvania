using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Metroidvania.InputSystem;

namespace Tests.Input
{
    public class WalkingTest
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test WalkingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [UnityTest]
        public IEnumerator PerformWalkingTest()
        {
            // Arrange
            int walkDirection = 0;
            bool walkedLeft = false;
            bool walkedRight = false;
            bool movementStopped = false;

            InputGenerator inputGenerator = InputGenerator.instance;
            inputGenerator.EnableDebugPrints();
            InputReader.instance.MoveEvent += dir =>
            {
                walkDirection = dir == 0 ? 0 : (dir < 0 ? -1 : 1);
            };

            // Wait for one second to ensure stuff is initialized
            yield return TestWait.ForSeconds(1f);

            // Act
            // Try walking left first
            inputGenerator.HoldLeft();
            yield return TestWait.ForSeconds(1.5f);
            walkedLeft = walkDirection == -1;
            inputGenerator.ReleaseMovement();

            inputGenerator.HoldRight();
            yield return TestWait.ForSeconds(1.5f);
            walkedRight = walkDirection == 1;
            inputGenerator.ReleaseMovement();

            yield return TestWait.ForSeconds(0.5f);
            movementStopped = walkDirection == 0;

            // Assert
            Assert.IsTrue(walkedLeft, "Left walk was not triggered");
            Assert.IsTrue(walkedRight, "Right walk was not triggered");
            Assert.IsTrue(movementStopped, "Movement release was not triggered");
            yield return null;
        }
    }
}

