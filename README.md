# TpsParser

A library for parsing Clarion TPS files.

Copyright 2019 Blake Burgess.  Licensed under MIT (see [LICENSE](LICENSE)).

Based on the previous work by Erik Hooijmeijer, [tps-parse](https://github.com/ctrl-alt-dev/tps-parse). Copyright 2012-2013 Erik Hooijmeijer.  Licensed under [Apache 2](https://www.apache.org/licenses/LICENSE-2.0.html).

## Overview

This library is a C# port of the Java library [tps-parse](https://github.com/ctrl-alt-dev/tps-parse).  It is able to open and read both unencrypted and encrypted TPS files.  It is not able to write to TPS files.

Unlike the original library, this version does not include a CSV exporter, and thus does not function as a standalone program.

Included in the port is a set of classes that is able to recover encrypted files where the password is not known.  Compute intensive portions of this section have been parallelized and make use of asynchronous Tasks.

Other miscellaneous performance improvmements have also been made.