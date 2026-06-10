using System.Collections;
using System.Collections.Generic;
using System.IO;
using Aplib.Core;
using Aplib.Core.Belief.Beliefs;
using Aplib.Core.Belief.BeliefSets;
using Aplib.Integrations.Unity;
using Metroidvania.Characters.Knight;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Experiments
{
    public enum AgentType
    {
        Hybrid,
        Aplib,
        RL
    }

    public class ExperimentBeliefSet : BeliefSet
    {
        public readonly Belief<GameObject, GameObject> Player =
            new(reference: GameObject.Find("Player"), x => x);

        public readonly Belief<GameObject, KnightCharacterController> PlayerController =
            new(reference: GameObject.Find("Player"), x => x.GetComponent<KnightCharacterController>());

        public readonly Belief<GameObject, GameObject> Target = new(
            GameObject.Find("Target"), x => x);

        public readonly Belief<TargetPathFinder, List<Vector2>> currentPath = new(
            GameObject.Find("Player").GetComponent<TargetPathFinder>(), x => x.GetCurrentPath());
    }

    public abstract class BaseExperiment
    {
        protected static readonly int[] _runs = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        protected abstract AgentType AgentType { get; }
        protected InputGenerator inputGenerator;
        protected ExperimentBeliefSet beliefSet;
        protected GameObject player;
        protected AbortableAplibRunner runner;

        private static Dictionary<string, int> _writtenKeys;
        private static string ResultKey(AgentType agentType, string scene, int run) => $"{agentType}:{scene}:{run}";

        protected virtual void Arrange()
        {
            inputGenerator = InputGenerator.instance;
            beliefSet = new ExperimentBeliefSet();
            player = beliefSet.Player;
        }

        public abstract IEnumerator Act(int run);

        public IEnumerator RunExperiment(string resultsFileName, int run = 1)
        {
            Arrange();

            Assert.GreaterOrEqual(player.transform.position.y, -10, "Player should start above y = -10");

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            try
            {
                yield return Act(run);
            }
            finally
            {
                WriteResult(
                    fileName: resultsFileName,
                    agentType: AgentType,
                    scene: UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    run: run,
                    status: runner.Status.ToString(),
                    durationSeconds: stopWatch.Elapsed.TotalSeconds);
            }

            Assert.AreEqual(CompletionStatus.Success, runner.Status, $"Experiment failed. Abort reason: {runner.AbortReason}");
        }

        protected static Dictionary<string, int> getWrittenKeys(string filePath, AgentType agentType)
        {
            if (_writtenKeys != null) return _writtenKeys;
            _writtenKeys = new Dictionary<string, int>();
            if (!File.Exists(filePath)) return _writtenKeys;

            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                int sceneStart = line.IndexOf("\"scene\":\"") + 9;
                int sceneEnd = line.IndexOf('"', sceneStart);
                int runStart = line.IndexOf("\"run\":") + 6;
                int runEnd = line.IndexOf(',', runStart);
                int agentStart = line.IndexOf("\"agent\":\"") + 9;
                int agentEnd = line.IndexOf('"', agentStart);
                if (sceneEnd < 0 || runEnd < 0 || agentEnd < 0) continue;
                string s = line.Substring(sceneStart, sceneEnd - sceneStart);
                string a = line.Substring(agentStart, agentEnd - agentStart);
                if (!System.Enum.TryParse(a, out AgentType aType) || aType != agentType) continue;
                if (int.TryParse(line.Substring(runStart, runEnd - runStart), out int r))
                    _writtenKeys[ResultKey(agentType, s, r)] = i;
            }
            Debug.Log($"Written keys loaded: {string.Join(", ", _writtenKeys.Keys)}");
            return _writtenKeys;
        }

        protected static void WriteResult(string fileName, AgentType agentType, string scene, int run, string status, double durationSeconds)
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ExperimentResults"));
            Directory.CreateDirectory(dir);
            string duration = durationSeconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            string newLine = $"{{ \"agent\":\"{agentType}\", \"scene\":\"{scene}\",\"run\":{run},\"status\":\"{status}\",\"duration\":{duration}}}";
            string filePath = Path.Combine(dir, fileName);
            string key = ResultKey(agentType, scene, run);
            Dictionary<string, int> writtenKeys = getWrittenKeys(filePath, agentType);

            // Replace line if it exists, otherwise append
            if (writtenKeys.TryGetValue(key, out int lineIndex))
            {
                string[] lines = File.ReadAllLines(filePath);
                lines[lineIndex] = newLine;
                File.WriteAllLines(filePath, lines);
                return;
            }

            File.AppendAllText(filePath, newLine + "\n");
            _writtenKeys[key] = writtenKeys.Count;
        }

        [TearDown]
        public virtual void TearDown()
        {
            inputGenerator.ReleaseMovement();
            inputGenerator.ReleaseJump();
        }
    }
}
