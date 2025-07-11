
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOName
    {
        public string Value { get; private set; }

        public VOName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidNameException();

            Value = value;
        }

        public override string ToString() => Value;
    }
}
