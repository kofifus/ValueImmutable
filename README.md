# F
C# support for decoupling Data, state and Logic

## Introduction

A desirable 'functional' programming paradign (as opposed to OOP) is one in which there is clear separation between Data, State and Logic:
- Data represents 'values'. Data is immutable (cannot change once created) and has value semantics (for equality etc). Data may contain methods to return different representations of itself (ie the decimal or fration part of a real number), however its methods cannot change itself or interact with States or Logic.
- State represents 'memory'. It is made of Data with clearly defined mechanisms to access and mutatation. It does not mutate other States or use any Logic. (Note this is different from state/stateful/stateless with a lowercase 's' which is commonly used to mean 'has a value' etc)
- Logic represents 'behaviour'. Is is stateless ('pure') functionality that links input (from UI etc), Data and State(s) and is the only entity that can mutate the state(s).

OOP will have objects of type 'Dog' that know their name and address and can 'WalkHome()', while this design maybe useful for some scenarios it has major limitations in the more common kind of software which deals with information and UI. Archiving/journaling/reasoning about state changes is difficult, and refactoring/reusing logic and data is difficult as they are coupled together with the state. A 'functional' paradigm will have an immutable ''DogRecord'' (Data) having name and address, a separate ''DogsArchive'' (State) keeping the current dog records and a backlog (ie of address changes), and a 'DogController' (Logic) that can fetch a dog record from the database and change it's location on a map etc.<br>

A good summary of the bebefit of a such a 'functional' approach can be found here - https://clojure.org/about/state (in Clojure Data is called value, State is called Identity and Logic is called functionality). This is a vast topic but in short separating Data, State and Logic will give you programming superpowers. 

C# started as an OOP language where data state and logic are strongly coupled in classes. This makes coding in such a 'functional' paradigm challenging:
- Immutable data with value semantics is challenging to create as C# Objects are by default mutable (though the addition of read-only properties is a good step) and correctly implementing value semantics is not trivial . Immutable containers were recently added to .NET but they are cumbersome to use and have reference semantics. 
- Encapsulating a state with it's access/mutation API is also difficult though recent language additions can give good solutions.
- Stateless logic can be expressed by static classes and static functions

The purpose of the F package is to greatly simplify the creation of data (immutable objects with value semantics), and to provide a mechanism for creating, accessing and mutating states.

## Definitions - FData Object

An object that once constructed:

- All public fields and public properties always return the same values
- Equals(obj) will return true iff:
   - this and obj are of the exact same type (same GetType() result for both)
   - all public fields and public properties of this and obj return equal values respectively
- operators == and != return the same result as Equals and !Equals respectively
- GetHashCode() always return the same value

A ValueImmutable object can fields/properties which are privately mutable, this is commonly used for caching etc, in such cases care should be taken that the above conditions still hold or the results will be unexpected.

## Components

**FRecord** 

Allow easy creation of immutable data types with value semantics

**F containers (FList, FSet, FDict, FQueue, FArray)**

Value immutable versions of commonn containers with enhanced API

**FState**

Encapsulates FData object so that the _only_ way to modify it is through clearly defined access/mutation mechanism. Two concrete implementations of FState are provided - FLockedState which provides thread safety by locking on mutation, and FJournaledLockedState which also archive previous versions of the State.

**FWrapper**

Allow the easy creation of a new (FData) type which encapsulates another (FData) type. 

**FComposer**

Allow the easy creation of a new (FData) type which encapsulates another type which is not (FData) itself.  


## A Simple Example

**Data:**
```
public class Employee : FRecord<Employee> {
  public string Name { get; }
  public readonly int Age;
  public FSet<string> Phones { get; }
  public Employee(string name, int age, FSet<string> phones) => (this.Name, this.Age, this.Phones) = (name, age, phones);
}
```
`VSet` is an immutable hashset with value semantics and other additions. So all of `Employee`'s fields are immutable with ValueSemantics. 
By deriving from `VRecord`, `Employee` get three features:<br> 
First, value semantics itself, that is an `Equals` and `GetHashCode` that compare all it's fields.<br>
Second, a `With` method that allows easy creation of mutations (ie `emp2 = emp1.With(x => x.Name, "newname");`)<br>
Third, `FRecord` static constructor will in DEBUG mode verify that all fields/properties are publically readonly and will throw execptions otherwise


**State:**
```
public static class Store {
  public static readonly FLockedState<FDict<string, Employee>> Employees = FLockedState.Create(new FDict<string, Employee>());
}
```
Store holds the State of the program in this case. It is implemented as a static class with readonly `FState` fields<br>. 
`VDict` is an immutable dictionary with value semantics and other additions. 
`Store.Employees` is a mutable state that locks itself before allowing mutation so that the _only_ way to change it is threadsafe. It has three methods: `Ref` locks and mutate, `In` locks and allows readonly access, and 'Val' allows threadsafe readonly access of a possibly stale value. <br>
Using `Ref` and `In` hides locking and eliminate multithreading issues where locking was forgotten. Using 'Val' whereever stale values can be tolerated prevents unecessary locking while preserving thready safety.


**Logic:**
```
public static class EmployeeLogic {
	public static void AddEmployee(Employee employee) {
		Store.Employees.Ref((ref FDict<string, Employee> storeEmployees) => {
			storeEmployees += (employee.Name, employee);
		});
	}

	public static bool AddEmployeePhone(string name, string phone) {
		return Store.Employees.Ref((ref FDict<string, Employee> storeEmployees) => {
			bool ok;
			(ok, storeEmployees) = storeEmployees.With(name, x => x.Phones, phones => phones + phone);
			return ok;
		});
	}

	public static IEnumerable<string> GetEmployeePhones(string name) {
		var (ok, employee) = Store.Employees.Val[name];
		if (!ok) return Enumerable.Empty<string>();
		return employee.Phones;
	}
}
```
Logic is a collection of static (pure) methods.<br>
`AddEmployeePhone` uses `Ref` to acquire a reference access to mutate the employees dictionary and add/set an employee.
Using `Ref` is the _only_ way to change Store.Employee and becasue it is an `FLockedState` this operation is threadsafe (a lock is acquired internally).
Also note the use of += to add a (key, value) to the dictionary.

`AddEmployeePhone` similary uses a `Ref` to mutate `Store.Employees` in a threadsafe way. It uses `With` to calculate and return a mutation of storeEmployees with a mutated Phones property, and assign it to back to the State. 
Also note the way success is returned in `ok`. Using C# Nullable reference types (`#nullable enable`), gives a compiler warning if you try to access `storeEmployees` without checking that `ok` is true. F uses this pattern for all collections boundary checks and does not throw exceptions in these cases. 

`GetEmployeePhones` uses `Val` to get access to the current value of the `Store.Employees` and return the phones of a particular employee. No lock is taken in this case so the result may be stale which is fine in this case. However the call is still threadsafe as the returned value (being an FData) is immutable. This kind of threadsafe access to possibly stale values wherever possible can add great effiency.



