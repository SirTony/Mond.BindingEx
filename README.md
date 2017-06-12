<h1 align="center">Mond.BindingEx</h1>

An alternative binding API for Mond that uses (read: abuses) reflection and LINQ to be as flexible as possible in order to allow the binding of any eligible .NET object to Mond without having to create wrappers for entire classes. Please note that because method, property, constructor, and operator calls are resolved via Reflection and argument matching *every time* they're invoked from Mond, this binding API is a potentially poor choice for applications where high performance is essential due to it's increased memory usage and execution time.

## Features

- [x] Class binding (*partial*)
  - [x] Instance member binding (*partial*)
    - [x] Methods
    - [x] Properties
    - [x] Constructors
    - [ ] Events
  - [x] Static member binding (*partial*)
    - [x] Methods
    - [x] Properties
    - [x] User defined operators
    - [ ] Events
    - [x] Nested objects
- [x] Enum binding
- [x] Delegate/function binding
- [x] Method parameter/return type conversion (*partial*)
  - [x] Automatic conversion of basic types (*`char`, `bool`, `string`, `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, and `double`*)
  - [x] Automatic conversion for delegates/functions
  - [x] Automatic conversion for classes, interfaces, and structs (*partial; only classes are converted*)
  - [x] Automatic conversion for enums
