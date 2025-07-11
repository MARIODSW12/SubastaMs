using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOClaimDate
    {
        public DateTime Value { get; private set; }

        public VOClaimDate(DateTime value)
        {
            Value = value;
        }

        public string ToString() => Value.ToString();
    }
}
