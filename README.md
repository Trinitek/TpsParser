# TpsParser

A library for parsing Clarion TopSpeed (TPS) files.

Copyright © 2019 Blake Burgess.  Licensed under MIT (see [LICENSE](LICENSE)).

Based on the previous work by Erik Hooijmeijer, [tps-parse](https://github.com/ctrl-alt-dev/tps-parse). Copyright © 2012-2013 Erik Hooijmeijer.  Licensed under [Apache 2](https://www.apache.org/licenses/LICENSE-2.0.html).

## Overview

This library is a C# port of the Java library [tps-parse](https://github.com/ctrl-alt-dev/tps-parse).  It is able to open and read both unencrypted and encrypted TPS files.  It is not able to write to TPS files.

Unlike the original library, this version does not include a CSV exporter, and thus does not function as a standalone program.

Included in the port is a set of classes that is able to recover encrypted files where the password is not known.  Compute intensive portions of this section have been parallelized and make use of asynchronous Tasks.

Other miscellaneous performance improvmements have also been made.

## Clean Table Objects

The `Table` class abstracts away the low-level file structures for easier manipulation.

```cs
using (var parser = new TpsParser("contacts.tps"))
{
    // Currently only supports one table per file
    Table contactsTable = parser.BuildTable();

    Row firstContactRow = contactsTable.Rows.First();

    // Look up values by case-insensitive column names
    string firstName = firstContactRow.GetValueCaseInsensitive("fname");
    // Throws an exception if the field is not present in the row
    string lastName = firstContactRow.GetValueCaseInsensitive("lname", isRequired: true);
    // MEMO fields are mapped, too
    string notes = firstContactRow.GetValueCaseInsensitive("notes");
}
```

## Deserializer

This library includes a deserializer that allows you to read fields--including MEMOs and BLOBs--into POCO objects marked with the appropriate attributes.

```cs
class Contact
{
    // Use this to include the TPS record number.
    [TpsRecordNumber]
    public int Id { get; set; }

    // Special attribute for string properties. Trims ending whitespace by default.
    [TpsStringField("fname")]
    public string FirstName { get; set; }

    // Throw an exception if the field is not present in all rows.
    [TpsString("lname", IsRequired = true)]
    public string LastName { get; set; }

    // Column names are case-insensitive.
    [TpsStringField("addrStreet")]
    public string Street { get; set; }

    [TpsStringField("addrCity")]
    public string City { get; set; }

    // Field values can be automatically converted to booleans in accordance with Clarion expression rules.
    [TpsField("addrCity")]
    public bool HasCity { get; set; }

    // Fields, string fields especially, might encode booleans with specific values for true/false.
    [TpsBooleanField("validated", TrueValue = "Y", FalseValue = "N")]
    public bool IsValidated { get; set; }

    // Conversions from the internal LONG type to DATE/TIME are handled automatically when necessary.
    [TpsField("entry")]
    public DateTime EntryDate { get; set; }

    // TpsStringField lets you specify a StringFormat when reading from non-string fields -- in this case, a DATE.
    [TpsStringField("modified", StringFormat = "MM - dd - yyyy")]
    public string LastModified { get; set; }

    // Reads from MEMO fields too.
    [TpsField("notes")]
    public string Notes { get; set; }
}
```

```cs
using (var parser = new TpsParser("contacts.tps"))
{
    var contacts = parser.Deserialize<Contact>();
}
```
