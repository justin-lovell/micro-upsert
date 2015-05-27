using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MicroUpsert
{
    public sealed class BufferingWindowUpsertWriter : UpsertWriter
    {
        private readonly UpsertWriter _nextUpsertWriter;
        private readonly Queue<WorkingSet> _workingSets = new Queue<WorkingSet>();
        private WorkingSet _currentWorkingSet;

        public BufferingWindowUpsertWriter(UpsertWriter nextUpsertWriter)
        {
            if (nextUpsertWriter == null)
            {
                throw new ArgumentNullException("nextUpsertWriter");
            }

            _nextUpsertWriter = nextUpsertWriter;
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

                workingSet.EnlistDetails(_nextUpsertWriter);
                _nextUpsertWriter.Go();
            }

            _currentWorkingSet = null;
        }

        public override void GoAndRead(Action<IDataReader> readCallback)
        {
            while (_workingSets.Count > 0)
            {
                var workingSet = _workingSets.Dequeue();

                workingSet.EnlistDetails(_nextUpsertWriter);
                _nextUpsertWriter.GoAndRead(readCallback);
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

            public void EnlistDetails(UpsertWriter nextUpsertWriter)
            {
                foreach (var upsert in _identityToUpsert)
                {
                    nextUpsertWriter.Upsert(upsert.Key, upsert.Value);
                }

                if (ProcedureDetails != null)
                {
                    nextUpsertWriter.Call(ProcedureDetails);
                }
            }
        }
    }
}
