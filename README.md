# ValueImmutable
C# support for immutable objects with value semantics

## Introduction

A desirable 'functional' programming paradign (as opposed to OOP) is one in which there is clear separation between Data, State and Logic:
- Data represents 'information'. Data is either an instance of a basic type or a collection/composition of other Data. All Data is immutable (cannot change once created) and has value semantics (for equality etc). Data may contain methods to return different representations of itself (ie the decimal or fration part of a real number), however it's methods cannot change itself or interact with States or Logic.
- State represents 'memory'. It is made of Data with clearly defined mechanisms to access and mutate it. It does not mutate other States or initiate other Logics. 
- Logic represents 'operation'. Is is stateless ('pure') functionality that links input (from UI etc), Data and State(s) and is the only entity that can mutates the state(s).

C# started as an OOP language where data state and logic are strongly coupled in classes. This makes coding in such a 'functional' paradigm challenging:
- Immutable data with value semantics is challenging to create as C# Objects are by default mutable (though the addition of read-only properties is a good step) and correctly implementing value semantics is not trivial . Immutable containers were recently added to .NET but they are cumersome to use and have reference semantics. 
- Encapsulating a state with it's access/mutation API is also difficult though recent language additions can give good solutions.
- Stateless logic can be expressed by static classes and static functions

The purpose of the ValueImmutable (or 'V') package is to greatly simplify the creation of data (immutable objects with value semantics), and to provide a mechanism for creating, accessing and mutating states.

## Definitions - ValueImmutable Object

An object that once constructed:

- All public fields and public properties always return the same values
- Equals(obj) will return true iff:
   - this and obj are of the exact same type (same GetType() result for both)
   - all public fields and public properties of this and obj return equal values respectively
- operators == and != return the same result as Equals and !Equals respectively
- GetHashCode() always return the same value

## Components

### VRecord 

Allow easy creation of immutable data types with value semantics

### V containers

Value immutable versions of commonn containers (Array, List, HashSet, Dictionary etc) with enhanced API

### VState

Encapsulate a ValueImmutable object so that the _only_ way to modify it is through clearly defined access/mutation mechanism. Two concrete implementations of VState are provided - VLockedState and VJournaledLockedState.

### VWrapper

Allow the easy creation of a new (ValueImmutable) type which encapsulates another (ValueImmutable) type 

### VComposer

Allow the easy creation of a new (ValueImmutable) type which encapsulates another type which is not ValueImmutable itself.  


## A Simple Example

Data:
```
class Employee : VRecord<Employee> {
  string Name { get; }
  VSet<string> Phones  { get; }
  public Employee(string name, VSet<string> phones) => (this.Name, this.Phones) = (name, phones);
}
```

State:
```
VLockedState<VDict<string, Employee>> Employees =  VLockedState.Create(new VDict<string, Employee>());
```

Logic:
```
bool AddEmployeePhone(string name, string phone) {
  return Employees.Ref((ref VDict<string, Employee> employees) => {
    var (ok, employee) = employees[name];
    if (!ok) return false;
    var newPhones = employee.Phones + phone;
    var newEmployee = employee.With(x => x.Phones, newPhones);
    employees = employees + (name, newEmployee);
    return true;
  });
}
```

The above can also be shortened to:
```
bool AddEmployeePhone(string name, string phone) {
  return Employees.Ref((ref VDict<string, Employee> employees) => {
   var (ok, employees) = employees.With(name, x => x.Phones, phones => phones+phone);
   return ok;
  });
}
```
