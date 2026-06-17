using UnityEngine;
using NUnit.Framework;
using Tests.AplibTests;
using System.Collections;
using Aplib.Core;
using System.Linq;
using Metroidvania.Characters.Knight;

namespace Tests.Experiments
{
    public class RLExperiment : BaseExperiment
    {
        protected override AgentType AgentType => AgentType.RL;
        protected override CompletionStatus CompletionStatus => status;
        private CompletionStatus status;
        private GameObject target;
        private GameObject RLPlayer;
        private IanAgentRaysForTest agent;
        private KnightCharacterController character;

        protected override void Arrange()
        {
            base.Arrange();
            player.SetActive(false); // disable default player gameobject

            RLPlayer = GameObject.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ).FirstOrDefault(go => go.name == "Player_RL");
            if (RLPlayer == null)
                Assert.Fail("Player_RL GameObject not found in scene");

            target = GameObject.Find("Target");
            if (target == null)
                Assert.Fail("Target GameObject not found in scene");
            agent = RLPlayer.GetComponent<IanAgentRaysForTest>();
            if (agent == null)
                Assert.Fail("IanAgentRaysForTest component not found on Player_RL GameObject");
            character = RLPlayer.GetComponent<KnightCharacterController>();
            if (character == null)
                Assert.Fail("KnightCharacterController component not found on Player_RL GameObject");

            status = CompletionStatus.Unfinished;
            agent.SetGoal(target);
            RLPlayer.SetActive(true);
        }

        private bool hasReachedTarget()
        {
            return Vector3.Distance(RLPlayer.transform.position, target.transform.position) < 1f;
        }

        bool isAlive()
        {
            return character.lifeAttribute.currentValue > 0;
        }

        public override IEnumerator Act(int run)
        {
            while (!hasReachedTarget())
            {
                if (RLPlayer.transform.position.y < -10 || !isAlive())
                {
                    agent.enabled = false;
                    Debug.Log("Agent has died. Ending experiment.");
                    status = CompletionStatus.Failure;
                    yield break;
                }

                yield return null;
            }

            agent.enabled = false;
            Debug.Log("Agent has reached the target.");
            status = CompletionStatus.Success;
            yield break;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            agent.enabled = false;
        }
    }
}
