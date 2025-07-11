using Subasta.Domain.Exceptions;

namespace Subasta.Domain.ValueObjects
{
    public class VOStartDate
    {
        public DateTime Value { get; private set; }

        public VOStartDate(DateTime value)
        {
            Value = value;
        }

        public string ToString() => Value.ToString();
    }
}
