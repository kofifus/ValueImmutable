# ValueImmutable
C# support for immutable objects with value semantics

## Introduction

A desirable 'functional' programming paradign (as opposed to OOP) is one in which there is clear separation between logic, data, and state:
- data is immutable with value semantics but no logic beyond returning different representations of itself
- state (made of data) has clearly defined mechanisms for access and mutation
- logic is stateless ('pure') functionality, the only entity that can mutates the state(s)

C# started as OOP language where logic data and state are strongly coupled in classes. This makes coding in such a paradigm unnatural:
- Stateless data (immutable and with value semantics) is challenging as C# Objects are by default mutable (though the addition of read-only properties is a good step) and correctly implementing value semantics is not trivial . Immutable containers were recently added to .NET but they are cumersome to use and by default have reference semantics (for Equality etc). 
- Encapsulating a state with it's access/mutation API is also difficult though recent language additions can give good solutions.
- Stateless logic however _can_ be cleanly expressed by static classes and static functions

The purpose of the ValueImmutable (or 'V') package is to greatly simplify the creation of data (immutable objects with value semantics), and to provide a mechanism for creating, accessing and mutating states.

## Definitions - ValueImmutable Object

An object that once constructed:

- all public fields and public properties always return the same values
- GetHashCode() always return the same value
- Equals(obj) will return true iff:
   - this and obj are of the exact same type (equal GetType() result for both)
   - all public fields and public properties of this and obj return equal values respectively
- operators == and != return the same result as Equals and !Equals respectively




