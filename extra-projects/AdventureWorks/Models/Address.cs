using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AdventureWorks.Models;

/// <summary>
/// Street address information for customers, employees, and vendors.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class Address : IEquatable<Address>
{
    public bool Equals(Address? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return AddressId == other.AddressId
               && AddressLine1 == other.AddressLine1
               && AddressLine2 == other.AddressLine2
               && City == other.City
               && StateProvinceId == other.StateProvinceId
               && PostalCode == other.PostalCode
               && Rowguid.Equals(other.Rowguid)
               && ModifiedDate.Equals(other.ModifiedDate)
            && BusinessEntityAddresses.Equals(other.BusinessEntityAddresses)
            && SalesOrderHeaderBillToAddresses.Equals(other.SalesOrderHeaderBillToAddresses)
            && SalesOrderHeaderShipToAddresses.Equals(other.SalesOrderHeaderShipToAddresses)
            && StateProvince.Equals(other.StateProvince);
        ;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Address)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(AddressId);
        hashCode.Add(AddressLine1);
        hashCode.Add(AddressLine2);
        hashCode.Add(City);
        hashCode.Add(StateProvinceId);
        hashCode.Add(PostalCode);
        hashCode.Add(Rowguid);
        hashCode.Add(ModifiedDate);
        //hashCode.Add(BusinessEntityAddresses);
        //hashCode.Add(SalesOrderHeaderBillToAddresses);
        //hashCode.Add(SalesOrderHeaderShipToAddresses);
        //hashCode.Add(StateProvince);
        return hashCode.ToHashCode();
    }
}
