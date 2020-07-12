# ValueImmutable
C# support for immutable objects with value semantics

## Introduction

A desirable 'functional' programming paradign (as opposed to OOP) is one in which there is clear separation between Data, State and Logic:
- Data represents 'information'. Data is either an instance of a basic type or a collection/composition of other Data. All Data is immutable (cannot change once created) and has value semantics (for equality etc). Data may contain methods to return different representations of itself (ie the decimal or fration part of a real number), however it's methods cannot change itself or interact with States or Logic.
- State represents 'memory'. It is made of Data with clearly defined mechanisms to access and mutate it. It does not mutate other States or initiate other Logics. 
- Logic represents 'operation'. Is is stateless ('pure') functionality that links input (from UI etc), Data and State(s) and is the only entity that can mutates the state(s).

OOP will have objects of type ''Dog'' that know their name and address and can 'WalkHome()', this kind of design has major limitations in the more common kind of software which deals with information and UI. A 'functional' paradigm will have an immutable ''DogRecord'' (Data) having name and address, a ''DogDatabase'' (State) keeping the current dog records plus an archive (ie of address changes), and a 'DogController' (Logic) that can fetch a dog record from the database and change it's location on a map etc.  

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

A ValueImmutable object can have mutable non public fields (usually used for caching) as long as the above conditions still hold.

## Components

**VRecord** 

Allow easy creation of immutable data types with value semantics

**V containers**

Value immutable versions of commonn containers (Array, List, HashSet, Dictionary etc) with enhanced API

**VState**

Encapsulate a ValueImmutable object so that the _only_ way to modify it is through clearly defined access/mutation mechanism. Two concrete implementations of VState are provided - VLockedState and VJournaledLockedState.

**VWrapper**

Allow the easy creation of a new (ValueImmutable) type which encapsulates another (ValueImmutable) type 

**VComposer**

Allow the easy creation of a new (ValueImmutable) type which encapsulates another type which is not ValueImmutable itself.  


## A Simple Example

**Data:**
```
class Employee : VRecord<Employee> {
  string Name { get; }
  VSet<string> Phones  { get; }
  public Employee(string name, VSet<string> phones) => (this.Name, this.Phones) = (name, phones);
}
```
`VSet` is an immutable hashset with value semantics and other additions. So all of `Employee`'s fields are immutable with ValueSemantics. 
By deriving from `VRecord`, `Employee` get two features:<br> 
First, value semantics itself that is an `Equals` and `GetHashCode` that compare all it's fields.<br>
Second, a `With` method that allows easy creation of mutations (ie `emp2 = emp1.With(x => x.Name, "newname");`)


**State:**
```
public static class Store {
   public static readonly VLockedState<VDict<string, Employee>> Employees =  VLockedState.Create(new VDict<string, Employee>());
}
```
Store holds the State of the program in this case. It is implemented as a static class with readonly `IState` fields<br>. 
`VDict` is an immutable dictionary with value semantics and other additions. 
`EmployeesStore` is a mutable state that locks itself before allowing mutation so that the _only_ way to change it is threadsafe. It has three methods: `Ref` locks and mutate, `In` locks and allows readonly access, and 'Val' allows threadsafe readonly access of a possibly stale value. <br>
Using `Ref` and `In` hides locking and eliminate multithreading issues when a lock was forgotten. Using 'Val' whereever stale values can be tolerated prevents unecessary locking while preserving thready safety.


**Logic:**
```
public static class EmployeeLogic {
  public static bool AddEmployeePhone(string name, string phone) {
    return Store.Employees.Ref((ref VDict<string, Employee> storeEmployees) => {
     var (ok, storeEmployees) = storeEmployees.With(name, x => x.Phones, phones => phones+phone);
     return ok;
    });
  }

  public static IEnumerable<string> GetAllEmployeeNames() => Employees.Val.Keys;
}  
```
Logic is a collection of static (pure) methods.<br>
`AddEmployeePhone` uses 
`AddEmployeePhone` uses a `Ref` to acquire a reference access to mutate the employees dictionary

