using System;
using System.Collections;
using System.Collections.Generic;
using TpsParser.Tps.Record;

namespace TpsParser.Tps;

internal sealed class FieldDefinitionEnumerator : IEnumerator<FieldDefinition>
{
    private IReadOnlyList<FieldDefinition> Records { get; }

    public FieldDefinitionEnumerator(IReadOnlyList<FieldDefinition> records)
    {
        Records = records ?? throw new ArgumentNullException(nameof(records));

        _position = -1;
    }

    public FieldDefinition Current => _current ?? throw new InvalidOperationException("Enumeration has either not started or has already completed.");
    private FieldDefinition? _current;

    object? IEnumerator.Current => Current;

    public int Position
    {
        get => _position;
        set
        {
            if (value < 0 || value >= Records.Count)
            {
                _current = null;
            }
            else if (_position != value)
            {
                _current = Records[value];
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
