
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOProductId
    {
        public string Value { get; private set; }

        public VOProductId(string value)
        {
            if (value == null)
                throw new InvalidAuctionProductIdException();

            Value = value;
        }

        public override string ToString() => Value;
    }
}
