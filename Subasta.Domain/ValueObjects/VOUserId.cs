using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOUserId
    {
        public string Value { get; private set; }

        public VOUserId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidUserIdException();

            if (!Guid.TryParse(value, out _))
                throw new InvalidUserIdException();

            Value = value;
        }

        //public static VOId Generate() => new VOId(Guid.NewGuid().ToString());

        public override string ToString() => Value;
    }
}
