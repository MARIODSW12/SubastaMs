using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOPrizeId
    {
        public string Value { get; private set; }

        public VOPrizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidPrizeIdException();

            if (!Guid.TryParse(value, out _))
                throw new InvalidPrizeIdException();

            Value = value;
        }

        //public static VOId Generate() => new VOId(Guid.NewGuid().ToString());

        public override string ToString() => Value;
    }
}
