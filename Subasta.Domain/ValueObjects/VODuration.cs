
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VODuration
    {
        public int Value { get; private set; }

        public VODuration(int value)
        {
            if (value <= 0)
                throw new InvalidDurationException();

            Value = value;
        }

        public int ToInt() => Value;
    }
}
