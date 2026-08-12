using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Difficulty3RL : RLExperiment
    {
        private static readonly string[] _scenes =
        {
            "clouds", "dropper", "hostile-clouds", "ladder", "target-spotted"
        };

        private static string _resultsFile = "difficulty3.jsonl";

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator PerformDifficulty3([ValueSource(nameof(_scenes))] string scene, [ValueSource(nameof(_runs))] int run)
        {
            Debug.Log($"Starting RL experiment {scene}");
            SceneManager.LoadScene(scene);
            yield return null;
            yield return RunExperiment(_resultsFile, run);
        }
    }

}
