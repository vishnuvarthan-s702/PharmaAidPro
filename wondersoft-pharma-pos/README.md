# PharmaAid Pro | Modern Wondersoft Shopaid Remake

**PharmaAid Pro** is a high-speed, modern web-native remaking of **Wondersoft Shopaid (Shopaid.NET / ReferenceDB_Pharmacy)** retail ERP and Pharmacy Point-of-Sale software.

It retains all the hallmark features that pharmacies and medical stores depend on—high-speed keyboard billing, batch & expiry control, Schedule H/H1 compliance registers, generic substitute suggestions, GST invoicing, distributor inward, and customer Khata—while delivering an ultra-fast, zero-dependency, modern UI that works 100% offline.

---

## ⚡ Highlights & Key Features

### 1. High-Speed Counter POS Billing (F2)
- **Keyboard-First Interface:** Complete operation using function keys (`F1`-`F12`), arrow navigation, and `Enter`.
- **Intelligent Omnisearch:** Instant search by Brand Name, Generic Molecule / Salt, Rack Location, or Barcode scan.
- **Batch Selection & FIFO:** Automatically recommends the earliest expiry batch; displays batch number, expiry date (MM/YY), MRP, and stock in hand.
- **Strip vs. Loose Tablet Conversion:** Sell full strips or loose tablets/capsules with automatic price pro-rating.
- **Schedule H / H1 Regulatory Warnings:** Visual alert badge when scheduled drugs are in the cart; enforces prescribing doctor assignment.
- **Bill Parking & Recall (F7 / F8):** Put up to 10 customer carts on hold and instantly resume without losing billing state.
- **Multi-Mode Tender (F10):** Settle with Cash (automatic change calculator), simulated dynamic UPI QR Code, Card, or Customer Khata (store credit).

### 2. Generic Substitute Finder Engine (F3)
- Search any branded medicine (e.g., *Augmentin 625*, *Pan 40*, *Telma 40*, *Shelcal 500*).
- System detects active chemical salt formulation and scans local stock for generic equivalents.
- Displays price comparison and patient savings percentage (e.g., *"Save ₹78.50 / 55% OFF"*).
- One-click insertion of generic alternatives directly into the active bill.

### 3. Regulatory Compliance: Schedule H & H1 Register
- Statutory compliance with Indian **Drugs and Cosmetics Rules (Rule 65 & 97)**.
- Automatically records every dispensed Schedule H / H1 drug with Date, Invoice #, Patient Name, Patient Contact, Prescribing Doctor, Medical Council Reg #, Drug Name, Batch #, Expiry, and Quantity.
- **Export to CSV** for Drug Inspector audits and one-click statutory printing.

### 4. Advanced Inventory, Batches & Expiry Desk (F4)
- Live inventory valuation (MRP value & PTR cost value).
- Low-stock warnings and real-time stock decrement upon sale.
- **Expiry Quarantine Desk:** Segregates expired and near-expiry (< 90 days) medicines away from the billing counter.
- **Supplier Return Debit Note:** Generate debit return vouchers to distributors for credit reimbursement.

### 5. Purchase Inward Entry & Wholesaler Management
- Record supplier purchase invoices with Batch #, Expiry, Received Quantity, Free Scheme items (e.g., 10+2 free), PTR, MRP, Trade Discounts, and GST.
- Automatically updates batch inventory and supplier credit balance.

### 6. Customer Khata & Loyalty Ledger
- Maintain customer health profiles, loyalty points, and credit accounts (Khata).
- Collect payments against credit balances with immediate ledger reconciliation.

### 7. Dual Format GST Printing (F12)
- **80mm / 58mm Thermal Receipt:** Optimized for high-speed thermal POS printers (includes DL numbers, GSTIN, HSN breakdown, schedule warning text).
- **A4 / A5 Tax Invoice:** Official GST Tax Invoice layout for corporate/B2B customers and tax documentation.

### 8. 100% Offline & Zero Server Dependencies
- Uses persistent browser storage (`localStorage` + `IndexedDB`).
- Full database backup (`.json`) and restore capability.
- Preloaded with 20+ realistic Indian medicine brands, batches, doctors, customers, and distributors.

---

## ⌨️ Keyboard Shortcuts Reference

| Hotkey | Function |
|---|---|
| **F1** | Open Keyboard Shortcuts & Help Guide |
| **F2** | Go to POS Billing / Focus Search Bar |
| **F3** | Open Generic Medicine & Substitute Finder |
| **F4** | Go to Inventory & Batches Browser |
| **F7** | Park / Hold Current Bill |
| **F8** | Open Held / Parked Bills Drawer |
| **F10** | Settle Bill & Tender Payment Modal |
| **F12** | Print Bill (Thermal Receipt or A4 Invoice) |
| **Esc** | Close any open modal / Cancel search dropdown |

---

## 🚀 How to Run the Application

### Option 1: Direct Double Click
Double-click `start-app.bat` inside this folder. It will instantly launch the application in your default web browser (Edge, Chrome, etc.).

### Option 2: PowerShell
Right-click `start-app.ps1` and choose **Run with PowerShell**, or execute:
```powershell
powershell -ExecutionPolicy Bypass -File .\start-app.ps1
```

### Option 3: Direct File Open
Simply double-click `index.html` to open it in any web browser.

---

## 📂 File Structure

- `index.html`: Semantic, responsive single-page application structure.
- `styles.css`: Complete styling with CSS variables, light/dark themes, responsive layout, and print stylesheets for 80mm thermal rolls and A4 paper.
- `app.js`: Complete business logic, persistent data layer, POS engine, inventory math, GST calculations, and regulatory registers.
- `start-app.bat`: Windows batch launcher.
- `start-app.ps1`: PowerShell launcher script.
- `README.md`: System manual and technical documentation.
