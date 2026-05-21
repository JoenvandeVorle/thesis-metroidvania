using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Difficulty2 : BaseExperiment
    {
        private static readonly string[] _scenes =
        {
            "backwards-progress", "cave", "caverv", "faith", "fork", "g1r1", "g1r1rv",
            "g6u1", "infestation", "jump-limit", "jumparound", "softlockable", "staircase", "up"
        };

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator PerformDifficulty2([ValueSource(nameof(_scenes))] string scene)
        {
            Debug.Log($"Starting experiment {scene}");
            SceneManager.LoadScene(scene);
            yield return null;
            yield return PerformExperiment();
        }
    }

}
