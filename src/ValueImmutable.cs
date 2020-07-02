using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Immutable.ValueImmutable {

  // Inheriting ValueImmutable decalres the class as immutable with value semantics
  // That is it once constructed: 
  // - all public fields always return the same values
  // - all public getters always return the same values
  // - Equals(o2) will return true iff:
  //   - this and o2 are of the exactly same type
  //   - all public fields of this and o2 always return equal values
  //   - all public getters of this and o2 always return equal values
  // - operator==(o1, o2) always returns object.Equals(o1, o2)
  // - operator!=(o1, o2) always returns !object.Equals(o1, o2)
  // - GetHashCode() always return the same value

  public abstract class ValueImmutable<TDerived> : IEquatable<TDerived> {
    // VEquals does the actual work of comparing by value 
    // when VEequals is called it is asserted that o is not null and 
    // this and o are of exactly the same type and have the same hashcode
    public abstract bool VEquals(object o);

    // VHashCode does the actual work calculating the 'value' hashcode
    // the result is cached for future calls
    public abstract int VHashCode();

    public bool Equals([AllowNull] TDerived o) => object.Equals(this, o);

    public override bool Equals(object? o) {
      return object.ReferenceEquals(this, o) || // same reference ->  equal
      o is object // this is not null but o is -> not equal
      && this.GetType() == o.GetType() // o is of different type (ie more derived) than this -> not equal
      && this.GetHashCode() == o.GetHashCode() // optimization, hash codes different -> not equal
      && this.VEquals(o);
    }

    public static bool operator ==(ValueImmutable<TDerived> o1, ValueImmutable<TDerived> o2) => object.Equals(o1, o2);
    public static bool operator !=(ValueImmutable<TDerived> o1, ValueImmutable<TDerived> o2) => !object.Equals(o1, o2);

    [JsonIgnore] [DebuggerBrowsable(DebuggerBrowsableState.Never)] protected int? valueImmutableCachedHashCode = null;
    public override int GetHashCode() {
      if (valueImmutableCachedHashCode is null) valueImmutableCachedHashCode = VHashCode();
      return valueImmutableCachedHashCode.Value;
    }
  }
}
