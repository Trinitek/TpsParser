using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace TpsParser.Tests;

internal sealed class TestFieldDefinitionEnumerable
{
    [Test]
    public void MergeGroupPointers_GroupsNone_TargetA0_MergeA_A0()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 4, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "A0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd1),
            []);

        FieldIteratorPointer groupToBeMerged = new(
            FieldDefinitionPointer.Create(fd0),
            [newPointer]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            groupToBeMerged,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(groupToBeMerged.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.Zero);
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsNone_TargetB0_MergeAB_B0()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 4, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 4, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "B0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            []);

        FieldIteratorPointer g1 = new(
            FieldDefinitionPointer.Create(fd1),
            [newPointer]);

        FieldIteratorPointer g0 = new(
            FieldDefinitionPointer.Create(fd0),
            [g1]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            g0,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(g0.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(g1.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.Zero);
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsA_TargetB0_MergeAB_B0()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 4, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 4, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "B0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [
            new(FieldDefinitionPointer.Create(fd0), [])
            ];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            []);

        FieldIteratorPointer g1 = new(
            FieldDefinitionPointer.Create(fd1),
            [newPointer]);

        FieldIteratorPointer g0 = new(
            FieldDefinitionPointer.Create(fd0),
            [g1]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            g0,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(g0.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(g1.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.Zero);
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsAB_TargetB0_MergeAB_B0()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 4, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 4, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "B0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [
            new(FieldDefinitionPointer.Create(fd0), [
                new(FieldDefinitionPointer.Create(fd1), [])])
            ];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            []);

        FieldIteratorPointer g1 = new(
            FieldDefinitionPointer.Create(fd1),
            [newPointer]);

        FieldIteratorPointer g0 = new(
            FieldDefinitionPointer.Create(fd0),
            [g1]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            g0,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(g0.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(g1.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.Zero);
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsAB_TargetC_MergeABC_C0C1()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "C", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd3 = new() { FullName = "C0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };
        FieldDefinition fd4 = new() { FullName = "C1", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [
            new(FieldDefinitionPointer.Create(fd0), [
                new(FieldDefinitionPointer.Create(fd1), [])])
            ];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            [
                new(FieldDefinitionPointer.Create(fd3), []),
                new(FieldDefinitionPointer.Create(fd4), []),
            ]);

        FieldIteratorPointer g2 = new(
            FieldDefinitionPointer.Create(fd2),
            [newPointer]);

        FieldIteratorPointer g1 = new(
            FieldDefinitionPointer.Create(fd1),
            [g2]);

        FieldIteratorPointer g0 = new(
            FieldDefinitionPointer.Create(fd0),
            [g1]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            g0,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(g0.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(g1.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(g2.DefinitionPointer));

            var ii3 = ii2.ChildIterators[0];

            Assert.That(ii3.ChildIterators, Has.Count.EqualTo(2));
            Assert.That(ii3.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
            Assert.That(ii3.ChildIterators.Select(ci => ci.DefinitionPointer), Is.EqualTo([
                FieldDefinitionPointer.Create(fd3),
                FieldDefinitionPointer.Create(fd4),
                ]));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsABC_TargetC_MergeABC_C0C1()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "C", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd3 = new() { FullName = "C0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };
        FieldDefinition fd4 = new() { FullName = "C1", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [
            new(FieldDefinitionPointer.Create(fd0), [
                new(FieldDefinitionPointer.Create(fd1), [
                    new(FieldDefinitionPointer.Create(fd2), [])])])
            ];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            [
                new(FieldDefinitionPointer.Create(fd3), []),
                new(FieldDefinitionPointer.Create(fd4), []),
            ]);

        FieldIteratorPointer gB = new(
            FieldDefinitionPointer.Create(fd1),
            [newPointer]);

        FieldIteratorPointer gA = new(
            FieldDefinitionPointer.Create(fd0),
            [gB]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            gA,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(gA.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(gB.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.EqualTo(2));
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
            Assert.That(ii2.ChildIterators.Select(ci => ci.DefinitionPointer), Is.EqualTo([
                FieldDefinitionPointer.Create(fd3),
                FieldDefinitionPointer.Create(fd4),
                ]));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsABC_C0_TargetC_MergeABC_C0C1()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "C", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd3 = new() { FullName = "C0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };
        FieldDefinition fd4 = new() { FullName = "C1", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [
            new(FieldDefinitionPointer.Create(fd0), [
                new(FieldDefinitionPointer.Create(fd1), [
                    new(FieldDefinitionPointer.Create(fd2), [
                        new(FieldDefinitionPointer.Create(fd3), [])])])])
            ];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            [
                new(FieldDefinitionPointer.Create(fd3), []),
                new(FieldDefinitionPointer.Create(fd4), []),
            ]);

        FieldIteratorPointer gB = new(
            FieldDefinitionPointer.Create(fd1),
            [newPointer]);

        FieldIteratorPointer gA = new(
            FieldDefinitionPointer.Create(fd0),
            [gB]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            gA,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(gA.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(gB.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.EqualTo(2));
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
            Assert.That(ii2.ChildIterators.Select(ci => ci.DefinitionPointer), Is.EqualTo([
                FieldDefinitionPointer.Create(fd3),
                FieldDefinitionPointer.Create(fd4),
                ]));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsABC_C1_TargetC_MergeABC_C0C1()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "C", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd3 = new() { FullName = "C0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };
        FieldDefinition fd4 = new() { FullName = "C1", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [
            new(FieldDefinitionPointer.Create(fd0), [
                new(FieldDefinitionPointer.Create(fd1), [
                    new(FieldDefinitionPointer.Create(fd2), [
                        new(FieldDefinitionPointer.Create(fd4), [])])])])
            ];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            [
                new(FieldDefinitionPointer.Create(fd3), []),
                new(FieldDefinitionPointer.Create(fd4), []),
            ]);

        FieldIteratorPointer gB = new(
            FieldDefinitionPointer.Create(fd1),
            [newPointer]);

        FieldIteratorPointer gA = new(
            FieldDefinitionPointer.Create(fd0),
            [gB]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            gA,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(gA.DefinitionPointer));

            var ii1 = ii0.ChildIterators[0];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(gB.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.EqualTo(2));
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
            Assert.That(ii2.ChildIterators.Select(ci => ci.DefinitionPointer), Is.EqualTo([
                FieldDefinitionPointer.Create(fd3),
                FieldDefinitionPointer.Create(fd4),
                ]));
        }
    }

    [Test]
    public void MergeGroupPointers_GroupsAXBC_TargetC_MergeABC_C0C1()
    {
        FieldDefinition fd0 = new() { FullName = "A", TypeCode = FieldTypeCode.Group, Length = 16, StringMask = string.Empty };
        FieldDefinition fdx = new() { FullName = "X", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fdx0 = new() { FullName = "X0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };
        FieldDefinition fdx1 = new() { FullName = "X1", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };
        FieldDefinition fd1 = new() { FullName = "B", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd2 = new() { FullName = "C", TypeCode = FieldTypeCode.Group, Length = 8, StringMask = string.Empty };
        FieldDefinition fd3 = new() { FullName = "C0", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };
        FieldDefinition fd4 = new() { FullName = "C1", TypeCode = FieldTypeCode.Long, Length = 4, StringMask = string.Empty };

        List<FieldIteratorPointer> iterators = [
            new(FieldDefinitionPointer.Create(fd0), [
                new(FieldDefinitionPointer.Create(fdx), [
                    new(FieldDefinitionPointer.Create(fdx0), []),
                    new(FieldDefinitionPointer.Create(fdx1), []),
                    ]),
                new(FieldDefinitionPointer.Create(fd1), [
                    new(FieldDefinitionPointer.Create(fd2), [
                        new(FieldDefinitionPointer.Create(fd4), [])])])])
            ];

        FieldIteratorPointer newPointer = new(
            FieldDefinitionPointer.Create(fd2),
            [
                new(FieldDefinitionPointer.Create(fd3), []),
                new(FieldDefinitionPointer.Create(fd4), []),
            ]);

        FieldIteratorPointer gB = new(
            FieldDefinitionPointer.Create(fd1),
            [newPointer]);

        FieldIteratorPointer gA = new(
            FieldDefinitionPointer.Create(fd0),
            [gB]);

        FieldDefinitionEnumerable.MergeGroupPointers(
            iterators,
            gA,
            newPointer);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(iterators, Has.Count.EqualTo(1));

            var ii0 = iterators[0];

            Assert.That(ii0.ChildIterators, Has.Count.EqualTo(2));
            Assert.That(ii0.DefinitionPointer, Is.EqualTo(gA.DefinitionPointer));

            var iix = ii0.ChildIterators[0];
            Assert.That(iix.ChildIterators, Has.Count.EqualTo(2));
            Assert.That(iix.DefinitionPointer, Is.EqualTo(FieldDefinitionPointer.Create(fdx)));
            Assert.That(iix.ChildIterators.Select(ci => ci.DefinitionPointer), Is.EqualTo([
                FieldDefinitionPointer.Create(fdx0),
                FieldDefinitionPointer.Create(fdx1),
                ]));

            var ii1 = ii0.ChildIterators[1];

            Assert.That(ii1.ChildIterators, Has.Count.EqualTo(1));
            Assert.That(ii1.DefinitionPointer, Is.EqualTo(gB.DefinitionPointer));

            var ii2 = ii1.ChildIterators[0];

            Assert.That(ii2.ChildIterators, Has.Count.EqualTo(2));
            Assert.That(ii2.DefinitionPointer, Is.EqualTo(newPointer.DefinitionPointer));
            Assert.That(ii2.ChildIterators.Select(ci => ci.DefinitionPointer), Is.EqualTo([
                FieldDefinitionPointer.Create(fd3),
                FieldDefinitionPointer.Create(fd4),
                ]));
        }
    }

    [Test]
    public void FieldIsInsideGroup_LongA_LongB_NotInsideOneAnother()
    {
        FieldDefinition fd0 = new() { Index = 0, FullName = "A", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };
        FieldDefinition fd1 = new() { Index = 1, FullName = "B", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 4, ElementCount = 1, StringMask = string.Empty };

        bool result1 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd1);
        bool result2 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd1, subject: fd0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.False);
            Assert.That(result2, Is.False);
        }
    }

    [Test]
    public void FieldIsInsideGroup_GroupIsNotInsideOfItself()
    {
        FieldDefinition fd0 = new() { Index = 0, FullName = "A", TypeCode = FieldTypeCode.Group, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };

        bool result1 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.False);
        }
    }

    [Test]
    public void FieldIsInsideGroup_LongADim2_LongB_NotInsideOneAnother()
    {
        FieldDefinition fd0 = new() { Index = 0, FullName = "A", TypeCode = FieldTypeCode.Long, Length = 8, Offset = 0, ElementCount = 2, StringMask = string.Empty };
        FieldDefinition fd1 = new() { Index = 1, FullName = "B", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 8, ElementCount = 1, StringMask = string.Empty };

        bool result1 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd1);
        bool result2 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd1, subject: fd0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.False);
            Assert.That(result2, Is.False);
        }
    }

    [Test]
    public void FieldIsInsideGroup_GroupA_LongA0_LongB_A0IsInsideA()
    {
        FieldDefinition fd0 = new() { Index = 0, FullName = "A", TypeCode = FieldTypeCode.Group, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };
        FieldDefinition fd1 = new() { Index = 1, FullName = "A0", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };
        FieldDefinition fd2 = new() { Index = 2, FullName = "B", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 4, ElementCount = 1, StringMask = string.Empty };

        bool result1 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd1);
        bool result2 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd1, subject: fd0);
        bool result3 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd2);
        bool result4 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd2, subject: fd0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(result3, Is.False);
            Assert.That(result4, Is.False);
        }
    }

    [Test]
    public void FieldIsInsideGroup_GroupADim2_LongA0_LongB_A0IsInsideA_BIsNotInsideA()
    {
        FieldDefinition fd0 = new() { Index = 0, FullName = "A", TypeCode = FieldTypeCode.Group, Length = 8, Offset = 0, ElementCount = 2, StringMask = string.Empty };
        FieldDefinition fd1 = new() { Index = 1, FullName = "A0", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };
        FieldDefinition fd2 = new() { Index = 2, FullName = "B", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 8, ElementCount = 1, StringMask = string.Empty };

        bool result1 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd1);
        bool result2 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd1, subject: fd0);
        bool result3 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd2);
        bool result4 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd2, subject: fd0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(result3, Is.False);
            Assert.That(result4, Is.False);
        }
    }

    [Test]
    public void FieldIsInsideGroup_GroupA_GroupA0_LongA00_A00IsInsideAAndA0()
    {
        FieldDefinition fd0 = new() { Index = 0, FullName = "A", TypeCode = FieldTypeCode.Group, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };
        FieldDefinition fd1 = new() { Index = 1, FullName = "A0", TypeCode = FieldTypeCode.Group, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };
        FieldDefinition fd2 = new() { Index = 2, FullName = "A00", TypeCode = FieldTypeCode.Long, Length = 4, Offset = 0, ElementCount = 1, StringMask = string.Empty };

        bool result1 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd0);
        bool result2 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd1);
        bool result3 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd0, subject: fd2);
        bool result4 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd1, subject: fd0);
        bool result5 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd1, subject: fd1);
        bool result6 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd1, subject: fd2);
        bool result7 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd2, subject: fd0);
        bool result8 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd2, subject: fd1);
        bool result9 = FieldDefinitionEnumerable.IsFieldInsideGroup(maybeGroup: fd2, subject: fd2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.False);
            Assert.That(result2, Is.True);
            Assert.That(result3, Is.True);
            Assert.That(result4, Is.False);
            Assert.That(result5, Is.False);
            Assert.That(result6, Is.True);
            Assert.That(result7, Is.False);
            Assert.That(result8, Is.False);
            Assert.That(result9, Is.False);
        }
    }
}
