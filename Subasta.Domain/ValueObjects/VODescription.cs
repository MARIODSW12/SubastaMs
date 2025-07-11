
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VODescription
    {
        public string Value { get; private set; }

        public VODescription(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidDescriptionException();

            Value = value;
        }

        public override string ToString() => Value;
    }
}
