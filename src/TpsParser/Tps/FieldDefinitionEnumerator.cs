using System;
using System.Collections;
using System.Collections.Generic;
using TpsParser.Tps.Record;

namespace TpsParser.Tps
{
    internal class FieldDefinitionEnumerator : IEnumerator<IFieldDefinitionRecord>
    {
        private IReadOnlyList<IFieldDefinitionRecord> Records { get; }

        public FieldDefinitionEnumerator(IReadOnlyList<IFieldDefinitionRecord> records)
        {
            Records = records ?? throw new ArgumentNullException(nameof(records));
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
        private int _position = 0;

        public void Dispose()
        { }

        public bool MoveNext()
        {
            if (NextPosition >= Records.Count)
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
