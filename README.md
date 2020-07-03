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

## Definitions

### ValueImmutable Object

An object that once constructed:

- all public fields always return the same values
- all public getters always return the same values
- GetHashCode() always return the same value
- Equals(o2) will return true iff:
   - this and o2 are of the exact same type (equal GetType() )
   - all public fields of this and o2 always return equal values
   - all public getters of this and o2 always return equal values
- operator==(o1, o2) always returns object.Equals(o1, o2)
- operator!=(o1, o2) always returns !object.Equals(o1, o2)




