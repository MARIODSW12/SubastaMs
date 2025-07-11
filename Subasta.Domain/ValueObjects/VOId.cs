using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOId
    {
        public string Value { get; private set; }

        public VOId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidIdException();

            if (!Guid.TryParse(value, out _))
                throw new InvalidIdException();

            Value = value;
        }

        //public static VOId Generate() => new VOId(Guid.NewGuid().ToString());

        public override string ToString() => Value;
    }
}
