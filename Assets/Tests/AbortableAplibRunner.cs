using Aplib.Core;
using System.Collections;
using Aplib.Core.Agents;

namespace Aplib.Integrations.Unity
{
    public class AbortableAplibRunner
    {
        public CompletionStatus Status => _isAborted ? CompletionStatus.Failure : _agent.Status;
        public string AbortReason { get; private set; }

        /// <summary>
        /// The agent that the test runner is testing.
        /// </summary>
        private readonly IAgent _agent;

        private bool _isAborted = false;

        public AbortableAplibRunner(IAgent agent)
        {
            _agent = agent;
        }

        /// <summary>
        /// Runs the test for the agent. The test continues until the agent's status is no longer Unfinished.
        /// </summary>
        /// <returns>An IEnumerator that can be used to control the execution of the test.</returns>
        public IEnumerator Test()
        {
            while (_agent.Status == CompletionStatus.Unfinished && !_isAborted)
            {
                // Perform computation or update the agent here
                _agent.Update();

                // Wait for the next frame
                yield return null;
            }
        }

        public void Abort(string reason)
        {
            _isAborted = true;
            AbortReason = reason;
        }
    }
}