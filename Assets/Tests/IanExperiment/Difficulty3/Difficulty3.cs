using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Difficulty3 : BaseExperiment
    {
        private static readonly string[] _scenes =
        {
            "clouds", "dropper", "hostile-clouds", "ladder", "target-spotted"
        };

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator PerformDifficulty3([ValueSource(nameof(_scenes))] string scene, [ValueSource(nameof(_runs))] int run)
        {
            Debug.Log($"Starting experiment {scene}");
            SceneManager.LoadScene(scene);
            yield return null;
            yield return PerformExperiment(run);
        }
    }

}
