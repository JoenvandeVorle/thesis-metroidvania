using System.Collections;
using System.Collections.Generic;
using System.IO;
using Aplib.Core;
using Aplib.Core.Agents;
using Aplib.Core.Belief.Beliefs;
using Aplib.Core.Belief.BeliefSets;
using Aplib.Core.Desire.DesireSets;
using Aplib.Core.Desire.Goals;
using Aplib.Core.Desire.GoalStructures;
using Aplib.Core.Intent.Actions;
using Aplib.Core.Intent.Tactics;
using Aplib.Integrations.Unity;
using NUnit.Framework;
using Tests.AplibTests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.Experiments
{
    public class ExperimentBeliefSet : BeliefSet
    {
        public readonly Belief<GameObject, GameObject> Player =
            new(reference: GameObject.Find("Player"), x => x);

        public readonly Belief<GameObject, GameObject> Target = new(
            GameObject.Find("Target"), x => x);

        public readonly Belief<TargetPathFinder, List<Vector2>> currentPath = new(
            GameObject.Find("Player").GetComponent<TargetPathFinder>(), x => x.GetCurrentPath());
    }

    public class BaseExperiment
    {
        public static int PATHFINDER_WAIT_TIMEOUT = 5;
        protected static readonly int[] _runs = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        private static Dictionary<string, int> _writtenKeys;
        private static string ResultKey(string scene, int run) => $"{scene}:{run}";

        protected InputGenerator inputGenerator;
        protected ExperimentBeliefSet beliefSet;
        protected GameObject player;
        protected JumpTacticForTest jumpTacticAgent;
        protected TargetPathFinder pathfinder;
        protected System.Tuple<int, Vector2> nextPathNode;

        protected void Arrange()
        {
            inputGenerator = InputGenerator.instance;
            beliefSet = new ExperimentBeliefSet();
            player = beliefSet.Player;

            jumpTacticAgent = player.GetComponent<JumpTacticForTest>();
            if (jumpTacticAgent == null)
                Assert.Fail("JumpTacticForTest component not found on Player GameObject");
            jumpTacticAgent.enabled = false;

            pathfinder = player.GetComponent<TargetPathFinder>();
            if (pathfinder == null)
                Assert.Fail("TargetPathFinder component not found on Player GameObject");

            if (pathfinder.GetCurrentPath() == null)
                Debug.Log("Remember to wait for the pathfinder first when running the test");
        }

        #region movement tactics

        protected Action<ExperimentBeliefSet> moveAction;
        protected Action<ExperimentBeliefSet> stopMovementAction;
        protected PrimitiveTactic<ExperimentBeliefSet> startMovementTactic;
        protected PrimitiveTactic<ExperimentBeliefSet> stopMovementTactic;
        protected FirstOfTactic<ExperimentBeliefSet> moveTactic;

        bool hasReachedJumpPoint(ExperimentBeliefSet beliefSet)
        {
            if (jumpTacticAgent.enabled)
                return true;

            GameObject player = beliefSet.Player;
            if (nextPathNode.Item1 == -1)
            {
                Debug.LogWarning("No next path index available in hasReachedJumpPoint guard.");
                return false;
            }
            if (nextPathNode.Item2.y > player.transform.position.y + 0.5f)
            {
                Debug.Log($"Reached jump point at i:{nextPathNode.Item1} - {nextPathNode.Item2}");
                return true;
            }
            return false;
        }

        bool hasPath(ExperimentBeliefSet beliefSet)
        {
            List<Vector2> path = beliefSet.currentPath;
            return path != null && path.Count > 0;
        }

        protected void InitializeMoveTactics()
        {
            moveAction = new(
                beliefSet =>
                {
                    nextPathNode = pathfinder.GetNextPathNode();
                    Vector3 direction = ((Vector3)nextPathNode.Item2 - player.transform.position).normalized;
                    inputGenerator.MoveTowards(direction);
                }
            );
            startMovementTactic = new(moveAction, hasPath);

            stopMovementAction = new(beliefSet => inputGenerator.ReleaseMovement());
            stopMovementTactic = new(stopMovementAction, hasReachedJumpPoint);
            moveTactic = new FirstOfTactic<ExperimentBeliefSet>(stopMovementTactic, startMovementTactic);
        }

        #endregion

        #region  jump tactic agent

        protected Action<ExperimentBeliefSet> jumpAction;
        protected Action<ExperimentBeliefSet> stopJumpAction;
        protected PrimitiveTactic<ExperimentBeliefSet> startJumpTactic;
        protected PrimitiveTactic<ExperimentBeliefSet> stopJumpTactic;
        protected FirstOfTactic<ExperimentBeliefSet> jumpTactic;

        bool hasReachedJumpTarget(ExperimentBeliefSet beliefSet)
        {
            if (jumpTacticAgent.ReachedGoal)
                Debug.Log($"Reached jump target: {jumpTacticAgent.goalPosition}");
            return jumpTacticAgent.ReachedGoal;
        }

        bool mayJump(ExperimentBeliefSet beliefSet) => hasReachedJumpPoint(beliefSet) && hasPath(beliefSet);

        protected void InitializeJumpTactics()
        {
            jumpAction = new(
                beliefSet =>
                {
                    if (jumpTacticAgent.enabled)
                        return;
                    Vector2 jumpEndPoint = pathfinder.FindEndOfJumpStartingAtIndex(nextPathNode.Item1 - 1);
                    Debug.Log($"Starting jump to: " + jumpEndPoint + $" from index {nextPathNode.Item1 - 1}");
                    jumpTacticAgent.StartAgent(jumpEndPoint);
                }
            );
            startJumpTactic = new(jumpAction, mayJump);

            stopJumpAction = new(
                beliefSet =>
                {
                    jumpTacticAgent.StopAgent();
                    nextPathNode = pathfinder.GetNextPathNode();
                }
            );
            stopJumpTactic = new(stopJumpAction, hasReachedJumpTarget);
            jumpTactic = new FirstOfTactic<ExperimentBeliefSet>(stopJumpTactic, startJumpTactic);
        }

        #endregion

        #region path correction

        protected Action<ExperimentBeliefSet> updatePathAction;
        protected PrimitiveTactic<ExperimentBeliefSet> updatePathTactic;
        bool isOffPath(ExperimentBeliefSet beliefSet)
        {
            GameObject player = beliefSet.Player;
            if (!hasPath(beliefSet))
                return true;
            System.Tuple<int, Vector2> closestNode = pathfinder.GetNextPathNode();
            if (Vector2.Distance(player.transform.position, closestNode.Item2) > 5.0f)
                return true;
            return false;
        }

        protected void InitializePathCorrection()
        {
            updatePathAction = new(
                beliefSet =>
                {
                    Debug.Log("Updating path due to deviation...");
                    pathfinder.UpdatePath();
                    nextPathNode = pathfinder.GetNextPathNode();
                }
            );
            updatePathTactic = new(updatePathAction, isOffPath);
        }

        #endregion

        #region experiment goal and agent

        protected FirstOfTactic<ExperimentBeliefSet> moveAndJumpTactic;
        protected Goal<ExperimentBeliefSet> reachedTargetGoal;
        protected PrimitiveGoalStructure<ExperimentBeliefSet> experimentGoalStructure;
        protected DesireSet<ExperimentBeliefSet> desireSet;
        protected BdiAgent<ExperimentBeliefSet> agent;
        protected AbortableAplibRunner runner;

        public void SetupExperiment()
        {
            Arrange();
            InitializeMoveTactics();
            InitializeJumpTactics();
            InitializePathCorrection();

            moveAndJumpTactic = new FirstOfTactic<ExperimentBeliefSet>(updatePathTactic, jumpTactic, moveTactic);

            bool didNotFall()
            {
                GameObject player = beliefSet.Player;
                return player.transform.position.y > -10;
            }

            reachedTargetGoal = new Goal<ExperimentBeliefSet>(
                moveAndJumpTactic,
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    GameObject target = beliefSet.Target;

                    if (!didNotFall())
                    {
                        runner.Abort();
                        return false;
                    }

                    return Vector2.Distance(player.transform.position, target.transform.position) < 0.5f;
                }
            );
            experimentGoalStructure = new PrimitiveGoalStructure<ExperimentBeliefSet>(reachedTargetGoal);
            desireSet = new DesireSet<ExperimentBeliefSet>(experimentGoalStructure);
            agent = new BdiAgent<ExperimentBeliefSet>(beliefSet, desireSet);
            runner = new AbortableAplibRunner(agent);
        }

        public IEnumerator PerformExperiment(int run = 1)
        {
            SetupExperiment();
            int pathfinderWaitTime = 0;
            while (pathfinder.GetCurrentPath() == null && pathfinderWaitTime < PATHFINDER_WAIT_TIMEOUT)
            {
                Debug.Log("Waiting for path to be calculated...");
                pathfinderWaitTime++;
                yield return TestWait.ForSeconds(1f);
            }
            if (pathfinder.GetCurrentPath() == null)
            {
                Debug.LogError("Pathfinder failed to calculate a path within the timeout period.");
                WriteResult(SceneManager.GetActiveScene().name, run, "PathfinderTimeout", 0);
                Assert.Fail("Pathfinder failed to calculate a path within the timeout period.");
                yield break;
            }
            nextPathNode = pathfinder.GetNextPathNode();

            // Act
            Assert.GreaterOrEqual(player.transform.position.y, -10, "Player should start above y = -10");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                yield return runner.Test();
            }
            finally
            {
                stopwatch.Stop();
                WriteResult(SceneManager.GetActiveScene().name, run, runner.Status.ToString(), stopwatch.Elapsed.TotalSeconds);
            }

            // Assert
            Assert.AreEqual(CompletionStatus.Success, runner.Status);
        }

        #endregion

        private static Dictionary<string, int> getWrittenKeys(string filePath)
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
                if (sceneEnd < 0 || runEnd < 0) continue;
                string s = line.Substring(sceneStart, sceneEnd - sceneStart);
                if (int.TryParse(line.Substring(runStart, runEnd - runStart), out int r))
                    _writtenKeys[ResultKey(s, r)] = i;
            }
            Debug.Log($"Written keys loaded: {string.Join(", ", _writtenKeys.Keys)}");
            return _writtenKeys;
        }

        private static void WriteResult(string scene, int run, string status, double durationSeconds)
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ExperimentResults"));
            Directory.CreateDirectory(dir);
            string duration = durationSeconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            string newLine = $"{{\"scene\":\"{scene}\",\"run\":{run},\"status\":\"{status}\",\"duration\":{duration}}}";
            string filePath = Path.Combine(dir, "results.jsonl");
            string key = ResultKey(scene, run);
            Dictionary<string, int> writtenKeys = getWrittenKeys(filePath);

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
    }
}
