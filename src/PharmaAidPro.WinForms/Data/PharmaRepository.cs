using System.Collections.Generic;
using System.Linq;
using Dapper;
using PharmaAidPro.WinForms.Models;

namespace PharmaAidPro.WinForms.Data
{
    public class PharmaRepository
    {
        public StoreSettings GetStoreSettings()
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                return conn.QuerySingle<StoreSettings>("SELECT * FROM StoreSettings WHERE Id = 1");
            }
        }

        public int GetNextProductCode()
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                var nextFromSettings = conn.ExecuteScalar<int>("SELECT NextProductCode FROM StoreSettings WHERE Id = 1");
                var maxExisting = conn.ExecuteScalar<int>("SELECT COALESCE(MAX(ProductCode), 0) FROM Products");
                return nextFromSettings > maxExisting ? nextFromSettings : maxExisting + 1;
            }
        }

        public List<HsnTax> GetAllHsnTaxes()
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                return conn.Query<HsnTax>("SELECT * FROM HsnTaxMaster ORDER BY TaxPercent, HsnCode").ToList();
            }
        }

        public List<Product> SearchProducts(string query)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                var like = "%" + (query ?? string.Empty).Trim() + "%";
                return conn.Query<Product>(@"
                    SELECT p.*,
                           COALESCE((SELECT SUM(b.StockBaseUnits) FROM ProductBatches b WHERE b.ProductCode = p.ProductCode), 0) AS TotalStockUnits
                    FROM Products p
                    WHERE p.ProductName LIKE @Like OR CAST(p.ProductCode AS TEXT) LIKE @Like
                    ORDER BY p.ProductName
                    LIMIT 50", new { Like = like }).ToList();
            }
        }

        public List<Supplier> SearchSuppliers(string query)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                var like = "%" + (query ?? string.Empty).Trim() + "%";
                return conn.Query<Supplier>(@"
                    SELECT * FROM Suppliers
                    WHERE SupplierName LIKE @Like OR CAST(SupplierCode AS TEXT) LIKE @Like
                    ORDER BY SupplierName
                    LIMIT 50", new { Like = like }).ToList();
            }
        }

        public List<ProductBatch> GetBatchesForProduct(int productCode)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                return conn.Query<ProductBatch>(@"
                    SELECT * FROM ProductBatches
                    WHERE ProductCode = @ProductCode
                    ORDER BY ExpirySortKey ASC", new { ProductCode = productCode }).ToList();
            }
        }

        public void GetDatabaseCounts(out int products, out int batches, out int suppliers)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                products = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Products");
                batches = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM ProductBatches");
                suppliers = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Suppliers");
            }
        }
    }
}
