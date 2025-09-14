#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// State and province lookup table.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class StateProvince : IEquatable<StateProvince>
{
    public bool Equals(StateProvince? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return StateProvinceId == other.StateProvinceId && StateProvinceCode == other.StateProvinceCode && CountryRegionCode == other.CountryRegionCode && IsOnlyStateProvinceFlag == other.IsOnlyStateProvinceFlag && Name == other.Name && TerritoryId == other.TerritoryId && Rowguid.Equals(other.Rowguid) && ModifiedDate.Equals(other.ModifiedDate) && Addresses.Equals(other.Addresses) && CountryRegionCodeNavigation.Equals(other.CountryRegionCodeNavigation) && SalesTaxRates.Equals(other.SalesTaxRates) && Territory.Equals(other.Territory);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StateProvince)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(StateProvinceId);
        hashCode.Add(StateProvinceCode);
        hashCode.Add(CountryRegionCode);
        hashCode.Add(IsOnlyStateProvinceFlag);
        hashCode.Add(Name);
        hashCode.Add(TerritoryId);
        hashCode.Add(Rowguid);
        hashCode.Add(ModifiedDate);
        hashCode.Add(Addresses);
        hashCode.Add(CountryRegionCodeNavigation);
        hashCode.Add(SalesTaxRates);
        hashCode.Add(Territory);
        return hashCode.ToHashCode();
    }
}
