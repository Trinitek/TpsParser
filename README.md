# TpsParser

A library for parsing Clarion TopSpeed (TPS) files.

## Overview

This repository contains two projects: **TpsParser**, a low-level reader for TPS files, and **TpsParser.Data**, an ADO.NET adapter.

**TpsParser** is a C# rewrite of the Java library [tps-parse](https://github.com/ctrl-alt-dev/tps-parse). It is able to open and read both unencrypted and encrypted TPS files. It is not able to write to TPS files.

**TpsParser.Data** is a read-only ADO.NET adapter. It is designed for extract-transform-load scenarios where you need to read a folder full of TPS files and write them to another data store.

## Migrating from v5 to v6

Version 6 constitutes a major rewrite to dramatically reduce memory usage, improve read speed, and improve testability.

### The deserializer has been removed.
Version 5 included a deserializer that could map a data record to a user-defined class. The `TpsParser` class, `Deserialize` methods, and attributes have been removed. The deserializer was complicated to maintain through the rewrite. They may return in a future version under a different NuGet package; kindly file an issue if you need this.

### Class names and namespaces are changed.

The namespace hierarchy has been flattened into the `TpsParser` namespace. Data type model classes are located in the `TpsParser.TypeModel` namespace. Type models have adopted a `Cla*` prefix instead of `Tps*`. Type models are designed to better reflect behaviors of their respective type from the Clarion language runtime.

## TpsParser: Basic Usage Guidelines

### Simple Table Objects

For simple (but less performant) reading of tables, the `Table` class abstracts away the low-level file structures for easier manipulation.

```cs
using TpsParser;

using var fs = new FileStream("contacts.tps", FileMode.Open);
var tpsFile = new TpsFile(fs)

// Gets the first table in the file by default.
Table contactsTable = Table.MaterializeFromFile(tpsFile);

Row firstContactRow = contactsTable.Rows.First();

// Look up values by their column names.
IClaString firstName = (IClaString)firstContactRow.Values["FNAME"];
Console.WriteLine(firstName.ToString());

// MEMOs and BLOBs are in a separate dictionary.
if (firstContactRow.Memos.TryGetValue("NOTES", out IClaMemo? maybeNotes)
{
    Console.WriteLine(maybeNotes.ToString());
}
```

### Compound Data Structures

The parser can read `GROUP` and array structures. `GROUP`s can have nested `GROUP`s, arrayed fields, and arrays of `GROUP`s.

#### `GROUP` Structures

`GROUP` fields, modeled by the `ClaGroup` type, can contain one or more other value types, including other nested `GROUP`s.

```cs
ClaGroup address = (ClaGroup)firstContactRow.Values["ADDRESS"];

foreach (var value in address.GetValues())
{
    FieldDefinitionPointer field = value.Field;
    IClaObject value = value.Value;

    Console.WriteLine($"{field.Name} : {value.GetType().Name} : {value.ToString()}");
}

/*
 * Output:
 * -------
 * LINE1 : ClaString : Mulberry Lane
 * LINE2 : ClaString : Apt 2299
 * CITY : ClaString : Atlanta
 * STATE : ClaString : GA
 * POSTCODE : ClaString: 30303
 */
```

#### Array Structures

Array structures, modeled by the `ClaArray` type, are simply the same field type repeated _n_ number of times. All arrays are one-dimensioned.

```cs
ClaArray tagNumbers = (ClaArray)firstContactRow.Values["TAGS"];

Console.WriteLine($"TypeCode : {tagNumbers.TypeCode}");
Console.WriteLine($"Count : {tagNumbers.Count}");

for (int i = 0; i < tagNumbers.Count; i++)
{
    FieldEnumerationResult value = tagNumbers[i];

    FieldDefinitionPointer field = value.Field;
    ClaLong value = (ClaLong)value.Value;

    Console.WriteLine($"{i} : {value.ToString()}");
}

/*
 * Output:
 * -------
 * TypeCode : Long  <-- This is a Clarion LONG, equivalent to Int32
 * Count : 6
 * 0 : 36
 * 1 : 88
 * 2 : 294
 * 3 : 506
 * 4 : 311
 * 5 : 9286
 */
```

## TpsParser.Data: Basic Usage Guidelines

Many Clarion apps you encounter will structure their TPS file such that there are many files with one table in each. TpsParser.Data is oriented around using the folder as the Data Source.

```cs
using TpsParser.Data;

var csBuilder = new TpsConnectionStringBuilder
{
    Folder = "c:/path/to/folder
};

var conn = new TpsDbConnection(csBuilder.ConnectionString);

conn.Open();

var command = new TpsDbCommand()
{
    Connection = conn
    CommandText = "SELECT * FROM contacts"
};

var reader = command.ExecuteReader();
```

For files that contain more than one table, specify the table name in the SELECT statement using the `\!` separator as you would in the equivalent Clarion-language `FILE` declaration.

```cs
/*
 * Clarion snippet:
 * ----------------
 * FooContact   FILE,DRIVER('TOPSPEED'),NAME('Contacts\!Foo')
 */

CommandText = "SELECT * FROM contacts\!foo"
```

## Attributions

Copyright © 2025 Blake Burgess.  Licensed under MIT (see [LICENSE](LICENSE)).

Based on the previous work by Erik Hooijmeijer, [tps-parse](https://github.com/ctrl-alt-dev/tps-parse). Copyright © 2012-2013 Erik Hooijmeijer.  Licensed under [Apache 2](https://www.apache.org/licenses/LICENSE-2.0.html).
