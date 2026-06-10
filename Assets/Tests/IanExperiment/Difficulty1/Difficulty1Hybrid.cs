using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Difficulty1Hybrid : HybridExperiment
    {
        private static readonly string[] _scenes =
        {
            "trivial", "trivialrv", "diver", "e1", "g1", "g1u1", "g2u1", "g2u1ceil", "g2u2",
            "offscreen-pitfall", "pitfall", "u1", "u2", "u3"
        };

        private static string _resultsFile = "difficulty1.jsonl";

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator PerformDifficulty1([ValueSource(nameof(_scenes))] string scene, [ValueSource(nameof(_runs))] int run)
        {
            Debug.Log($"Starting Hybrid experiment {scene}");
            SceneManager.LoadScene(scene);
            yield return null; // let the scene finish loading before SetupExperiment runs
            yield return RunExperiment(_resultsFile, run);
        }
    }
}
