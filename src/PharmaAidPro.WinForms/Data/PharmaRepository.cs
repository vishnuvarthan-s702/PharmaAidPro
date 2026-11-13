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

        public List<Product> GetAllProducts(string filter)
        {
            return SearchProducts(filter);
        }

        public Product GetProductByCode(int productCode)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                return conn.QuerySingleOrDefault<Product>(@"
                    SELECT p.*,
                           COALESCE((SELECT SUM(b.StockBaseUnits) FROM ProductBatches b WHERE b.ProductCode = p.ProductCode), 0) AS TotalStockUnits
                    FROM Products p
                    WHERE p.ProductCode = @ProductCode", new { ProductCode = productCode });
            }
        }

        public int CreateProduct(Product product)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            using (var tx = conn.BeginTransaction())
            {
                if (product.ProductCode <= 0)
                {
                    var nextFromSettings = conn.ExecuteScalar<int>("SELECT NextProductCode FROM StoreSettings WHERE Id = 1", transaction: tx);
                    var maxExisting = conn.ExecuteScalar<int>("SELECT COALESCE(MAX(ProductCode), 0) FROM Products", transaction: tx);
                    product.ProductCode = nextFromSettings > maxExisting ? nextFromSettings : maxExisting + 1;
                }

                conn.Execute(@"
                    INSERT INTO Products (
                        ProductCode, ProductName, PurchaseUnit, PurchasePrice,
                        SaleUnit, SalePrice, Mrp, HsnCode, TaxPercent, Rack, Salt
                    ) VALUES (
                        @ProductCode, @ProductName, @PurchaseUnit, @PurchasePrice,
                        @SaleUnit, @SalePrice, @Mrp, @HsnCode, @TaxPercent, @Rack, @Salt
                    );", product, tx);

                conn.Execute(@"
                    UPDATE StoreSettings
                    SET NextProductCode = CASE WHEN NextProductCode <= @ProductCode THEN @ProductCode + 1 ELSE NextProductCode END
                    WHERE Id = 1;", new { product.ProductCode }, tx);

                tx.Commit();
                return product.ProductCode;
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                conn.Execute(@"
                    UPDATE Products SET
                        ProductName = @ProductName,
                        PurchaseUnit = @PurchaseUnit,
                        PurchasePrice = @PurchasePrice,
                        SaleUnit = @SaleUnit,
                        SalePrice = @SalePrice,
                        Mrp = @Mrp,
                        HsnCode = @HsnCode,
                        TaxPercent = @TaxPercent
                    WHERE ProductCode = @ProductCode;", product);
            }
        }

        public int GetNextSupplierCode()
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                return conn.ExecuteScalar<int>("SELECT COALESCE(MAX(SupplierCode), 0) + 1 FROM Suppliers");
            }
        }

        public void SaveSupplier(Supplier supplier, bool isNew)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                if (isNew)
                {
                    conn.Execute(@"
                        INSERT INTO Suppliers (SupplierCode, SupplierName, Address, Phone, Gstin, StateName, OutstandingAmount)
                        VALUES (@SupplierCode, @SupplierName, @Address, @Phone, @Gstin, @StateName, @OutstandingAmount);", supplier);
                }
                else
                {
                    conn.Execute(@"
                        UPDATE Suppliers SET
                            SupplierName = @SupplierName,
                            Address = @Address,
                            Phone = @Phone,
                            Gstin = @Gstin,
                            StateName = @StateName,
                            OutstandingAmount = @OutstandingAmount
                        WHERE SupplierCode = @SupplierCode;", supplier);
                }
            }
        }

        public int GetNextGrnNumber()
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            {
                var nextFromSettings = conn.ExecuteScalar<int>("SELECT NextGrnNumber FROM StoreSettings WHERE Id = 1");
                var maxExisting = conn.ExecuteScalar<int>("SELECT COALESCE(MAX(GrnNumber), 0) FROM PurchaseHeaders");
                return nextFromSettings > maxExisting ? nextFromSettings : maxExisting + 1;
            }
        }

        public static string ParseExpiryToSortKey(string mmyy)
        {
            if (string.IsNullOrWhiteSpace(mmyy)) return "2099-12";
            var parts = mmyy.Trim().Split('/');
            if (parts.Length == 2)
            {
                int mm, yy;
                if (int.TryParse(parts[0], out mm) && int.TryParse(parts[1], out yy))
                {
                    int yyyy = yy < 100 ? 2000 + yy : yy;
                    return string.Format("{0:0000}-{1:00}", yyyy, mm);
                }
            }
            return "2099-12";
        }

        public int SavePurchaseGrn(PurchaseHeader header, List<PurchaseLine> lines)
        {
            using (var conn = DatabaseInitializer.CreateConnection())
            using (var tx = conn.BeginTransaction())
            {
                var nextFromSettings = conn.ExecuteScalar<int>("SELECT NextGrnNumber FROM StoreSettings WHERE Id = 1", transaction: tx);
                var maxExisting = conn.ExecuteScalar<int>("SELECT COALESCE(MAX(GrnNumber), 0) FROM PurchaseHeaders", transaction: tx);
                int grnNo = nextFromSettings > maxExisting ? nextFromSettings : maxExisting + 1;
                header.GrnNumber = grnNo;

                conn.Execute(@"
                    INSERT INTO PurchaseHeaders (
                        GrnNumber, GrnDate, SupplierCode, SupplierName, InvoiceNo, InvoiceDate, PaymentDate,
                        Series, AmountPayable, DiscountPercent, DiscountAmt, OtherCrDt, CoinAdj,
                        GrossAmt, SgstAmt, CgstAmt, NetAmt, CreatedBy
                    ) VALUES (
                        @GrnNumber, @GrnDate, @SupplierCode, @SupplierName, @InvoiceNo, @InvoiceDate, @PaymentDate,
                        @Series, @AmountPayable, @DiscountPercent, @DiscountAmt, @OtherCrDt, @CoinAdj,
                        @GrossAmt, @SgstAmt, @CgstAmt, @NetAmt, @CreatedBy
                    );", header, tx);

                foreach (var line in lines)
                {
                    line.GrnNumber = grnNo;
                    conn.Execute(@"
                        INSERT INTO PurchaseLines (
                            GrnNumber, Sno, ProductCode, ProductName, Packing, QtyPacks, FreeQtyPacks,
                            DiscountAmt, ProdDisPercent, BatchDesc, ExpiryMMYY, HsnCode, TaxPercent,
                            PPrice, SPrice, Mrp, ItemCost, MarginPercent
                        ) VALUES (
                            @GrnNumber, @Sno, @ProductCode, @ProductName, @Packing, @QtyPacks, @FreeQtyPacks,
                            @DiscountAmt, @ProdDisPercent, @BatchDesc, @ExpiryMMYY, @HsnCode, @TaxPercent,
                            @PPrice, @SPrice, @Mrp, @ItemCost, @MarginPercent
                        );", line, tx);

                    var prod = conn.QuerySingle<Product>("SELECT * FROM Products WHERE ProductCode = @ProductCode", new { line.ProductCode }, tx);
                    decimal addedBaseUnits = (line.QtyPacks + line.FreeQtyPacks) * (prod.PurchaseUnit > 0 ? prod.PurchaseUnit : 1);
                    string sortKey = ParseExpiryToSortKey(line.ExpiryMMYY);
                    string today = System.DateTime.Now.ToString("yyyy-MM-dd");

                    var existingBatch = conn.QuerySingleOrDefault<ProductBatch>(@"
                        SELECT * FROM ProductBatches WHERE ProductCode = @ProductCode AND BatchDesc = @BatchDesc",
                        new { line.ProductCode, line.BatchDesc }, tx);

                    if (existingBatch != null)
                    {
                        conn.Execute(@"
                            UPDATE ProductBatches SET
                                ExpiryMMYY = @ExpiryMMYY,
                                ExpirySortKey = @ExpirySortKey,
                                PPrice = @PPrice,
                                SPrice = @SPrice,
                                Mrp = @Mrp,
                                StockBaseUnits = StockBaseUnits + @AddedUnits,
                                LastUpdated = @LastUpdated
                            WHERE BatchId = @BatchId;", new
                        {
                            line.ExpiryMMYY,
                            ExpirySortKey = sortKey,
                            line.PPrice,
                            line.SPrice,
                            line.Mrp,
                            AddedUnits = addedBaseUnits,
                            LastUpdated = today,
                            existingBatch.BatchId
                        }, tx);
                    }
                    else
                    {
                        conn.Execute(@"
                            INSERT INTO ProductBatches (
                                ProductCode, BatchDesc, ExpiryMMYY, ExpirySortKey,
                                PPrice, SPrice, Mrp, StockBaseUnits, Location, LastUpdated
                            ) VALUES (
                                @ProductCode, @BatchDesc, @ExpiryMMYY, @ExpirySortKey,
                                @PPrice, @SPrice, @Mrp, @StockBaseUnits, 'STORE', @LastUpdated
                            );", new
                        {
                            line.ProductCode,
                            line.BatchDesc,
                            line.ExpiryMMYY,
                            ExpirySortKey = sortKey,
                            line.PPrice,
                            line.SPrice,
                            line.Mrp,
                            StockBaseUnits = addedBaseUnits,
                            LastUpdated = today
                        }, tx);
                    }

                    conn.Execute(@"
                        UPDATE Products SET
                            PurchasePrice = @PPrice,
                            SalePrice = @SPrice,
                            Mrp = @Mrp,
                            HsnCode = CASE WHEN @HsnCode <> '' THEN @HsnCode ELSE HsnCode END,
                            TaxPercent = @TaxPercent
                        WHERE ProductCode = @ProductCode;", new
                    {
                        line.PPrice,
                        line.SPrice,
                        line.Mrp,
                        HsnCode = line.HsnCode ?? string.Empty,
                        line.TaxPercent,
                        line.ProductCode
                    }, tx);
                }

                if (header.Series == "INVOICE CREDIT")
                {
                    conn.Execute(@"
                        UPDATE Suppliers SET OutstandingAmount = OutstandingAmount + @NetAmt
                        WHERE SupplierCode = @SupplierCode;", new { header.NetAmt, header.SupplierCode }, tx);
                }

                conn.Execute("UPDATE StoreSettings SET NextGrnNumber = @NextGrn WHERE Id = 1;", new { NextGrn = grnNo + 1 }, tx);

                tx.Commit();
                return grnNo;
            }
        }
    }
}
