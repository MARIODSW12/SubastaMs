
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOProductQuantity
    {
        public int Value { get; private set; }

        public VOProductQuantity(int value)
        {
            if (value <= 0)
                throw new InvalidAuctionProductQuantityException();

            Value = value;
        }

        public int ToInt() => Value;
    }
}
