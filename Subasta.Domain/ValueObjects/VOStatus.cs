
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOStatus
    {
        private readonly List<string> _validStatus =
            ["pending", "active", "ended",
            "canceled", "deserted", "completed",
            "delivered"];
        public string Value { get; private set; }

        public VOStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidStatusException();
            if (!_validStatus.Contains(value.ToLower()))
            {
                throw new InvalidStatusException();
            }

            Value = value;
        }

        public override string ToString() => Value;
    }
}
