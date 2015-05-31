<h1 align="center">Mond.BindingEx</h1>

An alternative binding API for Mond that uses (read: abuses) reflection and LINQ to be as flexible as possible in order to allow the binding of any eligible .NET to Mond without having to create wrappers for entire classes. Please note that because method, property, constructor, and operator calls are resolved via Reflection and argument matching *every time* they're invoked from Mond, this binding API is a potentially poor choice for applications where high performance is essential due to it's increased memory usage and execution time.

## Features

- [x] Class binding (*partial*)
  - [x] Method binding (*static and instance*)
  - [x] Property binding (*static and instance*)
  - [x] User defined operator binding (*static methods only*)
  - [x] Constructor binding
  - [ ] Event binding (*not yet implemented*)
  - [ ] Nested object binding (*not yet implemented*)
- [x] Delegate binding
- [x] Enum binding
- [x] Method parameter/return type conversion (*partial*)
  - [x] Automatic conversion of basic types (*`char`, `bool`, `string`, `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, and `double`*)
  - [ ] Automatic conversion for delegates/functions
  - [ ] Automatic conversion for classes, interfaces, and structs
  - [ ] Automatic conversion for enums
