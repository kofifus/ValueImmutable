# ValueImmutable
C# support for immutable objects with value semantics

## Introduction

A desirable 'functional' programming paradign (as opposed to OOP) is one in which logic is clearly separated from data and state. In such a paradigm, logic is stateless ('pure') functionality, data is represented by stateless objects that are immutable and have value semantics, and state is a collection of data with clearly defined access and mutation mechanisms.

C# started as OOP language where logic data and state are strongly coupled in classes. This makes coding in such a paradigm unnatural. Stateless logic can be cleanly expressed by static classes and static functions. However creating stateless data (immutable and with value semantics) is challenging as C# Objects are by default mutable (though the addition of read-only properties is a good step) and correctly implementing value semantics is not trivial . Immutable containers were recently added to .NET but they are cumersome to use and by default have reference semantics (for Equality etc). Encapsulating a state with it's access/mutation API is also difficult though recent language additions can give good solutions.

The purpose of the ValuieImmutable (or 'V') package is to greatly simplify the creation of data (immutable objects with value semantics), and to provide a mechanism for creating, accessing and mutating states.

## Definitions


