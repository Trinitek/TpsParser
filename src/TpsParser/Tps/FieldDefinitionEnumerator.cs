using System;
using System.Collections;
using System.Collections.Generic;
using TpsParser.Tps.Record;

namespace TpsParser.Tps
{
    internal struct FieldDefinitionEnumerator : IEnumerator<IFieldDefinitionRecord>
    {
        private IReadOnlyList<IFieldDefinitionRecord> Records { get; }

        public FieldDefinitionEnumerator(IReadOnlyList<IFieldDefinitionRecord> records)
        {
            Records = records ?? throw new ArgumentNullException(nameof(records));

            _position = default;
            Current = default;
        }

        public IFieldDefinitionRecord Current { get; private set; }

        object IEnumerator.Current => Current;

        public int NextPosition
        {
            get => _position;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("The position must be zero or greater.", nameof(value));
                }
                else
                {
                    _position = value;
                }
            }
        }
        private int _position;

        public void Dispose()
        { }

        public bool MoveNext()
        {
            if (Records is null)
            {
                throw new InvalidOperationException("The backing collection is null.");
            }
            else if (NextPosition >= Records.Count)
            {
                return false;
            }

            Current = Records[NextPosition];
            NextPosition++;

            return true;
        }

        public void Reset() => throw new NotImplementedException();
    }
}
