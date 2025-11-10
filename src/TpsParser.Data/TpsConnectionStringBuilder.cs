using System;

namespace TpsParser.Data;

public sealed class TpsConnectionStringBuilder : System.Data.Common.DbConnectionStringBuilder
{

    public TpsConnectionStringBuilder()
    { }

    public TpsConnectionStringBuilder(string? ConnectionString)
    {
        this.ConnectionString = ConnectionString;
    }

    public TpsConnectionErrorHandling OnError
    {
        get
        {
            TryGetValue(nameof(OnError), out var tret);
            var ret = (tret as TpsConnectionErrorHandling?) ?? TpsConnectionErrorHandling.OnErrorResumeNext;
            return ret;
        }
        set
        {
            this[nameof(OnError)] = value;
        }
    }

    public string? Folder
    {
        get
        {
            TryGetValue(nameof(Folder), out var tret);
            var ret = tret as string;
            return ret;
        }
        set
        {
            this[nameof(Folder)] = value;
        }
    }

    public override object this[string keyword]
    {
        get
        {
            return base[keyword];
        }
        set
        {
            var NewValue = value;

            if(string.Equals(keyword, nameof(OnError), StringComparison.InvariantCultureIgnoreCase))
            {

                if (NewValue is string { } V1)
                {
                    if (Enum.TryParse<TpsConnectionErrorHandling>(V1, true, out var Value))
                    {
                        NewValue = Value;
                    }
                    else if (string.IsNullOrEmpty(V1))
                    {
                        NewValue = TpsConnectionErrorHandling.OnErrorResumeNext;
                    }
                    else
                    {
                        throw new InvalidOperationException($@"{NewValue} is not a valid {nameof(TpsConnectionErrorHandling)}");
                    }
                }
                else if(NewValue is TpsConnectionErrorHandling { } V2)
                {
                    NewValue = V2;
                }
                else
                {
                    throw new InvalidOperationException($@"{NewValue} is not a valid {nameof(TpsConnectionErrorHandling)}");
                }
            }

            base[keyword] = NewValue;
        }
    }
}

public enum TpsConnectionErrorHandling
{
    OnErrorResumeNext,
    OnErrorThrow,
}
