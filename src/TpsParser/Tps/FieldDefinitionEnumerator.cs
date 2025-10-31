using System;
using System.Collections;
using System.Collections.Generic;
using TpsParser.Tps.Record;

namespace TpsParser.Tps
{
    internal sealed class FieldDefinitionEnumerator : IEnumerator<IFieldDefinitionRecord>
    {
        private IReadOnlyList<IFieldDefinitionRecord> Records { get; }

        public FieldDefinitionEnumerator(IReadOnlyList<IFieldDefinitionRecord> records)
        {
            Records = records ?? throw new ArgumentNullException(nameof(records));

            _position = -1;
        }

        public IFieldDefinitionRecord Current { get; private set; }

        object IEnumerator.Current => Current;

        public int Position
        {
            get => _position;
            set
            {
                if (value < 0 || value >= Records.Count)
                {
                    Current = null;
                }
                else if (_position != value)
                {
                    Current = Records[value];
                }

                _position = value;
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
            else if (Position >= Records.Count - 1)
            {
                return false;
            }

            Position++;

            return true;
        }

        public void Reset() => throw new NotImplementedException();
    }
}
