# PharmaAid Pro — System Architecture & Implementation Plan

> **Purpose:** 1:1 offline desktop replacement for **Wondersoft Shopaid.Net-RE (v7.5.16)** at **The Nethaji Medical Stores**, built specifically for the store's 32-bit Windows 7 workstation and TVS MSP dot-matrix printer.

---

## 1. Target Hardware, OS & Technology Stack

| Component | Exact Store Environment | Implementation Decision |
| :--- | :--- | :--- |
| **Workstation (`COMPAQ-PC`)** | Windows 7 Professional SP1 (**32-bit `x86`**) | Compile strictly for **`x86` (32-bit)** |
| **CPU & RAM** | Intel Core i3-3220 @ 3.30 GHz, 4.00 GB RAM (3.41 GB usable) | C# **WinForms** (.NET Framework) — ~35 MB RAM, `< 1s` startup |
| **Display** | 1366×768 resolution | Responsive WinForms `TableLayoutPanel` & `Dock` layouts |
| **Printer** | **TVS MSP Champion Series** Dot-Matrix Printer (tractor-feed continuous stationery) | Raw Win32 `winspool.drv` **ESC/P** print engine + GDI+ Print Preview fallback |
| **Database** | Replacing SQL Server 2008 R2 Express | **SQLite** (`System.Data.SQLite.Core` 1.0.119 + `Dapper` 2.1.89) in **`WAL` mode** |
| **Serialization / Config** | Local settings, backups & exports | `Newtonsoft.Json` 13.0.4 (pre-downloaded in `packages/`) |
| **Licensing & Cloud** | Old app polled `crmlicense.wondersoft.in` & enforced AMC expiry | **100% Offline Forever** — zero internet dependency, zero license lockout |

---

## 2. Store Profile & Numbering Series

* **Store Name:** `The Nethaji Medical Stores`
* **Address:** `No.204/A, Meenakshipuram, Nagercoil-629001`
* **Drug License:** `DLNo : TNK/351/20,21, TNK/417/20`
* **GSTIN:** `33CREPS6130A1Z8` (State Code `33` — Tamil Nadu)
* **Default User:** `Admin` | Legacy Reference ID: `RETM33420`
* **Sales Bill Series:** `C01` + 6-digit sequential zero-padded number (e.g., `C01006808`, `C01006809`, continuing from `C01000001`)
* **Purchase GRN Series:** Sequential integer (`497`, `557`, etc.)

---

## 3. Core Unit, Stock & GST Math

### 3.1 Purchase Unit & Sale Unit Parity
* **Bottles, syrups, creams, ointments, drops, sprays:** `Purchase Unit = 1`, `Sale Unit = 1`.
* **Tablets & capsules (strips):** `Purchase Unit = Sale Unit = Strip Size` (e.g., `10`, `15`, `20`, `30`).
* **Auto-Sync Rule:** Entering `Purchase Unit` automatically populates `Sale Unit` with the exact same value, and entering `MRP` automatically populates `Sale Price = MRP`, preventing billing vs. stock mismatches.

### 3.2 Purchase Inward Math (Whole Packs)
* `Qty` and `Free Qty` on Purchase Invoices are entered in **whole packs/strips** (e.g., `5` strips of `PHLOGAM 10'S`).
* Internal batch stock is stored in **base saleable units**:
  $$\text{Added Base Units} = (\text{Qty} + \text{Free Qty}) \times \text{PurchaseUnit}$$
* UI displays stock in both `Packs` and `Loose` units (e.g., `5.0` or `5 Strip + 0 Tab`).

### 3.3 Retail Sales Billing Math (Loose or Full Units)
* Counter enters **saleable units (tablets/pieces)** in `Qty`, while `Rate` displays **Pack MRP (`SalePrice`)**.
* **Line Amount Formula:**
  $$\text{Line Amount} = \left(\frac{\text{Qty}}{\text{SaleUnit}}\right) \times \text{MRP} \times \left(1 - \frac{\text{Disc\%}}{100}\right)$$
* **Verified Real Bill Examples:**
  * `ZOCEF 500mg TABS` (`SaleUnit = 10`, `MRP = 540.00`, `Qty = 30`) $\rightarrow$ $(30 / 10) \times 540.00 = 1620.00$
  * `CHYMORAL FORTE` (`SaleUnit = 20`, `MRP = 514.12`, `Qty = 20`) $\rightarrow$ $(20 / 20) \times 514.12 = 514.12$
  * `JALRA M 50/500MG 15'S` (`SaleUnit = 15`, `MRP = 316.86`, `Qty = 15`) $\rightarrow$ $(15 / 15) \times 316.86 = 316.86$
  * `T BACT 15GM` (`SaleUnit = 1`, `MRP = 325.08`, `Qty = 2`) $\rightarrow$ $(2 / 1) \times 325.08 = 650.16$

### 3.4 GST Calculation
* **Retail Sales (MRP-Inclusive):**
  $$\text{Base} = \frac{\text{LineAmount}}{1 + (\text{GST\%} / 100)}, \quad \text{SGST} = \text{CGST} = \frac{\text{LineAmount} - \text{Base}}{2}$$
  Plus `ROD` (Round-Off Difference) to compute final `Bill Amount`.
* **Purchase Entry (PTR-Exclusive):**
  $$\text{Taxable} = (\text{Qty} \times \text{PPrice}) - \text{Discount}, \quad \text{SGST} = \text{CGST} = \text{Taxable} \times \frac{\text{GST\%}}{200}$$
  $$\text{Net} = \text{Taxable} + \text{SGST} + \text{CGST} + \text{CoinAdj}$$
  Compared directly against `Amount Payable` (paper supplier bill total) to display **`Diff`** (`Amount Payable - Net`) before saving.

---

## 4. Screen-by-Screen Specifications

### 4.1 Main MDI Shell
* **Top Menu Bar:** `File | Sales | Purchase | Order Management | Accounts | Inventory | Reports | Maintenance | Windows | GST`
* **Status Bar:** Store Name, License Ref (`RETM33420`), Active User (`Admin`), Live Date/Time, and contextual keyboard hints (`F1 - Process`, `F4 - Batch List`, `Ctrl+N - New Product`, `Esc - Close`).
* **100% Keyboard Driven:** `Enter` moves focus field-to-field and cell-to-cell across all forms and grids; `Up`/`Down` navigates live search popups; `Esc` closes popups/dialogs.

### 4.2 Streamlined `NewProduct` Window (Strictly 9 Fields Only)
Replaces the cluttered 23-field legacy modal with **only the 9 fields** used in daily operations:

| # | Field Name | Behavior & Validation |
| :- | :--- | :--- |
| 1 | **Product Code** | Read-only, auto-generated sequential integer (e.g., `39032`, `39033`...) |
| 2 | **Product Name** | Auto-focused on open with red highlight border; mandatory |
| 3 | **Purchase Unit** | Default `1` (bottles/creams/syrups) or strip count (`10`, `15`, `20`, `30`) |
| 4 | **Purchase Price** | Numeric `0.00` (optional during quick entry; editable in Purchase grid) |
| 5 | **Sale Unit** | Automatically mirrors **Purchase Unit** as you type (`Sale Unit = Purchase Unit`) |
| 6 | **Sale Price** | Automatically mirrors **MRP** (`Sale Price = MRP`) |
| 7 | **MRP** | Numeric `0.00`; updates `Sale Price` automatically |
| 8 | **HSN Code** | Type directly (e.g., `30049099`) or enter `0` + `Enter` to open the HSN & GST slab popup (`0%`, `5%`, `12%`, `18%`, `28%`) |
| 9 | **Tax** | Auto-filled from HSN selection or selectable directly (`0 %`, `5 % GST`, `12 % GST`, `18 % GST`, `28 % GST`) |

* **Action Buttons:** `Close` (`Esc`) and `Ok` (`Enter` on last field / `Alt+O`).
* All unused legacy fields (`Alternate Product Code`, `Division`, `Unit Description`, `Bar Code`, `Department`, `Class`, `SubClass`, `Minimum Stock`, `Maximum Stock`, `PackageInfo`, duplicate `Schedule Type`, `Brand Name`) are omitted.

### 4.3 Purchase Entry (`Purchase -> Purchase Entry -> Purchase Invoice (Ctrl+I)`)
* **Header:** `GRN Date`, `Possible GRN No`, `Supplier Name` (live search popup with `SupplierCode`, `SupplierName`, `TinNumber/GSTIN`), auto-filled `Code`, `Address`, `Phone No`, `GST No`, `State`, `Outstanding Rs`, plus `Invoice No`, `Invoice Date`, `Payment Date`, `Amount Payable`, `Discount %`, `Discount Amt`, `Other Cr/Dt`, `Series` (`INVOICE CREDIT` / `INVOICE CASH`), `Coin Adj.`
* **Purchase Grid:** `Sno`, `PCode`, `Product Name` (live search popup showing `P_code`, `ProductName`, `Rack`, `Stock`, `UnitDescr`, `MRP`; press `Ctrl+N` to open the 9-field `NewProduct` modal), `Packing`, `Qty`, `Free Qty`, `Discount Amt`, `Prod Dis%`, `Batch Desc` (`F4` shows existing batches for the selected product), `Expiry Date` (`MM/yy`), `HSN`, `Tax`, `PPrice`, `SPrice`, `MRP`.
* **Bottom Panels:**
  * **Tax & Tally Panel:** Slab-wise `PTax%`, `STax%`, `Location`, `Gross Amt`, `SGST`, `CGST`, `CESS`, `Net`, and **`Diff`** (`Amount Payable - Net`).
  * **Stock & Margin Panel:** `Location` (`STORE`), `Rack`, `Batch Stock`, `Total Stock`, `Item Cost`, `Discounted Sale Rate`, `Cur.Margin`, `Prev.Margin`, `Prev.Cost`, `Tot Margin % On Cost`, `Mrp Total`.
  * **Actions:** `Cancel` and `Process - F1` $\rightarrow$ saves atomically and shows `Purchase Updated Successfully! GRN Number : <No>`.

### 4.4 Retail Sales POS Billing (`Sales -> Retail Bill` / `F2`)
* **Header:** `Bill No` (`C01xxxxxx`), `Bill Date`, `Customer Name`, `Doctor Name`, and high-visibility top-right **`Total : Rs 0.00`** banner.
* **Billing Grid:** `S.No`, `P.Code`, `Product Name` (live search popup), `Quantity`, `Batch` (auto-picks earliest non-expired batch via FIFO; `F4` opens batch list), `Expiry Date` (`MM/yy`), `Rate` (`MRP`), `Dis%`, `Amount`, `Tax%`, `Total`.
* **Function Bar:** `F1 - Save & Print`, `F3 - Generic Substitute`, `F4 - Batch List`, `F5 - Hold Bill`, `F6 - Recall Bill`, `F7 - Sales Return`, `F8 - Bill Copy / Reprint`.

### 4.5 TVS MSP Dot-Matrix Continuous Receipt Engine
Outputs raw **ESC/P** 80-column draft text directly to the Windows print spooler (`TVS MSP Series`):

```text
                       WISH YOU A SPEEDY RECOVERY

                    THE NETAJI MEDICAL STORE
              No.204/A,Meenakshipuram,Nagercoil-629001
  DLNo :TNK/351/20,21, TNK/417/20       GST No :33CREPS6130A1Z8
  --------------------------------------------------------------------
  Name   : M.VENKATESWARAN              Bill No  : C01006808
  Doctor :                              Bill Date: 23/10/26 08:01:16
  --------------------------------------------------------------------
   Qty Particulars              Batch No   Exp     Rate     Amount
  --------------------------------------------------------------------
    30 ZOCEF 500mg TABS         25460251  01/27   540.00   1620.00
     1 ENSURE VANILA 400GM REFI 037MN1    01/28   745.00    745.00
  --------------------------------------------------------------------
  Base : 5686.16 SGST: 142.15 CGST: 142.15 GST:284.3082  ROD: 0
  Total Item : 8  Total Qty : 100  User : Admin
                                     Bill Amount : 5970.47
                                   for THE NETAJI MEDICAL STORE
```

Includes an on-screen **Print Preview / A4 PDF fallback** when testing without the physical printer attached.

---

## 5. SQLite Database Schema & Legacy Data Migration

### 5.1 Core Tables (`PharmaAidPro.db`)
* `StoreSettings` — Store header, DLNo, GSTIN, Bill/GRN prefixes, printer port/name, backup folder.
* `HsnTaxMaster` — HSN codes (`0`, `30049061`, `30049029`, etc.) mapped to GST% slabs (`0`, `5`, `12`, `18`, `28`).
* `Suppliers` — Code, Name, Address, Phone, GSTIN, State, Opening/Current Outstanding balance.
* `Products` — `ProductCode`, `ProductName`, `PurchaseUnit`, `PurchasePrice`, `SaleUnit`, `SalePrice`, `MRP`, `HsnCode`, `TaxPercent`, `Rack`, `Salt`.
* `ProductBatches` — `BatchId`, `ProductCode`, `BatchDesc`, `ExpiryMMYY`, `ExpirySortKey`, `PPrice`, `SPrice`, `MRP`, `StockBaseUnits`, `LastUpdated`.
* `PurchaseHeaders` & `PurchaseLines` — GRN No, Supplier, Invoice No/Dates, Payable/Discount/Tax/Net totals, line-level Qty, FreeQty, Batch, Expiry, Cost, Margin.
* `SalesHeaders` & `SalesLines` — `BillNo` (`C01xxxxxx`), BillDate, Customer, Doctor, Base/SGST/CGST/ROD/Total, line-level Qty, Batch, MRP, Discount, Tax.
* `Customers` & `Doctors` — Quick lookup and credit/Khata tracking.

### 5.2 Legacy Data Migration (`oldsoftware/`)
* Dedicated importer utility to migrate products (~39,031 masters), suppliers (`SUN AGENCIES`, `S.A.V. TRADING CORPORATION`, `SUNDAR PHARMA`, etc.), and active batch stock from `oldsoftware/ReferenceDB_Pharmacy.bak` / `ReferenceDB_Pharmacy_7.5.0_Data.rar` (or exported CSV/SQL).
* Out-of-the-box SQLite database pre-seeded with real medicines, suppliers, and bills observed in the store references for instant verification.

---

## 6. Phased Implementation & Git Workflow

1. **Phase 1 — Project Scaffold & SQLite Database Layer (`x86` WinForms)**
   * Create `PharmaAidPro.sln` and `PharmaAidPro.WinForms.csproj` targeting `.NET Framework` (`x86`), referencing local `packages/`.
   * Implement SQLite initialization (`WAL` mode), schema creation, and store/sample data seeding.
2. **Phase 2 — Main MDI Shell, 9-Field `NewProduct` Modal & Masters**
   * Build the `Shopaid.Net-RE` MDI shell, top menu bar, status strip, and keyboard navigation framework.
   * Build the streamlined **9-field `NewProduct`** dialog with `Sale Unit = Purchase Unit` & `Sale Price = MRP` auto-sync and HSN/Tax selector popup.
   * Build Supplier, Product, and Batch Master views.
3. **Phase 3 — Purchase Invoice (`Ctrl+I`) & GRN Processing**
   * Build Purchase Entry with live Supplier/Product search popups, `F4` batch list, tax/margin calculations, `Amount Payable` vs `Net` tally (`Diff`), and atomic `Process - F1` GRN save.
4. **Phase 4 — Retail Sales POS (`F2`), Bill Copy & TVS MSP Dot-Matrix Printing**
   * Build Retail Sales Billing (`C01xxxxxx`), FIFO batch selection, loose-unit pricing, GST back-calculation, and Raw ESC/P dot-matrix printing + preview.
5. **Phase 5 — Reports, Backup Utility & Legacy Data Importer**
   * Add Sales/Purchase registers, Stock & Near-Expiry alerts, GST summary exports, automatic local/USB backup, and legacy data import tool.
6. **Phase 6 — Login Page UI (`LoginForm.cs`) & Store Background Loading Screen (`LoadingSplashForm.cs`)**
   * Startup `LoginForm` (`Admin` / `SHOPAID`) followed by a full-background store photo loading splash screen (`Assets/store_bg.jpg`) for **The Nethaji Medical Stores**, plus MDI background branding and Logout / Switch User support.

* **Git Discipline:** Every completed milestone and update is committed and pushed directly to [`https://github.com/vishnuvarthan-s702/PharmaAidPro`](https://github.com/vishnuvarthan-s702/PharmaAidPro).
