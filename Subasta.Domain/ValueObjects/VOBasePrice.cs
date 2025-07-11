using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOBasePrice
    {
        public decimal Value { get; private set; }

        public VOBasePrice(decimal value)
        {
            if (value <= 0)
                throw new InvalidBasePriceException();

            Value = value;
        }

        public decimal ToDecimal() => Value;
    }
}
