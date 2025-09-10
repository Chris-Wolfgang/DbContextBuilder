#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AdventureWorks.Models;

/// <summary>
/// Street address information for customers, employees, and vendors.
/// </summary>
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

    /// <summary>
	/// Primary key for Address records.
	/// </summary>
	public int AddressId { get; set; }

	/// <summary>
	/// First street address line.
	/// </summary>
	public string AddressLine1 { get; set; } = null!;

	/// <summary>
	/// Second street address line.
	/// </summary>
	public string? AddressLine2 { get; set; }

	/// <summary>
	/// Name of the city.
	/// </summary>
	public string City { get; set; } = null!;

	/// <summary>
	/// Unique identification number for the state or province. Foreign key to StateProvince table.
	/// </summary>
	public int StateProvinceId { get; set; }

	/// <summary>
	/// Postal code for the street address.
	/// </summary>
	public string PostalCode { get; set; } = null!;

	/// <summary>
	/// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
	/// </summary>
	public Guid Rowguid { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

    public virtual ICollection<BusinessEntityAddress> BusinessEntityAddresses { get; set; } = new List<BusinessEntityAddress>();

    public virtual ICollection<SalesOrderHeader> SalesOrderHeaderBillToAddresses { get; set; } = new List<SalesOrderHeader>();

    public virtual ICollection<SalesOrderHeader> SalesOrderHeaderShipToAddresses { get; set; } = new List<SalesOrderHeader>();

    public virtual StateProvince StateProvince { get; set; } = null!;
}
