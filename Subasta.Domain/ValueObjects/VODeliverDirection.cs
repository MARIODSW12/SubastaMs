
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VODeliverDirection
    {
        public string Value { get; private set; }

        public VODeliverDirection(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidDeliverDirectionException();

            Value = value;
        }

        public override string ToString() => Value;
    }
}
