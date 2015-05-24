using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MicroUpsert
{
    public sealed class BufferingWindowPipeline : Pipeline
    {
        private readonly Pipeline _nextPipeline;
        private readonly Queue<WorkingSet> _workingSets = new Queue<WorkingSet>();
        private WorkingSet _currentWorkingSet;

        public BufferingWindowPipeline(Pipeline nextPipeline)
        {
            if (nextPipeline == null)
            {
                throw new ArgumentNullException("nextPipeline");
            }

            _nextPipeline = nextPipeline;
        }

        public override void Upsert(KeyIdentity identity, UpsertCommand command)
        {
            EnsureWorkingSetEnrolled();
            _currentWorkingSet.EnlistUpsert(identity, command);
        }

        private void EnsureWorkingSetEnrolled()
        {
            if (_currentWorkingSet != null)
            {
                return;
            }

            _currentWorkingSet = new WorkingSet();
            _workingSets.Enqueue(_currentWorkingSet);
        }

        public override void Call(CallProcedure details)
        {
            EnsureWorkingSetEnrolled();
            _currentWorkingSet.ProcedureDetails = details;
            _currentWorkingSet = null;
        }

        public override void Go()
        {
            while (_workingSets.Count > 0)
            {
                var workingSet = _workingSets.Dequeue();

                workingSet.EnlistDetails(_nextPipeline);
                _nextPipeline.Go();
            }

            _currentWorkingSet = null;
        }

        public override void GoAndRead(Action<IDataReader> readCallback)
        {
            while (_workingSets.Count > 0)
            {
                var workingSet = _workingSets.Dequeue();

                workingSet.EnlistDetails(_nextPipeline);
                _nextPipeline.GoAndRead(readCallback);
            }

            _currentWorkingSet = null;
        }

        private class WorkingSet
        {
            private readonly Dictionary<KeyIdentity, UpsertCommand> _identityToUpsert
                = new Dictionary<KeyIdentity, UpsertCommand>();

            public CallProcedure ProcedureDetails { get; set; }

            public void EnlistUpsert(KeyIdentity identity, UpsertCommand command)
            {
                UpsertCommand preExistingCommand;
                if (!_identityToUpsert.TryGetValue(identity, out preExistingCommand))
                {
                    _identityToUpsert.Add(identity, command);
                    return;
                }

                var newVectors = command.Values.Union(preExistingCommand.Values);
                var newCommand = UpsertCommand.On(newVectors.ToArray());

                _identityToUpsert[identity] = newCommand;
            }

            public void EnlistDetails(Pipeline nextPipeline)
            {
                foreach (var upsert in _identityToUpsert)
                {
                    nextPipeline.Upsert(upsert.Key, upsert.Value);
                }

                if (ProcedureDetails != null)
                {
                    nextPipeline.Call(ProcedureDetails);
                }
            }
        }
    }
}
