
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOMinimumIncrease
    {
        public decimal Value { get; private set; }

        public VOMinimumIncrease(decimal value)
        {
            if (value <= 0)
                throw new InvalidMinimumIncreaseException();

            Value = value;
        }

        public decimal ToDecimal() => Value;
    }
}
