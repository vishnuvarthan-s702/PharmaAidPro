using System;
using System.Data.SQLite;
using System.IO;
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
        }
    }
}
