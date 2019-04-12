using System;
using System.Collections.Generic;
using System.Linq;
using YuGiOhDatabaseBuilderV2.Models;

namespace YuGiOhDatabaseBuilderV2.Reporter
{
    public class MissingFieldReporter : IObserver<MissingField>
    {
        private IDisposable unsubscriber;
        private IDictionary<string, string> missingFields;

        public MissingFieldReporter()
        {
            missingFields = new Dictionary<string, string>();
        }

        public virtual void Subscribe(IObservable<MissingField> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public void OnCompleted()
        {
            foreach (var missingField in missingFields
                .Distinct()
                .OrderBy(o => o.Key))
            {
                Console.WriteLine($"MissingField {missingField.Key} at {missingField.Value}");
            }
            missingFields.Clear();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(MissingField value)
        {
            if (string.IsNullOrEmpty(value.Name)) return;
            if (!missingFields.Contains(KeyValuePair.Create(value.Name, value.Location)) && !missingFields.ContainsKey(value.Name))
                missingFields.Add(value.Name, value.Location);
        }
    }
}
