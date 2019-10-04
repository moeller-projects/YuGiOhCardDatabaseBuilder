using System;
using System.Collections.Generic;
using System.Linq;
using YuGiOhDatabaseBuilderV2.Models;

namespace YuGiOhDatabaseBuilderV2.Reporter
{
    public class MissingFieldReporter : IObserver<MissingField>
    {
        private IDisposable _unsubscriber;
        private readonly IDictionary<string, string> _missingFields;
        private readonly object accessLock = new object();

        public MissingFieldReporter()
        {
            _missingFields = new Dictionary<string, string>();
        }

        public virtual void Subscribe(IObservable<MissingField> provider)
        {
            _unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            _unsubscriber.Dispose();
        }

        public void OnCompleted()
        {
            foreach (var missingField in _missingFields
                .Distinct()
                .OrderBy(o => o.Key))
            {
                Console.WriteLine($"MissingField {missingField.Key} at {missingField.Value}");
            }
            _missingFields.Clear();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(MissingField value)
        {
            if (string.IsNullOrEmpty(value.Name)) return;
            lock (accessLock)
            {
                if (!_missingFields.Contains(KeyValuePair.Create(value.Name, value.Location)) && !_missingFields.ContainsKey(value.Name))
                    _missingFields.Add(value.Name, value.Location);
            }
        }
    }
}
