using System;

namespace TpsParser.Tests.DeserializerModels
{
    public class MemosPrivateSettersModel_NoTrim : IMemoModel
    {
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string Name { get; private set; }

        [TpsField("Date")]
        public DateTime Date { get; private set; }

        [TpsField("Notes")]
        public string Notes { get; private set; }

        [TpsField("AdditionalNotes")]
        public string AdditionalNotes { get; private set; }
    }

    public class MemosInternalSettersModel_NoTrim : IMemoModel
    {
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string Name { get; internal set; }

        [TpsField("Date")]
        public DateTime Date { get; internal set; }

        [TpsField("Notes")]
        public string Notes { get; internal set; }

        [TpsField("AdditionalNotes")]
        public string AdditionalNotes { get; internal set; }
    }

    public class MemosProtectedSettersModel_NoTrim : IMemoModel
    {
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string Name { get; protected set; }

        [TpsField("Date")]
        public DateTime Date { get; protected set; }

        [TpsField("Notes")]
        public string Notes { get; protected set; }

        [TpsField("AdditionalNotes")]
        public string AdditionalNotes { get; protected set; }
    }

    public class MemosPublicSettersModel_NoTrim : IMemoModel
    {
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string Name { get; set; }

        [TpsField("Date")]
        public DateTime Date { get; set; }

        [TpsField("Notes")]
        public string Notes { get; set; }

        [TpsField("AdditionalNotes")]
        public string AdditionalNotes { get; set; }
    }

    public class MemosPrivateFieldsModel_NoTrim : IMemoModel
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Never assigned to
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        private string _name;

        [TpsField("Date")]
        private DateTime _date;

        [TpsField("Notes")]
        private string _notes;

        [TpsField("AdditionalNotes")]
        private string _additionalNotes;
#pragma warning restore CS0649 // Never assigned to
#pragma warning restore IDE0044 // Add readonly modifier

        public string Name => _name;
        public DateTime Date => _date;
        public string Notes => _notes;
        public string AdditionalNotes => _additionalNotes;
    }

    public class MemosInternalFieldsModel_NoTrim : IMemoModel
    {
#pragma warning disable CS0649 // Never assigned to
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        internal string _name;

        [TpsField("Date")]
        internal DateTime _date;

        [TpsField("Notes")]
        internal string _notes;

        [TpsField("AdditionalNotes")]
        internal string _additionalNotes;
#pragma warning restore CS0649 // Never assigned to

        public string Name => _name;
        public DateTime Date => _date;
        public string Notes => _notes;
        public string AdditionalNotes => _additionalNotes;
    }

    public class MemosProtectedFieldsModel_NoTrim : IMemoModel
    {
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        protected string _name;

        [TpsField("Date")]
        protected DateTime _date;

        [TpsField("Notes")]
        protected string _notes;

        [TpsField("AdditionalNotes")]
        protected string _additionalNotes;

        public string Name => _name;
        public DateTime Date => _date;
        public string Notes => _notes;
        public string AdditionalNotes => _additionalNotes;
    }

    public class MemosPublicFieldsModel_NoTrim : IMemoModel
    {
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string _name;

        [TpsField("Date")]
        public DateTime _date;

        [TpsField("Notes")]
        public string _notes;

        [TpsField("AdditionalNotes")]
        public string _additionalNotes;

        public string Name => _name;
        public DateTime Date => _date;
        public string Notes => _notes;
        public string AdditionalNotes => _additionalNotes;
    }

    public class MemosNotesRequiredModel_NoTrim : IMemoModel
    {
        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string Name { get; set; }

        [TpsField("Date")]
        public DateTime Date { get; set; }

        [TpsField("Notes", IsRequired = true)]
        public string Notes { get; set; }

        [TpsField("AdditionalNotes")]
        public string AdditionalNotes { get; set; }
    }

    public class MemosRecordNumberPropertyModel_NoTrim
    {
        [TpsRecordNumber]
        public int Id { get; private set; }

        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string Name { get; private set; }
    }

    public class MemosRecordNumberFieldModel_NoTrim
    {
        [TpsRecordNumber]
        public int _id;

        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string _name;
    }

    public class MemosRecordNumberAndFieldAttrOnSameMemberModel_NoTrim
    {
        [TpsRecordNumber]
        [TpsField("Dummy")]
        public int Id { get; private set; }

        [TpsField("Name")]
        [StringOptions(TrimEnd = false)]
        public string Name { get; private set; }
    }
}
