
using Subasta.Domain.Enums;
using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VODeliverMethod
    {
        public string Value { get; private set; }

        public VODeliverMethod(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidDeliverMethodException();
            if (!Enum.IsDefined(typeof(DeliverMethodsEnum), value.ToLower()))
            {
                throw new InvalidDeliverMethodException();
            }

            Value = value;
        }

        public override string ToString() => Value;
    }
}
