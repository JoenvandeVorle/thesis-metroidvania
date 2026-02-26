using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Tests.Input
{
    public class WallJumpTest
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test WallJumpTest");
            SceneManager.LoadScene("WallJumpLeft");
        }

        [UnityTest]
        public IEnumerator PerformWallJumpTest()
        {
            // Arrange
            bool reachedTarget = false;
            Vector2 target = GameObject.Find("Target").transform.position;
            Transform playerTransform = GameObject.Find("Player").transform;

            InputGenerator inputGenerator = InputGenerator.instance;

            // Wait for one second to ensure stuff is initialized
            yield return TestWait.ForSeconds(1f);

            // Act
            inputGenerator.HoldLeft();
            yield return TestWait.ForSeconds(0.5f);

            while (playerTransform.position.y <= 1)
            {
                inputGenerator.PressJump();
                yield return TestWait.ForSeconds(0.5f);
            }

            while (!reachedTarget)
            {
                yield return null;
                if (Vector2.Distance(playerTransform.position, target) < 0.5f)
                {
                    reachedTarget = true;
                    inputGenerator.ReleaseMovement();
                }
            }

            // Assert
            Assert.IsTrue(reachedTarget, "Player did not reach the target");

            yield return null;
        }
    }
}
