using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using Dapper;

namespace PharmaAidPro.WinForms.Data
{
    public static class DatabaseInitializer
    {
        public static string DataDirectory
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"); }
        }

        public static string DatabasePath
        {
            get { return Path.Combine(DataDirectory, "PharmaAidPro.db"); }
        }

        public static string ConnectionString
        {
            get { return string.Format("Data Source={0};Version=3;Pooling=True;Max Pool Size=100;", DatabasePath); }
        }

        public static SQLiteConnection CreateConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            conn.Execute("PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;");
            return conn;
        }

        public static void Initialize()
        {
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }

            using (var conn = CreateConnection())
            {
                CreateSchema(conn);
                SeedInitialData(conn);
            }
        }

        private static void CreateSchema(SQLiteConnection conn)
        {
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS StoreSettings (
                    Id INTEGER PRIMARY KEY CHECK (Id = 1),
                    StoreName TEXT NOT NULL,
                    Address TEXT NOT NULL,
                    DrugLicenseNo TEXT NOT NULL,
                    Gstin TEXT NOT NULL,
                    LicenseRef TEXT NOT NULL,
                    DefaultUser TEXT NOT NULL,
                    BillPrefix TEXT NOT NULL,
                    NextBillNumber INTEGER NOT NULL,
                    NextGrnNumber INTEGER NOT NULL,
                    NextProductCode INTEGER NOT NULL,
                    PrinterName TEXT,
                    BackupDirectory TEXT
                );

                CREATE TABLE IF NOT EXISTS HsnTaxMaster (
                    HsnCode TEXT PRIMARY KEY,
                    Description TEXT NOT NULL,
                    TaxPercent REAL NOT NULL,
                    TaxLabel TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Suppliers (
                    SupplierCode INTEGER PRIMARY KEY,
                    SupplierName TEXT NOT NULL,
                    Address TEXT,
                    Phone TEXT,
                    Gstin TEXT,
                    StateName TEXT DEFAULT 'TAMILNADU',
                    OutstandingAmount REAL NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Products (
                    ProductCode INTEGER PRIMARY KEY,
                    ProductName TEXT NOT NULL,
                    PurchaseUnit INTEGER NOT NULL DEFAULT 1,
                    PurchasePrice REAL NOT NULL DEFAULT 0,
                    SaleUnit INTEGER NOT NULL DEFAULT 1,
                    SalePrice REAL NOT NULL DEFAULT 0,
                    Mrp REAL NOT NULL DEFAULT 0,
                    HsnCode TEXT DEFAULT '0',
                    TaxPercent REAL NOT NULL DEFAULT 5,
                    Rack TEXT DEFAULT 'STORE',
                    Salt TEXT
                );

                CREATE TABLE IF NOT EXISTS ProductBatches (
                    BatchId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductCode INTEGER NOT NULL,
                    BatchDesc TEXT NOT NULL,
                    ExpiryMMYY TEXT NOT NULL,
                    ExpirySortKey TEXT NOT NULL,
                    PPrice REAL NOT NULL DEFAULT 0,
                    SPrice REAL NOT NULL DEFAULT 0,
                    Mrp REAL NOT NULL DEFAULT 0,
                    StockBaseUnits REAL NOT NULL DEFAULT 0,
                    Location TEXT DEFAULT 'STORE',
                    LastUpdated TEXT NOT NULL,
                    UNIQUE(ProductCode, BatchDesc),
                    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode)
                );

                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerName TEXT NOT NULL UNIQUE,
                    Phone TEXT,
                    Address TEXT,
                    KhataBalance REAL NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Doctors (
                    DoctorId INTEGER PRIMARY KEY AUTOINCREMENT,
                    DoctorName TEXT NOT NULL UNIQUE,
                    RegNo TEXT,
                    Clinic TEXT
                );

                CREATE TABLE IF NOT EXISTS PurchaseHeaders (
                    GrnNumber INTEGER PRIMARY KEY,
                    GrnDate TEXT NOT NULL,
                    SupplierCode INTEGER NOT NULL,
                    SupplierName TEXT NOT NULL,
                    InvoiceNo TEXT NOT NULL,
                    InvoiceDate TEXT NOT NULL,
                    PaymentDate TEXT NOT NULL,
                    Series TEXT NOT NULL DEFAULT 'INVOICE CREDIT',
                    AmountPayable REAL NOT NULL DEFAULT 0,
                    DiscountPercent REAL NOT NULL DEFAULT 0,
                    DiscountAmt REAL NOT NULL DEFAULT 0,
                    OtherCrDt REAL NOT NULL DEFAULT 0,
                    CoinAdj REAL NOT NULL DEFAULT 0,
                    GrossAmt REAL NOT NULL DEFAULT 0,
                    SgstAmt REAL NOT NULL DEFAULT 0,
                    CgstAmt REAL NOT NULL DEFAULT 0,
                    NetAmt REAL NOT NULL DEFAULT 0,
                    CreatedBy TEXT NOT NULL DEFAULT 'Admin',
                    FOREIGN KEY (SupplierCode) REFERENCES Suppliers(SupplierCode)
                );

                CREATE TABLE IF NOT EXISTS PurchaseLines (
                    LineId INTEGER PRIMARY KEY AUTOINCREMENT,
                    GrnNumber INTEGER NOT NULL,
                    Sno INTEGER NOT NULL,
                    ProductCode INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    Packing TEXT,
                    QtyPacks REAL NOT NULL,
                    FreeQtyPacks REAL NOT NULL DEFAULT 0,
                    DiscountAmt REAL NOT NULL DEFAULT 0,
                    ProdDisPercent REAL NOT NULL DEFAULT 0,
                    BatchDesc TEXT NOT NULL,
                    ExpiryMMYY TEXT NOT NULL,
                    HsnCode TEXT,
                    TaxPercent REAL NOT NULL DEFAULT 0,
                    PPrice REAL NOT NULL DEFAULT 0,
                    SPrice REAL NOT NULL DEFAULT 0,
                    Mrp REAL NOT NULL DEFAULT 0,
                    ItemCost REAL NOT NULL DEFAULT 0,
                    MarginPercent REAL NOT NULL DEFAULT 0,
                    FOREIGN KEY (GrnNumber) REFERENCES PurchaseHeaders(GrnNumber),
                    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode)
                );

                CREATE TABLE IF NOT EXISTS SalesHeaders (
                    BillNo TEXT PRIMARY KEY,
                    BillDate TEXT NOT NULL,
                    CustomerName TEXT,
                    DoctorName TEXT,
                    PaymentMode TEXT NOT NULL DEFAULT 'CASH',
                    BaseAmt REAL NOT NULL DEFAULT 0,
                    SgstAmt REAL NOT NULL DEFAULT 0,
                    CgstAmt REAL NOT NULL DEFAULT 0,
                    TotalGstAmt REAL NOT NULL DEFAULT 0,
                    RoundOff REAL NOT NULL DEFAULT 0,
                    NetAmount REAL NOT NULL DEFAULT 0,
                    TotalItems INTEGER NOT NULL DEFAULT 0,
                    TotalQty REAL NOT NULL DEFAULT 0,
                    CreatedBy TEXT NOT NULL DEFAULT 'Admin'
                );

                CREATE TABLE IF NOT EXISTS SalesLines (
                    LineId INTEGER PRIMARY KEY AUTOINCREMENT,
                    BillNo TEXT NOT NULL,
                    Sno INTEGER NOT NULL,
                    ProductCode INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    QtyUnits REAL NOT NULL,
                    BatchDesc TEXT NOT NULL,
                    ExpiryMMYY TEXT NOT NULL,
                    RateMrp REAL NOT NULL,
                    SaleUnit REAL NOT NULL,
                    DiscPercent REAL NOT NULL DEFAULT 0,
                    TaxPercent REAL NOT NULL DEFAULT 0,
                    LineAmount REAL NOT NULL,
                    FOREIGN KEY (BillNo) REFERENCES SalesHeaders(BillNo),
                    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode)
                );

                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'Admin',
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE INDEX IF NOT EXISTS IX_Products_Name ON Products(ProductName);
                CREATE INDEX IF NOT EXISTS IX_Batches_ProdExp ON ProductBatches(ProductCode, ExpirySortKey);
                CREATE INDEX IF NOT EXISTS IX_Sales_Date ON SalesHeaders(BillDate);
                CREATE INDEX IF NOT EXISTS IX_Purchase_Date ON PurchaseHeaders(GrnDate);
            ");
        }

        private static void SeedInitialData(SQLiteConnection conn)
        {
            var settingsCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM StoreSettings");
            if (settingsCount == 0)
            {
                conn.Execute(@"
                    INSERT INTO StoreSettings (
                        Id, StoreName, Address, DrugLicenseNo, Gstin, LicenseRef,
                        DefaultUser, BillPrefix, NextBillNumber, NextGrnNumber, NextProductCode,
                        PrinterName, BackupDirectory
                    ) VALUES (
                        1,
                        'The Nethaji Medical Stores',
                        'No.204/A,Meenakshipuram,Nagercoil-629001',
                        'TNK/351/20,21, TNK/417/20',
                        '33CREPS6130A1Z8',
                        'RETM33420',
                        'Admin',
                        'C01',
                        6810,
                        498,
                        39032,
                        '',
                        ''
                    );
                ");
            }
            else
            {
                conn.Execute("UPDATE StoreSettings SET StoreName = 'The Nethaji Medical Stores' WHERE StoreName LIKE '%NETAJI%';");
            }

            if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Users") == 0)
            {
                conn.Execute(@"
                    INSERT INTO Users (UserName, Password, Role, IsActive) VALUES
                    ('Admin', 'SHOPAID', 'Administrator', 1),
                    ('Counter1', '1234', 'Cashier', 1);
                ");
            }

            if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM HsnTaxMaster") == 0)
            {
                conn.Execute(@"
                    INSERT INTO HsnTaxMaster (HsnCode, Description, TaxPercent, TaxLabel) VALUES
                    ('0', 'Exempt / Zero Rated', 0, '0 %'),
                    ('30049061', 'Analgesics / Anti-inflammatory (5% GST)', 5, '5 % GST'),
                    ('30049029', 'Topical Creams & Antiseptics (5% GST)', 5, '5 % GST'),
                    ('30049072', 'Cardiovascular / Antihypertensive (5% GST)', 5, '5 % GST'),
                    ('30049099', 'Other Medicaments (5% GST)', 5, '5 % GST'),
                    ('30041010', 'Antibiotics - Penicillin / Cephalosporin (12% GST)', 12, '12 % GST'),
                    ('30042019', 'Other Antibiotics (12% GST)', 12, '12 % GST'),
                    ('21069099', 'Nutritional Supplements / Health Drinks (18% GST)', 18, '18 % GST'),
                    ('33049990', 'Cosmetics / Personal Care (28% GST)', 28, '28 % GST');
                ");
            }

            if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Suppliers") == 0)
            {
                conn.Execute(@"
                    INSERT INTO Suppliers (SupplierCode, SupplierName, Address, Phone, Gstin, StateName, OutstandingAmount) VALUES
                    (99, 'SUN AGENCIES', 'RAJ RANI COMPLEX, 121, BALAMORE ROAD, NAGERCOIL-629001', '04652-224777', '33AATPF5655P1Z0', 'TAMILNADU', 132050.00),
                    (32, 'S.A.V. TRADING CORPORATION', '148, SOUTH CAR STREET, NAGERCOIL', '04652-227904', '33AACFS1234A1Z5', 'TAMILNADU', 0.00),
                    (2,  'SUNDAR PHARMA', 'NAGERCOIL', '', '33AABPS4321B1Z2', 'TAMILNADU', 0.00),
                    (4,  'S.D. HEALTH CARE', 'NAGERCOIL', '', '', 'TAMILNADU', 0.00),
                    (5,  'S.P. PHARMACEUTICALS', 'NAGERCOIL', '', '', 'TAMILNADU', 0.00);
                ");
            }

            if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Products") == 0)
            {
                conn.Execute(@"
                    INSERT INTO Products (ProductCode, ProductName, PurchaseUnit, PurchasePrice, SaleUnit, SalePrice, Mrp, HsnCode, TaxPercent, Rack, Salt) VALUES
                    (22390, 'PHLOGAM', 10, 221.71, 10, 291.00, 291.00, '30049061', 5, 'STORE', 'Trypsin + Bromelain + Rutoside'),
                    (38803, 'SKINLITE CREAM 25GM', 1, 223.93, 1, 293.91, 293.91, '30049029', 5, 'STORE', 'Hydroquinone + Mometasone + Tretinoin'),
                    (15998, 'AMLODAC 5MG', 30, 61.35, 30, 80.52, 80.52, '30049072', 5, 'STORE', 'Amlodipine 5mg'),
                    (17205, 'CILACOR-T-TAB', 10, 222.72, 10, 292.31, 292.31, '30049099', 5, 'STORE', 'Cilnidipine + Telmisartan'),
                    (5335,  'SILVEREX 20GM CREAM', 1, 129.29, 1, 169.69, 169.69, '30049029', 5, 'STORE', 'Silver Sulfadiazine + Chlorhexidine'),
                    (39020, 'ZOCEF 500mg TABS', 10, 410.00, 10, 540.00, 540.00, '30041010', 5, 'A-01', 'Cefuroxime Axetil 500mg'),
                    (39021, 'ENSURE VANILA 400GM REFILL', 1, 590.00, 1, 745.00, 745.00, '21069099', 18, 'B-02', 'Complete Balanced Nutrition'),
                    (39022, 'CHYMORAL FORTE', 20, 395.00, 20, 514.12, 514.12, '30049061', 5, 'A-04', 'Trypsin Chymotrypsin'),
                    (39023, 'T BACT 15GM', 1, 250.00, 1, 325.08, 325.08, '30049029', 5, 'C-01', 'Mupirocin 2% Ointment'),
                    (39024, 'JALRA M 50/500MG 15''S', 15, 245.00, 15, 316.86, 316.86, '30049099', 5, 'D-01', 'Vildagliptin + Metformin'),
                    (39025, 'VOLINI SPRAY 60GM', 1, 180.00, 1, 233.44, 233.44, '30049061', 5, 'C-03', 'Diclofenac Spray'),
                    (39026, 'TESS OINTMENT', 1, 118.00, 1, 152.81, 152.81, '30049029', 5, 'C-02', 'Triamcinolone Acetonide'),
                    (39027, 'PROLUTON DEPOT 250MG', 1, 220.00, 1, 287.00, 287.00, '30049099', 5, 'E-01', 'Hydroxyprogesterone Caproate');

                INSERT INTO ProductBatches (ProductCode, BatchDesc, ExpiryMMYY, ExpirySortKey, PPrice, SPrice, Mrp, StockBaseUnits, Location, LastUpdated) VALUES
                    (22390, 'CGD0023',  '12/28', '2028-12', 221.71, 291.00, 291.00, 50, 'STORE', '2026-11-03'),
                    (38803, 'IB00079A', '12/27', '2027-12', 223.93, 293.91, 293.91, 10, 'STORE', '2026-11-03'),
                    (15998, 'IB01638A', '05/28', '2028-05', 61.35,  80.52,  80.52,  60, 'STORE', '2026-11-03'),
                    (17205, 'A4T26017', '06/28', '2028-06', 222.72, 292.31, 292.31, 20, 'STORE', '2026-11-03'),
                    (5335,  'RDD0071',  '03/28', '2028-03', 129.29, 169.69, 169.69, 12, 'STORE', '2026-11-03'),
                    (39020, '25460251', '01/27', '2027-01', 410.00, 540.00, 540.00, 100,'STORE', '2026-10-23'),
                    (39021, '037MN1',   '01/28', '2028-01', 590.00, 745.00, 745.00, 8,  'STORE', '2026-10-23'),
                    (39022, '2KU6N01',  '03/29', '2029-03', 395.00, 514.12, 514.12, 80, 'STORE', '2026-10-23'),
                    (39023, 'UQ2E',     '02/27', '2027-02', 250.00, 325.08, 325.08, 15, 'STORE', '2026-10-23'),
                    (39024, '01004A',   '04/28', '2028-04', 245.00, 316.86, 316.86, 45, 'STORE', '2026-10-23'),
                    (39025, 'XSC0011',  '01/27', '2027-01', 180.00, 233.44, 233.44, 12, 'STORE', '2026-10-23'),
                    (39026, 'AH6087',   '02/28', '2028-02', 118.00, 152.81, 152.81, 10, 'STORE', '2026-10-23'),
                    (39027, 'OR0596B',  '03/28', '2028-03', 220.00, 287.00, 287.00, 20, 'STORE', '2026-10-24');
                ");
            }

            LoadBundledCatalogIfNeeded(conn);

            // Auto-sync live stock from SQL Server only when running directly on the shop machine COMPAQ-PC
            if (string.Equals(Environment.MachineName, "COMPAQ-PC", StringComparison.OrdinalIgnoreCase) &&
                conn.ExecuteScalar<int>("SELECT COUNT(*) FROM ProductBatches") <= 20)
            {
                int pSync, bSync, sSync;
                string msg;
                TrySyncFromLocalShopaidSqlServer(conn, "COMPAQ-PC", out pSync, out bSync, out sSync, out msg);
            }
        }

        private static string ResolveCatalogPakPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = new[]
            {
                Path.Combine(Path.Combine(baseDir, "Assets"), "catalog.pak"),
                Path.Combine(baseDir, "catalog.pak"),
                Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Assets\catalog.pak"))
            };
            foreach (string c in candidates)
            {
                if (File.Exists(c)) return c;
            }
            return null;
        }

        public static int LoadBundledCatalogIfNeeded(SQLiteConnection conn)
        {
            int currentCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Products");
            if (currentCount >= 1000) return 0;

            string pakPath = ResolveCatalogPakPath();
            if (pakPath == null || !File.Exists(pakPath)) return 0;

            int inserted = 0;
            using (var tx = conn.BeginTransaction())
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO Products
                    (ProductCode, ProductName, PurchaseUnit, PurchasePrice, SaleUnit, SalePrice, Mrp, HsnCode, TaxPercent, Rack, Salt)
                    VALUES (@Code, @Name, @PurUnit, @PurPrice, @SaleUnit, @SalePrice, @Mrp, '30049099', 5, 'STORE', '');";

                var pCode = cmd.Parameters.Add("@Code", DbType.Int32);
                var pName = cmd.Parameters.Add("@Name", DbType.String);
                var pPurUnit = cmd.Parameters.Add("@PurUnit", DbType.Int32);
                var pPurPrice = cmd.Parameters.Add("@PurPrice", DbType.Double);
                var pSaleUnit = cmd.Parameters.Add("@SaleUnit", DbType.Int32);
                var pSalePrice = cmd.Parameters.Add("@SalePrice", DbType.Double);
                var pMrp = cmd.Parameters.Add("@Mrp", DbType.Double);

                using (var fs = File.OpenRead(pakPath))
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                using (var br = new BinaryReader(gz))
                {
                    int total = br.ReadInt32();
                    for (int i = 0; i < total; i++)
                    {
                        pCode.Value = br.ReadInt32();
                        pName.Value = br.ReadString();
                        pPurUnit.Value = br.ReadInt32();
                        pSaleUnit.Value = br.ReadInt32();
                        pPurPrice.Value = br.ReadDouble();
                        pSalePrice.Value = br.ReadDouble();
                        pMrp.Value = br.ReadDouble();
                        inserted += cmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
            return inserted;
        }

        public static bool TrySyncFromLocalShopaidSqlServer(
            SQLiteConnection sqliteConn,
            string customServer,
            out int productsSynced,
            out int batchesSynced,
            out int suppliersSynced,
            out string statusMessage)
        {
            productsSynced = 0;
            batchesSynced = 0;
            suppliersSynced = 0;
            statusMessage = "";

            var servers = new List<string>();
            if (customServer != null && customServer.Trim().Length > 0)
            {
                servers.Add(customServer.Trim());
            }
            string machine = Environment.MachineName;
            if (!servers.Contains(machine)) servers.Add(machine);
            if (!servers.Contains("COMPAQ-PC")) servers.Add("COMPAQ-PC");
            if (!servers.Contains("(local)")) servers.Add("(local)");
            if (!servers.Contains(".")) servers.Add(".");
            if (!servers.Contains(@".\SQLEXPRESS")) servers.Add(@".\SQLEXPRESS");
            if (!servers.Contains(machine + @"\SQLEXPRESS")) servers.Add(machine + @"\SQLEXPRESS");

            SqlConnection sqlConn = null;
            string connectedServer = null;

            foreach (string srv in servers)
            {
                string[] connStrings = new[]
                {
                    string.Format("Data Source={0};Initial Catalog=shopaid;User ID=wonder;Password=wrcss;Connect Timeout=2;", srv),
                    string.Format("Data Source={0};Initial Catalog=shopaid;Integrated Security=True;Connect Timeout=2;", srv)
                };

                foreach (string cs in connStrings)
                {
                    try
                    {
                        var candidate = new SqlConnection(cs);
                        candidate.Open();
                        sqlConn = candidate;
                        connectedServer = srv;
                        break;
                    }
                    catch
                    {
                    }
                }
                if (sqlConn != null) break;
            }

            if (sqlConn == null)
            {
                statusMessage = "Could not connect to local SQL Server database 'shopaid'. (Built-in 31,857 medicine catalog is active.)";
                return false;
            }

            try
            {
                using (sqlConn)
                using (var tx = sqliteConn.BeginTransaction())
                {
                    // 1. Sync Products
                    using (var sqlCmd = new SqlCommand("SELECT ProductCode, ProductName, ISNULL(PurchaseUnit,1) AS PurUnit, ISNULL(PurchasePrice,0) AS PurPrice, ISNULL(SaleUnit,1) AS SUnit, ISNULL(SalePrice,0) AS SPrice, ISNULL(MRP,0) AS Mrp FROM Products WITH (NOLOCK)", sqlConn))
                    using (var rdr = sqlCmd.ExecuteReader())
                    using (var upProd = sqliteConn.CreateCommand())
                    {
                        upProd.Transaction = tx;
                        upProd.CommandText = @"
                            INSERT OR REPLACE INTO Products
                            (ProductCode, ProductName, PurchaseUnit, PurchasePrice, SaleUnit, SalePrice, Mrp, HsnCode, TaxPercent, Rack, Salt)
                            VALUES (@Code, @Name, @PurUnit, @PurPrice, @SaleUnit, @SalePrice, @Mrp,
                                    COALESCE((SELECT HsnCode FROM Products WHERE ProductCode = @Code), '30049099'),
                                    COALESCE((SELECT TaxPercent FROM Products WHERE ProductCode = @Code), 5),
                                    COALESCE((SELECT Rack FROM Products WHERE ProductCode = @Code), 'STORE'),
                                    COALESCE((SELECT Salt FROM Products WHERE ProductCode = @Code), ''));";

                        var pCode = upProd.Parameters.Add("@Code", DbType.Int32);
                        var pName = upProd.Parameters.Add("@Name", DbType.String);
                        var pPurUnit = upProd.Parameters.Add("@PurUnit", DbType.Int32);
                        var pPurPrice = upProd.Parameters.Add("@PurPrice", DbType.Double);
                        var pSaleUnit = upProd.Parameters.Add("@SaleUnit", DbType.Int32);
                        var pSalePrice = upProd.Parameters.Add("@SalePrice", DbType.Double);
                        var pMrp = upProd.Parameters.Add("@Mrp", DbType.Double);

                        while (rdr.Read())
                        {
                            int code = Convert.ToInt32(rdr[0], CultureInfo.InvariantCulture);
                            string name = Convert.ToString(rdr[1] ?? "").Trim();
                            if (code <= 0 || name.Length == 0) continue;

                            int purU = Math.Max(1, Convert.ToInt32(Math.Round(Convert.ToDouble(rdr[2], CultureInfo.InvariantCulture))));
                            double purP = Math.Max(0, Convert.ToDouble(rdr[3], CultureInfo.InvariantCulture));
                            int salU = Math.Max(1, Convert.ToInt32(Math.Round(Convert.ToDouble(rdr[4], CultureInfo.InvariantCulture))));
                            double salP = Math.Max(0, Convert.ToDouble(rdr[5], CultureInfo.InvariantCulture));
                            double mrp = Math.Max(0, Convert.ToDouble(rdr[6], CultureInfo.InvariantCulture));
                            if (salP <= 0) salP = mrp;

                            pCode.Value = code;
                            pName.Value = name;
                            pPurUnit.Value = purU;
                            pPurPrice.Value = Math.Round(purP, 2);
                            pSaleUnit.Value = salU;
                            pSalePrice.Value = Math.Round(salP, 2);
                            pMrp.Value = Math.Round(mrp, 2);
                            upProd.ExecuteNonQuery();
                            productsSynced++;
                        }
                    }

                    // 2. Sync Active Batches & Stock
                    try
                    {
                        using (var sqlCmd = new SqlCommand(@"
                            SELECT ProductCode,
                                   ISNULL(BatchDescription, 'DEFAULT') AS BatchDesc,
                                   ExpiryDate,
                                   ISNULL(PurchasePrice, 0) AS PPrice,
                                   ISNULL(SalePrice, 0) AS SPrice,
                                   ISNULL(MRP, 0) AS Mrp,
                                   (ISNULL(Stock, 0) + ISNULL(FreeStock, 0)) AS TotalStock
                            FROM Batches WITH (NOLOCK)
                            WHERE (ISNULL(Stock, 0) + ISNULL(FreeStock, 0)) > 0", sqlConn))
                        using (var rdr = sqlCmd.ExecuteReader())
                        using (var upBatch = sqliteConn.CreateCommand())
                        using (var upProdPrice = sqliteConn.CreateCommand())
                        {
                            upBatch.Transaction = tx;
                            upBatch.CommandText = @"
                                INSERT OR REPLACE INTO ProductBatches
                                (ProductCode, BatchDesc, ExpiryMMYY, ExpirySortKey, PPrice, SPrice, Mrp, StockBaseUnits, Location, LastUpdated)
                                VALUES (@PCode, @BDesc, @ExpMMYY, @ExpSort, @PPrice, @SPrice, @Mrp, @Stock, 'STORE', @Upd);";

                            var bPCode = upBatch.Parameters.Add("@PCode", DbType.Int32);
                            var bDesc = upBatch.Parameters.Add("@BDesc", DbType.String);
                            var bExp = upBatch.Parameters.Add("@ExpMMYY", DbType.String);
                            var bSort = upBatch.Parameters.Add("@ExpSort", DbType.String);
                            var bPP = upBatch.Parameters.Add("@PPrice", DbType.Double);
                            var bSP = upBatch.Parameters.Add("@SPrice", DbType.Double);
                            var bMrp = upBatch.Parameters.Add("@Mrp", DbType.Double);
                            var bStock = upBatch.Parameters.Add("@Stock", DbType.Double);
                            var bUpd = upBatch.Parameters.Add("@Upd", DbType.String);

                            upProdPrice.Transaction = tx;
                            upProdPrice.CommandText = "UPDATE Products SET Mrp = @Mrp, SalePrice = @SPrice, PurchasePrice = CASE WHEN @PPrice > 0 THEN @PPrice ELSE PurchasePrice END WHERE ProductCode = @PCode AND @Mrp > 0;";
                            var uPCode = upProdPrice.Parameters.Add("@PCode", DbType.Int32);
                            var uMrp = upProdPrice.Parameters.Add("@Mrp", DbType.Double);
                            var uSP = upProdPrice.Parameters.Add("@SPrice", DbType.Double);
                            var uPP = upProdPrice.Parameters.Add("@PPrice", DbType.Double);

                            string today = DateTime.Now.ToString("yyyy-MM-dd");

                            while (rdr.Read())
                            {
                                int pcode = Convert.ToInt32(rdr[0], CultureInfo.InvariantCulture);
                                string batchDesc = Convert.ToString(rdr[1] ?? "DEFAULT").Trim();
                                if (batchDesc.Length == 0) batchDesc = "DEFAULT";

                                string expMmyy = "12/28";
                                string expSort = "2028-12";
                                if (!(rdr[2] is DBNull) && rdr[2] != null)
                                {
                                    DateTime dt;
                                    if (rdr[2] is DateTime)
                                    {
                                        dt = (DateTime)rdr[2];
                                        expMmyy = dt.ToString("MM/yy");
                                        expSort = dt.ToString("yyyy-MM");
                                    }
                                    else if (DateTime.TryParse(Convert.ToString(rdr[2]), out dt))
                                    {
                                        expMmyy = dt.ToString("MM/yy");
                                        expSort = dt.ToString("yyyy-MM");
                                    }
                                }

                                double pp = Math.Round(Math.Max(0, Convert.ToDouble(rdr[3], CultureInfo.InvariantCulture)), 2);
                                double sp = Math.Round(Math.Max(0, Convert.ToDouble(rdr[4], CultureInfo.InvariantCulture)), 2);
                                double mrp = Math.Round(Math.Max(0, Convert.ToDouble(rdr[5], CultureInfo.InvariantCulture)), 2);
                                if (sp <= 0) sp = mrp;
                                double stk = Math.Round(Math.Max(0, Convert.ToDouble(rdr[6], CultureInfo.InvariantCulture)), 2);

                                bPCode.Value = pcode;
                                bDesc.Value = batchDesc;
                                bExp.Value = expMmyy;
                                bSort.Value = expSort;
                                bPP.Value = pp;
                                bSP.Value = sp;
                                bMrp.Value = mrp;
                                bStock.Value = stk;
                                bUpd.Value = today;
                                upBatch.ExecuteNonQuery();

                                uPCode.Value = pcode;
                                uMrp.Value = mrp;
                                uSP.Value = sp;
                                uPP.Value = pp;
                                upProdPrice.ExecuteNonQuery();

                                batchesSynced++;
                            }
                        }
                    }
                    catch
                    {
                    }

                    // 3. Sync Suppliers
                    try
                    {
                        using (var sqlCmd = new SqlCommand("SELECT * FROM Suppliers WITH (NOLOCK)", sqlConn))
                        using (var rdr = sqlCmd.ExecuteReader())
                        using (var upSup = sqliteConn.CreateCommand())
                        {
                            upSup.Transaction = tx;
                            upSup.CommandText = @"
                                INSERT OR REPLACE INTO Suppliers
                                (SupplierCode, SupplierName, Address, Phone, Gstin, StateName, OutstandingAmount)
                                VALUES (@Code, @Name, @Addr, @Phone, @Gstin, 'TAMILNADU', 0);";

                            var sCode = upSup.Parameters.Add("@Code", DbType.Int32);
                            var sName = upSup.Parameters.Add("@Name", DbType.String);
                            var sAddr = upSup.Parameters.Add("@Addr", DbType.String);
                            var sPhone = upSup.Parameters.Add("@Phone", DbType.String);
                            var sGstin = upSup.Parameters.Add("@Gstin", DbType.String);

                            var cols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < rdr.FieldCount; i++) cols[rdr.GetName(i)] = i;

                            int idxCode = cols.ContainsKey("SupplierCode") ? cols["SupplierCode"] : 0;
                            int idxName = cols.ContainsKey("SupplierName") ? cols["SupplierName"] : 1;
                            int idxAddr = cols.ContainsKey("Address1") ? cols["Address1"] : (cols.ContainsKey("Address") ? cols["Address"] : -1);
                            int idxPhone = cols.ContainsKey("Phone") ? cols["Phone"] : (cols.ContainsKey("Mobile") ? cols["Mobile"] : -1);
                            int idxGst = cols.ContainsKey("GSTNumber") ? cols["GSTNumber"] : (cols.ContainsKey("TINNumber") ? cols["TINNumber"] : -1);

                            while (rdr.Read())
                            {
                                int code = Convert.ToInt32(rdr[idxCode], CultureInfo.InvariantCulture);
                                string name = Convert.ToString(rdr[idxName] ?? "").Trim();
                                if (code <= 0 || name.Length == 0) continue;

                                sCode.Value = code;
                                sName.Value = name;
                                sAddr.Value = idxAddr >= 0 ? Convert.ToString(rdr[idxAddr] ?? "").Trim() : "";
                                sPhone.Value = idxPhone >= 0 ? Convert.ToString(rdr[idxPhone] ?? "").Trim() : "";
                                sGstin.Value = idxGst >= 0 ? Convert.ToString(rdr[idxGst] ?? "").Trim() : "";
                                upSup.ExecuteNonQuery();
                                suppliersSynced++;
                            }
                        }
                    }
                    catch
                    {
                    }

                    sqliteConn.Execute("UPDATE StoreSettings SET NextProductCode = (SELECT COALESCE(MAX(ProductCode), 39031) + 1 FROM Products) WHERE Id = 1;", transaction: tx);
                    tx.Commit();
                }

                statusMessage = string.Format(
                    "Successfully synced from old Shopaid SQL Server ({0}):\n\n" +
                    "• Medicines / Products : {1}\n" +
                    "• Active Stock Batches : {2}\n" +
                    "• Suppliers            : {3}",
                    connectedServer, productsSynced, batchesSynced, suppliersSynced);
                return true;
            }
            catch (Exception ex)
            {
                statusMessage = "Error while reading old Shopaid database: " + ex.Message;
                return false;
            }
        }
    }
}
