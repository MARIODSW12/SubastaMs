
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOReservePrice
    {
        public decimal Value { get; private set; }

        public VOReservePrice(decimal value)
        {
            if (value <= 0)
                throw new InvalidReservePriveException();

            Value = value;
        }

        public decimal ToDecimal() => Value;
    }
}
