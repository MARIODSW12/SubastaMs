using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOPrizeUserId
    {
        public string Value { get; private set; }

        public VOPrizeUserId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidPrizeUserIdException();

            if (!Guid.TryParse(value, out _))
                throw new InvalidPrizeUserIdException();

            Value = value;
        }

        //public static VOId Generate() => new VOId(Guid.NewGuid().ToString());

        public override string ToString() => Value;
    }
}
