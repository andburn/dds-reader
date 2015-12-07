# DDS Reader
A DDS reader in C#, extracted and refactored from [igaeditor](https://github.com/micolous/igaeditor).
With updates found in this [gist](https://gist.github.com/soeminnminn/e9c4c99867743a717f5b), to cater for more formats.

----
### Original Readme

*au.id.micolous.libs.DDSReader*

This library is a DDS image file reader for .NET.  It is based on DevIL, ported to native C# code.

Portions of this code are based on il_dds.c from DevIL. il_dds.c is Copyright 2000-2002 Denton Woods, and licensed under the same terms as this library.

This library (DDSReader) is licensed under these terms:

    DDSReader
    Copyright (c) 2006 - 2007 Michael Farrell (micolous)
    All rights reserved.

        This library is free software; you can redistribute it and/or
        modify it under the terms of the GNU Lesser General Public
        License as published by the Free Software Foundation; either
        version 2 of the License, or (at your option) any later version.

        This library is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
        Lesser General Public License for more details.

        You should have received a copy of the GNU Lesser General Public
        License along with this library; if not, write to the Free Software
        Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  US

    A full copy of license is included in the file 'LICENSE'.


#### Compilation Notes

To speed things up, there is some unsafe code in this program.

If you don't want the *unsafe* routines, compile with the `/define:SAFE_MODE` option, and *safe* routines will be used instead.  

The "safe" routines are very very slow on Microsoft's .NET CLR on Win32,but are still pretty fast on Mono. You have been warned.
