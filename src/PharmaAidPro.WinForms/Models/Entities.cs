using System;

namespace PharmaAidPro.WinForms.Models
{
    public class StoreSettings
    {
        public int Id { get; set; }
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string DrugLicenseNo { get; set; }
        public string Gstin { get; set; }
        public string LicenseRef { get; set; }
        public string DefaultUser { get; set; }
        public string BillPrefix { get; set; }
        public int NextBillNumber { get; set; }
        public int NextGrnNumber { get; set; }
        public int NextProductCode { get; set; }
        public string PrinterName { get; set; }
        public string BackupDirectory { get; set; }
    }

    public class HsnTax
    {
        public string HsnCode { get; set; }
        public string Description { get; set; }
        public decimal TaxPercent { get; set; }
        public string TaxLabel { get; set; }
    }

    public class Supplier
    {
        public int SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Gstin { get; set; }
        public string StateName { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class Product
    {
        public int ProductCode { get; set; }
        public string ProductName { get; set; }
        public int PurchaseUnit { get; set; }
        public decimal PurchasePrice { get; set; }
        public int SaleUnit { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Mrp { get; set; }
        public string HsnCode { get; set; }
        public decimal TaxPercent { get; set; }
        public string Rack { get; set; }
        public string Salt { get; set; }
        public decimal TotalStockUnits { get; set; }
    }

    public class ProductBatch
    {
        public int BatchId { get; set; }
        public int ProductCode { get; set; }
        public string BatchDesc { get; set; }
        public string ExpiryMMYY { get; set; }
        public string ExpirySortKey { get; set; }
        public decimal PPrice { get; set; }
        public decimal SPrice { get; set; }
        public decimal Mrp { get; set; }
        public decimal StockBaseUnits { get; set; }
        public string Location { get; set; }
        public string LastUpdated { get; set; }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal KhataBalance { get; set; }
    }

    public class Doctor
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string RegNo { get; set; }
        public string Clinic { get; set; }
    }

    public class PurchaseHeader
    {
        public int GrnNumber { get; set; }
        public string GrnDate { get; set; }
        public int SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string PaymentDate { get; set; }
        public string Series { get; set; }
        public decimal AmountPayable { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal OtherCrDt { get; set; }
        public decimal CoinAdj { get; set; }
        public decimal GrossAmt { get; set; }
        public decimal SgstAmt { get; set; }
        public decimal CgstAmt { get; set; }
        public decimal NetAmt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class PurchaseLine
    {
        public int LineId { get; set; }
        public int GrnNumber { get; set; }
        public int Sno { get; set; }
        public int ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Packing { get; set; }
        public decimal QtyPacks { get; set; }
        public decimal FreeQtyPacks { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal ProdDisPercent { get; set; }
        public string BatchDesc { get; set; }
        public string ExpiryMMYY { get; set; }
        public string HsnCode { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal PPrice { get; set; }
        public decimal SPrice { get; set; }
        public decimal Mrp { get; set; }
        public decimal ItemCost { get; set; }
        public decimal MarginPercent { get; set; }
    }

    public class SalesHeader
    {
        public string BillNo { get; set; }
        public string BillDate { get; set; }
        public string CustomerName { get; set; }
        public string DoctorName { get; set; }
        public string PaymentMode { get; set; }
        public decimal BaseAmt { get; set; }
        public decimal SgstAmt { get; set; }
        public decimal CgstAmt { get; set; }
        public decimal TotalGstAmt { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalQty { get; set; }
        public string CreatedBy { get; set; }
    }

    public class SalesLine
    {
        public int LineId { get; set; }
        public string BillNo { get; set; }
        public int Sno { get; set; }
        public int ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal QtyUnits { get; set; }
        public string BatchDesc { get; set; }
        public string ExpiryMMYY { get; set; }
        public decimal RateMrp { get; set; }
        public decimal SaleUnit { get; set; }
        public decimal DiscPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal LineAmount { get; set; }
    }

    public class AppUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int IsActive { get; set; }
    }

    public class StockExpiryRow
    {
        public int ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BatchDesc { get; set; }
        public string ExpiryMMYY { get; set; }
        public string ExpirySortKey { get; set; }
        public decimal StockBaseUnits { get; set; }
        public int SaleUnit { get; set; }
        public decimal Mrp { get; set; }
        public decimal StockValue { get; set; }
    }
}
