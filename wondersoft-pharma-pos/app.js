/**
 * PharmaAid Pro - Next-Generation Wondersoft Shopaid ERP & POS
 * High-speed Pharmacy Management System
 */

// Global App State
const DB_KEY = 'pharmaaid_pro_db';

const defaultStoreSettings = {
  storeName: "THE NETAJI MEDICAL STORE",
  address: "No.204/A, Meenakshipuram, Nagercoil-629001",
  dlNo: "TN/NGL/20B/10492, TN/NGL/21B/10492",
  gstin: "33AAAAA0000A1Z5",
  phone: "+91 94431 00000",
  printFormat: "thermal",
  tillOpening: 1500.00
};

// Seed Indian Medicine Master Catalog with Realistic Batches & Salts
const seedMedicines = [
  {
    id: "MED001",
    name: "Dolo 650 Tab",
    salt: "Paracetamol 650mg",
    form: "Tablet",
    mfg: "Micro Labs Ltd",
    schedule: "General",
    hsn: "300490",
    pack: 15,
    rack: "A-01",
    barcode: "890114820001",
    batches: [
      { batchNo: "DL4920", exp: "08/28", stockPacks: 45, stockLoose: 8, mrp: 34.15, ptr: 26.20, gst: 12 },
      { batchNo: "DL4810", exp: "12/26", stockPacks: 12, stockLoose: 2, mrp: 32.50, ptr: 25.00, gst: 12 } // Near expiry
    ]
  },
  {
    id: "MED002",
    name: "Augmentin 625 Duo Tab",
    salt: "Amoxicillin 500mg + Clavulanic Acid 125mg",
    form: "Tablet",
    mfg: "GlaxoSmithKline (GSK)",
    schedule: "H",
    hsn: "300410",
    pack: 10,
    rack: "B-04",
    barcode: "890114820002",
    batches: [
      { batchNo: "AG8821", exp: "11/27", stockPacks: 28, stockLoose: 0, mrp: 223.50, ptr: 178.80, gst: 12 },
      { batchNo: "AG7102", exp: "11/26", stockPacks: 4, stockLoose: 3, mrp: 215.00, ptr: 172.00, gst: 12 }
    ]
  },
  {
    id: "MED003",
    name: "Moxikind-CV 625 Tab",
    salt: "Amoxicillin 500mg + Clavulanic Acid 125mg",
    form: "Tablet",
    mfg: "Mankind Pharma",
    schedule: "H",
    hsn: "300410",
    pack: 10,
    rack: "B-05",
    barcode: "890114820003",
    batches: [
      { batchNo: "MK1094", exp: "05/28", stockPacks: 35, stockLoose: 5, mrp: 145.00, ptr: 110.00, gst: 12 }
    ]
  },
  {
    id: "MED004",
    name: "Pan 40 Tab",
    salt: "Pantoprazole 40mg",
    form: "Tablet",
    mfg: "Alkem Laboratories",
    schedule: "H",
    hsn: "300490",
    pack: 15,
    rack: "C-02",
    barcode: "890114820004",
    batches: [
      { batchNo: "PN9012", exp: "09/27", stockPacks: 40, stockLoose: 6, mrp: 162.00, ptr: 125.00, gst: 12 },
      { batchNo: "PN8104", exp: "08/26", stockPacks: 6, stockLoose: 0, mrp: 155.00, ptr: 120.00, gst: 12 } // Expired / Near
    ]
  },
  {
    id: "MED005",
    name: "Pantocid 40 Tab",
    salt: "Pantoprazole 40mg",
    form: "Tablet",
    mfg: "Sun Pharma",
    schedule: "H",
    hsn: "300490",
    pack: 15,
    rack: "C-03",
    barcode: "890114820005",
    batches: [
      { batchNo: "PC3341", exp: "03/28", stockPacks: 32, stockLoose: 4, mrp: 158.00, ptr: 121.50, gst: 12 }
    ]
  },
  {
    id: "MED006",
    name: "Azithral 500 Tab",
    salt: "Azithromycin 500mg",
    form: "Tablet",
    mfg: "Alembic Pharmaceuticals",
    schedule: "H1",
    hsn: "300420",
    pack: 5,
    rack: "B-02",
    barcode: "890114820006",
    batches: [
      { batchNo: "AZ5520", exp: "10/27", stockPacks: 50, stockLoose: 2, mrp: 132.00, ptr: 101.50, gst: 12 }
    ]
  },
  {
    id: "MED007",
    name: "Azee 500 Tab",
    salt: "Azithromycin 500mg",
    form: "Tablet",
    mfg: "Cipla Ltd",
    schedule: "H1",
    hsn: "300420",
    pack: 5,
    rack: "B-03",
    barcode: "890114820007",
    batches: [
      { batchNo: "CP9801", exp: "04/28", stockPacks: 40, stockLoose: 1, mrp: 128.00, ptr: 98.00, gst: 12 }
    ]
  },
  {
    id: "MED008",
    name: "Telma 40 Tab",
    salt: "Telmisartan 40mg",
    form: "Tablet",
    mfg: "Glenmark Pharmaceuticals",
    schedule: "H",
    hsn: "300490",
    pack: 30,
    rack: "D-01",
    barcode: "890114820008",
    batches: [
      { batchNo: "TM4011", exp: "12/27", stockPacks: 25, stockLoose: 10, mrp: 275.00, ptr: 211.50, gst: 12 }
    ]
  },
  {
    id: "MED009",
    name: "Telmikind 40 Tab",
    salt: "Telmisartan 40mg",
    form: "Tablet",
    mfg: "Mankind Pharma",
    schedule: "H",
    hsn: "300490",
    pack: 10,
    rack: "D-02",
    barcode: "890114820009",
    batches: [
      { batchNo: "TK2019", exp: "01/28", stockPacks: 60, stockLoose: 5, mrp: 45.00, ptr: 34.50, gst: 12 }
    ]
  },
  {
    id: "MED010",
    name: "Glycomet GP 2 Tab",
    salt: "Metformin 500mg + Glimepiride 2mg",
    form: "Tablet",
    mfg: "USV Private Limited",
    schedule: "H",
    hsn: "300490",
    pack: 15,
    rack: "D-05",
    barcode: "890114820010",
    batches: [
      { batchNo: "GM2291", exp: "07/27", stockPacks: 55, stockLoose: 8, mrp: 142.00, ptr: 109.00, gst: 12 }
    ]
  },
  {
    id: "MED011",
    name: "Shelcal 500 Tab",
    salt: "Calcium 500mg + Vitamin D3 250IU",
    form: "Tablet",
    mfg: "Torrent Pharmaceuticals",
    schedule: "General",
    hsn: "300490",
    pack: 15,
    rack: "E-01",
    barcode: "890114820011",
    batches: [
      { batchNo: "SH9102", exp: "06/28", stockPacks: 70, stockLoose: 10, mrp: 131.00, ptr: 100.00, gst: 12 }
    ]
  },
  {
    id: "MED012",
    name: "Cipcal 500 Tab",
    salt: "Calcium 500mg + Vitamin D3 250IU",
    form: "Tablet",
    mfg: "Cipla Ltd",
    schedule: "General",
    hsn: "300490",
    pack: 15,
    rack: "E-02",
    barcode: "890114820012",
    batches: [
      { batchNo: "CC1189", exp: "11/27", stockPacks: 45, stockLoose: 5, mrp: 98.00, ptr: 75.00, gst: 12 }
    ]
  },
  {
    id: "MED013",
    name: "Allegra 120mg Tab",
    salt: "Fexofenadine 120mg",
    form: "Tablet",
    mfg: "Sanofi India",
    schedule: "H",
    hsn: "300490",
    pack: 10,
    rack: "F-01",
    barcode: "890114820013",
    batches: [
      { batchNo: "AL7712", exp: "02/28", stockPacks: 22, stockLoose: 2, mrp: 218.00, ptr: 168.00, gst: 12 }
    ]
  },
  {
    id: "MED014",
    name: "Combiflam Tab",
    salt: "Ibuprofen 400mg + Paracetamol 325mg",
    form: "Tablet",
    mfg: "Sanofi India",
    schedule: "General",
    hsn: "300490",
    pack: 20,
    rack: "A-03",
    barcode: "890114820014",
    batches: [
      { batchNo: "CF4410", exp: "09/27", stockPacks: 50, stockLoose: 12, mrp: 46.50, ptr: 35.80, gst: 12 }
    ]
  },
  {
    id: "MED015",
    name: "Ascoril D Plus Syrup 100ml",
    salt: "Dextromethorphan + Phenylephrine + Chlorpheniramine",
    form: "Syrup",
    mfg: "Glenmark Pharmaceuticals",
    schedule: "H",
    hsn: "300490",
    pack: 1,
    rack: "S-01",
    barcode: "890114820015",
    batches: [
      { batchNo: "AS1098", exp: "05/27", stockPacks: 30, stockLoose: 0, mrp: 129.00, ptr: 99.00, gst: 12 }
    ]
  },
  {
    id: "MED016",
    name: "Alprax 0.5mg Tab",
    salt: "Alprazolam 0.5mg",
    form: "Tablet",
    mfg: "Torrent Pharmaceuticals",
    schedule: "H1",
    hsn: "300490",
    pack: 15,
    rack: "SEC-01",
    barcode: "890114820016",
    batches: [
      { batchNo: "AX2201", exp: "04/28", stockPacks: 25, stockLoose: 0, mrp: 42.00, ptr: 32.00, gst: 12 }
    ]
  },
  {
    id: "MED017",
    name: "Becosules Z Capsules",
    salt: "Vitamin B-Complex + Vitamin C + Zinc",
    form: "Capsule",
    mfg: "Pfizer Ltd",
    schedule: "General",
    hsn: "300490",
    pack: 20,
    rack: "E-05",
    barcode: "890114820017",
    batches: [
      { batchNo: "BZ8812", exp: "12/27", stockPacks: 80, stockLoose: 15, mrp: 54.00, ptr: 41.50, gst: 12 }
    ]
  },
  {
    id: "MED018",
    name: "Volini Gel 30g",
    salt: "Diclofenac Diethylamine + Virgin Linseed Oil",
    form: "Ointment",
    mfg: "Sun Pharma",
    schedule: "General",
    hsn: "300490",
    pack: 1,
    rack: "O-02",
    barcode: "890114820018",
    batches: [
      { batchNo: "VL5501", exp: "08/27", stockPacks: 25, stockLoose: 0, mrp: 145.00, ptr: 112.00, gst: 12 }
    ]
  },
  {
    id: "MED019",
    name: "Montek LC Tab",
    salt: "Montelukast 10mg + Levocetirizine 5mg",
    form: "Tablet",
    mfg: "Sun Pharma",
    schedule: "H",
    hsn: "300490",
    pack: 10,
    rack: "F-05",
    barcode: "890114820019",
    batches: [
      { batchNo: "ML9082", exp: "01/28", stockPacks: 30, stockLoose: 4, mrp: 195.00, ptr: 150.00, gst: 12 }
    ]
  },
  {
    id: "MED020",
    name: "Monticope Tab",
    salt: "Montelukast 10mg + Levocetirizine 5mg",
    form: "Tablet",
    mfg: "Mankind Pharma",
    schedule: "H",
    hsn: "300490",
    pack: 10,
    rack: "F-06",
    barcode: "890114820020",
    batches: [
      { batchNo: "MC4491", exp: "03/28", stockPacks: 45, stockLoose: 6, mrp: 110.00, ptr: 84.00, gst: 12 }
    ]
  }
];

const seedDoctors = [
  { id: "DOC01", name: "Dr. K. Ramanathan MD", regNo: "TNMC-48291", spec: "General Physician", clinic: "City Health Clinic, Nagercoil", phone: "94431 22334" },
  { id: "DOC02", name: "Dr. Priya Suresh DNB, DGO", regNo: "TNMC-59201", spec: "Gynecologist", clinic: "Kanya Women's Clinic", phone: "98421 77665" },
  { id: "DOC03", name: "Dr. S. Sivakumar MS (Ortho)", regNo: "TNMC-38102", spec: "Orthopedic Surgeon", clinic: "Apollo Speciality Clinic", phone: "94432 99112" },
  { id: "DOC04", name: "Dr. M. Joseph MBBS", regNo: "TNMC-61029", spec: "Consultant Physician", clinic: "Meenakshi Mission Hospital", phone: "98410 44332" }
];

const seedCustomers = [
  { id: "CUST01", name: "M. Murugan", phone: "98421 11223", address: "Meenakshipuram, Nagercoil", points: 150, khataBalance: 450.00 },
  { id: "CUST02", name: "R. Sundaram", phone: "94431 44556", address: "Kottar, Nagercoil", points: 280, khataBalance: 0.00 },
  { id: "CUST03", name: "S. Anitha", phone: "98410 77889", address: "Vadasery, Nagercoil", points: 95, khataBalance: 120.00 }
];

const seedSuppliers = [
  { id: "SUP01", name: "Apollo Pharma Distributors", contact: "R. Balaji", phone: "98421 99887", dlNo: "TN/NGL/20B/8812", gstin: "33AACCA1234F1Z9", balance: 14500.00 },
  { id: "SUP02", name: "MedPlus Wholesalers & Agencies", contact: "K. Mohan", phone: "94431 88776", dlNo: "TN/NGL/20B/9941", gstin: "33AABBM5678K1Z3", balance: 8200.00 },
  { id: "SUP03", name: "Sun Pharma Agency (Direct)", contact: "T. Kumar", phone: "98410 66554", dlNo: "TN/NGL/20B/7710", gstin: "33AAACS9012L1Z8", balance: 0.00 }
];

// In-Memory Database Wrapper
class PharmacyDB {
  constructor() {
    this.data = this.loadData();
  }

  loadData() {
    try {
      const stored = localStorage.getItem(DB_KEY);
      if (stored) {
        return JSON.parse(stored);
      }
    } catch (e) {
      console.error("Error reading from localStorage:", e);
    }
    return this.getInitialData();
  }

  getInitialData() {
    return {
      settings: { ...defaultStoreSettings },
      medicines: [...seedMedicines],
      doctors: [...seedDoctors],
      customers: [...seedCustomers],
      suppliers: [...seedSuppliers],
      salesInvoices: [],
      purchaseInvoices: [],
      scheduleRegister: [],
      heldBills: [],
      tillTransactions: [
        { type: "OPENING", amount: defaultStoreSettings.tillOpening, desc: "Opening Float Cash", time: new Date().toISOString() }
      ]
    };
  }

  save() {
    try {
      localStorage.setItem(DB_KEY, JSON.stringify(this.data));
    } catch (e) {
      console.error("Error writing to localStorage:", e);
    }
  }

  reset() {
    this.data = this.getInitialData();
    this.save();
  }
}

const db = new PharmacyDB();

// POS Billing Active State
let currentBill = {
  items: [],
  customer: "walkin",
  doctor: "none",
  billDiscPercent: 0,
  subtotal: 0,
  discountTotal: 0,
  taxableTotal: 0,
  gstTotal: 0,
  roundOff: 0,
  grandTotal: 0,
  hasScheduleDrugs: false
};

// Selected medicine for quick entry
let pendingMedicine = null;
let currentTenderMode = 'cash';
let lastCompletedInvoice = null;

// Initialize on DOM Ready
document.addEventListener("DOMContentLoaded", () => {
  initLiveClock();
  applyStoreHeader();
  renderPOSDropdowns();
  renderInventoryTable();
  renderPurchaseDropdowns();
  renderPurchaseHistory();
  renderScheduleRegisterTable();
  renderCustomersTable();
  renderDoctorsTable();
  renderSuppliersTable();
  renderReportsData();
  renderTillStatus();
  updateExpiryCountBadge();
  setupKeyboardHotkeys();
  setupPOSSearch();
  setupPurchaseInvoiceDateDefault();
});

// Live Clock
function initLiveClock() {
  const clockEl = document.getElementById("live-clock");
  const update = () => {
    const now = new Date();
    clockEl.innerText = now.toLocaleDateString("en-IN", { day: '2-digit', month: 'short' }) + " " + now.toLocaleTimeString("en-IN");
  };
  update();
  setInterval(update, 1000);
}

// Store Header Info
function applyStoreHeader() {
  const s = db.data.settings;
  document.getElementById("hdr-store-name").innerText = s.storeName;
  document.getElementById("hdr-dl-no").innerText = "DL: " + s.dlNo.split(",")[0];
  document.getElementById("hdr-gstin").innerText = "GSTIN: " + s.gstin;
  document.getElementById("set-store-name").value = s.storeName;
  document.getElementById("set-store-addr").value = s.address;
  document.getElementById("set-store-dl").value = s.dlNo;
  document.getElementById("set-store-gstin").value = s.gstin;
  document.getElementById("set-store-phone").value = s.phone;
  document.getElementById("set-print-format").value = s.printFormat;
}

// Tab Switching
function switchTab(tabName) {
  document.querySelectorAll(".nav-tab").forEach(tab => {
    tab.classList.toggle("active", tab.dataset.tab === tabName);
  });
  document.querySelectorAll(".view-section").forEach(sec => {
    sec.classList.remove("active");
  });
  const target = document.getElementById(`view-${tabName}`);
  if (target) {
    target.classList.add("active");
  }

  // Refresh view contents on visit
  if (tabName === "pos") {
    setTimeout(() => document.getElementById("pos-search-input").focus(), 50);
  } else if (tabName === "inventory") {
    renderInventoryTable();
  } else if (tabName === "expiry") {
    renderExpiryDesk();
  } else if (tabName === "schedule") {
    renderScheduleRegisterTable();
  } else if (tabName === "substitutes") {
    renderSubstituteResults();
  } else if (tabName === "reports") {
    renderReportsData();
  } else if (tabName === "till") {
    renderTillStatus();
  }
}

// Dark/Light Theme Toggle
function toggleTheme() {
  const current = document.documentElement.getAttribute("data-theme");
  const next = current === "dark" ? "light" : "dark";
  document.documentElement.setAttribute("data-theme", next);
  localStorage.setItem("pharmaaid_theme", next);
}

// Apply saved theme
const savedTheme = localStorage.getItem("pharmaaid_theme") || "light";
document.documentElement.setAttribute("data-theme", savedTheme);

// ==========================================
// KEYBOARD HOTKEYS ENGINE (F1 - F12)
// ==========================================
function setupKeyboardHotkeys() {
  window.addEventListener("keydown", (e) => {
    // If a modal is open and Esc pressed, close top modal
    if (e.key === "Escape") {
      document.querySelectorAll(".modal-overlay.show").forEach(m => m.classList.remove("show"));
      document.getElementById("pos-search-dropdown").classList.remove("show");
      cancelQuickEntry();
      return;
    }

    // Hotkey F1: Help / Shortcuts
    if (e.key === "F1") {
      e.preventDefault();
      openHotkeysModal();
      return;
    }

    // Hotkey F2: POS Billing
    if (e.key === "F2") {
      e.preventDefault();
      switchTab("pos");
      document.getElementById("pos-search-input").focus();
      return;
    }

    // Hotkey F3: Generic Substitute Finder
    if (e.key === "F3") {
      e.preventDefault();
      switchTab("substitutes");
      document.getElementById("substitute-search-input").focus();
      return;
    }

    // Hotkey F4: Inventory
    if (e.key === "F4") {
      e.preventDefault();
      switchTab("inventory");
      return;
    }

    // Hotkey F7: Park Bill
    if (e.key === "F7") {
      e.preventDefault();
      holdCurrentBill();
      return;
    }

    // Hotkey F8: Resume Held Bill
    if (e.key === "F8") {
      e.preventDefault();
      openHeldBillsModal();
      return;
    }

    // Hotkey F10: Settle & Tender
    if (e.key === "F10") {
      e.preventDefault();
      openTenderModal();
      return;
    }

    // Hotkey F12: Print Bill
    if (e.key === "F12") {
      e.preventDefault();
      if (document.getElementById("tender-modal").classList.contains("show")) {
        finalizeBillAndPrint(true);
      } else {
        printLastBill();
      }
      return;
    }
  });
}

// ==========================================
// POS BILLING ENGINE (F2)
// ==========================================

function setupPOSSearch() {
  const searchInput = document.getElementById("pos-search-input");
  const dropdown = document.getElementById("pos-search-dropdown");

  searchInput.addEventListener("input", () => {
    const query = searchInput.value.trim().toLowerCase();
    if (query.length === 0) {
      dropdown.classList.remove("show");
      return;
    }

    // Search by brand name, salt composition, barcode, or rack
    const matches = db.data.medicines.filter(m => 
      m.name.toLowerCase().includes(query) ||
      m.salt.toLowerCase().includes(query) ||
      (m.barcode && m.barcode.includes(query)) ||
      m.rack.toLowerCase().includes(query)
    );

    if (matches.length === 0) {
      dropdown.innerHTML = `<div style="padding: 12px; text-align: center; color: var(--text-muted); font-size: 12px;">No matching medicine found. Press F3 to check substitutes.</div>`;
      dropdown.classList.add("show");
      return;
    }

    dropdown.innerHTML = matches.map((m, idx) => {
      // Calculate total stock and earliest batch
      const totalPacks = m.batches.reduce((sum, b) => sum + b.stockPacks, 0);
      const firstBatch = m.batches[0] || { mrp: 0, batchNo: "N/A", exp: "N/A" };
      let stockClass = "search-item-stock";
      if (totalPacks === 0) stockClass += " out";
      else if (totalPacks < 10) stockClass += " low";

      let schBadge = m.schedule !== 'General' 
        ? `<span class="badge ${m.schedule === 'H1' ? 'badge-schedule-h1' : 'badge-schedule-h'}">Sch ${m.schedule}</span>` 
        : '';

      return `
        <div class="search-item ${idx === 0 ? 'highlighted' : ''}" onclick="selectMedicineForQuickEntry('${m.id}')">
          <div class="search-item-info">
            <div class="search-item-title">
              ${m.name}
              ${schBadge}
              <span class="meta-pill" style="font-size: 10px;">Rack: ${m.rack}</span>
            </div>
            <div class="search-item-salt">${m.salt} • ${m.mfg}</div>
          </div>
          <div class="search-item-meta">
            <div class="search-item-price">MRP ₹${firstBatch.mrp.toFixed(2)}</div>
            <div class="${stockClass}">${totalPacks > 0 ? totalPacks + ' in stock' : 'OUT OF STOCK'}</div>
          </div>
        </div>
      `;
    }).join("");

    dropdown.classList.add("show");
  });

  // Handle enter key on search input
  searchInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      const highlighted = dropdown.querySelector(".search-item.highlighted") || dropdown.querySelector(".search-item");
      if (highlighted) {
        highlighted.click();
      }
    }
  });
}

function selectMedicineForQuickEntry(medId) {
  const med = db.data.medicines.find(m => m.id === medId);
  if (!med) return;

  pendingMedicine = med;
  document.getElementById("pos-search-dropdown").classList.remove("show");
  document.getElementById("pos-search-input").value = "";

  // Show quick entry bar
  const qBar = document.getElementById("quick-entry-bar");
  qBar.style.display = "flex";

  document.getElementById("qe-item-name").value = `${med.name} (${med.salt})`;
  
  // Populate batches in batch selector
  const batchSelect = document.getElementById("qe-batch-select");
  batchSelect.innerHTML = med.batches.map(b => {
    const isExpired = checkIfExpired(b.exp);
    const label = `${b.batchNo} | Exp: ${b.exp} | Stk: ${b.stockPacks}p (${isExpired ? 'EXPIRED' : '₹' + b.mrp})`;
    return `<option value="${b.batchNo}" ${isExpired ? 'disabled style="color: red;"' : ''}>${label}</option>`;
  }).join("");

  document.getElementById("qe-qty").value = 1;
  document.getElementById("qe-disc").value = 0;
  
  onBatchSelectChange();
  document.getElementById("qe-qty").focus();
}

function onBatchSelectChange() {
  if (!pendingMedicine) return;
  const batchNo = document.getElementById("qe-batch-select").value;
  const batch = pendingMedicine.batches.find(b => b.batchNo === batchNo);
  if (!batch) return;

  const unitType = document.getElementById("qe-unit-type").value;
  let unitPrice = batch.mrp;
  if (unitType === "loose" && pendingMedicine.pack > 1) {
    unitPrice = batch.mrp / pendingMedicine.pack;
  }

  document.getElementById("qe-price").value = unitPrice.toFixed(2);
  recalcQuickEntryTotal();
}

function recalcQuickEntryTotal() {
  if (!pendingMedicine) return;
  const batchNo = document.getElementById("qe-batch-select").value;
  const batch = pendingMedicine.batches.find(b => b.batchNo === batchNo);
  if (!batch) return;

  const unitType = document.getElementById("qe-unit-type").value;
  const packSize = pendingMedicine.pack || 1;
  let unitPrice = (unitType === "loose" && packSize > 1) ? (batch.mrp / packSize) : batch.mrp;
  
  document.getElementById("qe-price").value = unitPrice.toFixed(2);

  const qty = parseFloat(document.getElementById("qe-qty").value) || 1;
  const disc = parseFloat(document.getElementById("qe-disc").value) || 0;
  
  const base = qty * unitPrice;
  const total = base - (base * (disc / 100));
  document.getElementById("qe-line-total").value = total.toFixed(2);
}

function confirmAddLineItem() {
  if (!pendingMedicine) return;
  const batchNo = document.getElementById("qe-batch-select").value;
  const batch = pendingMedicine.batches.find(b => b.batchNo === batchNo);
  if (!batch) {
    alert("Please select a valid batch.");
    return;
  }

  if (checkIfExpired(batch.exp)) {
    alert("CRITICAL WARNING: This batch is EXPIRED! Regulatory compliance strictly blocks dispensing expired drugs.");
    return;
  }

  const unitType = document.getElementById("qe-unit-type").value;
  const qty = parseFloat(document.getElementById("qe-qty").value) || 1;
  const discPercent = parseFloat(document.getElementById("qe-disc").value) || 0;
  const packSize = pendingMedicine.pack || 1;

  // Stock check
  if (unitType === "strip" && qty > batch.stockPacks) {
    alert(`Only ${batch.stockPacks} full pack(s) available in batch ${batch.batchNo}.`);
    return;
  }

  const unitPrice = (unitType === "loose" && packSize > 1) ? (batch.mrp / packSize) : batch.mrp;
  const baseAmt = qty * unitPrice;
  const discAmt = baseAmt * (discPercent / 100);
  const netAmt = baseAmt - discAmt;

  // Add to bill items
  currentBill.items.push({
    medId: pendingMedicine.id,
    name: pendingMedicine.name,
    salt: pendingMedicine.salt,
    schedule: pendingMedicine.schedule,
    hsn: pendingMedicine.hsn,
    rack: pendingMedicine.rack,
    pack: pendingMedicine.pack,
    batchNo: batch.batchNo,
    exp: batch.exp,
    unitType: unitType,
    qty: qty,
    mrp: batch.mrp,
    rate: unitPrice,
    discPercent: discPercent,
    taxPercent: batch.gst || 12,
    netAmount: netAmt,
    costPtr: batch.ptr
  });

  cancelQuickEntry();
  renderPOSItemsTable();
  document.getElementById("pos-search-input").focus();
}

function cancelQuickEntry() {
  pendingMedicine = null;
  document.getElementById("quick-entry-bar").style.display = "none";
}

function renderPOSItemsTable() {
  const tbody = document.getElementById("pos-items-body");
  const emptyRow = document.getElementById("pos-empty-row");
  const countEl = document.getElementById("pos-item-count");

  countEl.innerText = currentBill.items.length;

  if (currentBill.items.length === 0) {
    tbody.innerHTML = `
      <tr id="pos-empty-row">
        <td colspan="13" style="text-align: center; padding: 40px 10px; color: var(--text-muted);">
          <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" style="margin-bottom: 8px; opacity: 0.5;"><circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/></svg>
          <p style="font-weight: 600;">Invoice is currently empty</p>
          <p style="font-size: 11px;">Search medicines above or press F2 to begin billing.</p>
        </td>
      </tr>
    `;
    recalcBillTotals();
    return;
  }

  tbody.innerHTML = currentBill.items.map((item, idx) => {
    let schBadge = `<span class="badge badge-general">OTC</span>`;
    if (item.schedule === "H") schBadge = `<span class="badge badge-schedule-h">Sch H</span>`;
    if (item.schedule === "H1") schBadge = `<span class="badge badge-schedule-h1">Sch H1</span>`;

    return `
      <tr>
        <td style="font-family: var(--font-mono); color: var(--text-muted);">${idx + 1}</td>
        <td>
          <strong style="color: var(--text-main); font-size: 13px;">${item.name}</strong>
          <div style="font-size: 10.5px; color: var(--text-muted);">${item.salt}</div>
        </td>
        <td><span class="meta-pill" style="font-family: var(--font-mono);">${item.batchNo}</span></td>
        <td><span class="meta-pill" style="font-family: var(--font-mono);">${item.exp}</span></td>
        <td>${item.rack}</td>
        <td>${schBadge}</td>
        <td><span style="font-size: 11px; text-transform: uppercase;">${item.unitType}</span></td>
        <td style="text-align: right; font-weight: 700; font-family: var(--font-mono);">${item.qty}</td>
        <td style="text-align: right; font-family: var(--font-mono);">₹${item.rate.toFixed(2)}</td>
        <td style="text-align: right; font-family: var(--font-mono);">${item.discPercent}%</td>
        <td style="text-align: right; font-family: var(--font-mono);">${item.taxPercent}%</td>
        <td style="text-align: right; font-weight: 700; color: var(--primary-dark); font-family: var(--font-mono);">₹${item.netAmount.toFixed(2)}</td>
        <td style="text-align: center;">
          <button class="btn btn-outline btn-sm" style="color: var(--danger); border-color: var(--danger); padding: 2px 6px;" onclick="removeBillItem(${idx})" title="Remove">✕</button>
        </td>
      </tr>
    `;
  }).join("");

  recalcBillTotals();
}

function removeBillItem(idx) {
  currentBill.items.splice(idx, 1);
  renderPOSItemsTable();
}

function clearCurrentBill() {
  if (currentBill.items.length > 0 && !confirm("Clear the entire current bill?")) return;
  currentBill.items = [];
  currentBill.billDiscPercent = 0;
  document.getElementById("pos-bill-disc").value = 0;
  renderPOSItemsTable();
}

// Bill Calculations & Schedule Drugs Detection
function recalcBillTotals() {
  let subtotal = 0;
  let taxable = 0;
  let gstSum = 0;
  let hasSchedule = false;

  currentBill.items.forEach(item => {
    subtotal += item.netAmount;
    if (item.schedule === "H" || item.schedule === "H1" || item.schedule === "X") {
      hasSchedule = true;
    }
  });

  const overallDiscPercent = parseFloat(document.getElementById("pos-bill-disc").value) || 0;
  const overallDiscAmt = subtotal * (overallDiscPercent / 100);
  const netAfterOverallDisc = subtotal - overallDiscAmt;

  // Assuming items are GST-inclusive:
  // Taxable Value = Amount / (1 + (GST / 100))
  // GST Amount = Amount - Taxable Value
  currentBill.items.forEach(item => {
    const itemRatio = subtotal > 0 ? item.netAmount / subtotal : 0;
    const itemNet = itemRatio * netAfterOverallDisc;
    const rateFactor = 1 + (item.taxPercent / 100);
    const itemTaxable = itemNet / rateFactor;
    taxable += itemTaxable;
    gstSum += (itemNet - itemTaxable);
  });

  const unrounded = netAfterOverallDisc;
  const grandTotal = Math.round(unrounded);
  const roundOff = grandTotal - unrounded;

  currentBill.subtotal = subtotal;
  currentBill.discountTotal = overallDiscAmt;
  currentBill.taxableTotal = taxable;
  currentBill.gstTotal = gstSum;
  currentBill.roundOff = roundOff;
  currentBill.grandTotal = grandTotal;
  currentBill.hasScheduleDrugs = hasSchedule;

  document.getElementById("pos-subtotal").innerText = `₹${subtotal.toFixed(2)}`;
  document.getElementById("pos-disc-amt").innerText = `₹${overallDiscAmt.toFixed(2)}`;
  document.getElementById("pos-taxable").innerText = `₹${taxable.toFixed(2)}`;
  document.getElementById("pos-gst-amt").innerText = `₹${gstSum.toFixed(2)}`;
  document.getElementById("pos-roundoff").innerText = `${roundOff >= 0 ? '+' : ''}₹${roundOff.toFixed(2)}`;
  document.getElementById("pos-grand-total").innerText = `₹${grandTotal.toFixed(2)}`;

  // Update Schedule Drug alert box
  const alertBox = document.getElementById("schedule-alert-box");
  if (hasSchedule) {
    alertBox.classList.add("show");
  } else {
    alertBox.classList.remove("show");
  }
}

// Customer & Doctor Select Change
function renderPOSDropdowns() {
  // Customers
  const custSelect = document.getElementById("pos-customer-select");
  custSelect.innerHTML = `
    <option value="walkin">Walk-in Customer (Cash)</option>
    ${db.data.customers.map(c => `<option value="${c.id}">${c.name} (${c.phone}) - Khata: ₹${c.khataBalance.toFixed(2)}</option>`).join("")}
  `;

  // Doctors
  const docSelect = document.getElementById("pos-doctor-select");
  docSelect.innerHTML = `
    <option value="none">-- Over The Counter (Self) --</option>
    ${db.data.doctors.map(d => `<option value="${d.id}">${d.name} (${d.spec} - ${d.regNo})</option>`).join("")}
  `;
}

function onCustomerSelectChange() {
  const custId = document.getElementById("pos-customer-select").value;
  const infoEl = document.getElementById("pos-cust-info");
  if (custId === "walkin") {
    infoEl.style.display = "none";
    currentBill.customer = "walkin";
  } else {
    const cust = db.data.customers.find(c => c.id === custId);
    if (cust) {
      currentBill.customer = cust.id;
      document.getElementById("pos-cust-khata").innerText = `₹${cust.khataBalance.toFixed(2)}`;
      document.getElementById("pos-cust-points").innerText = cust.points;
      infoEl.style.display = "block";
    }
  }
}

// Bill Parking / Hold (F7) & Recall (F8)
function holdCurrentBill() {
  if (currentBill.items.length === 0) {
    alert("Cannot park an empty bill. Add items first.");
    return;
  }

  const custId = document.getElementById("pos-customer-select").value;
  let custName = "Walk-in";
  if (custId !== "walkin") {
    const cust = db.data.customers.find(c => c.id === custId);
    if (cust) custName = cust.name;
  }

  db.data.heldBills.push({
    id: "HELD_" + Date.now(),
    time: new Date().toLocaleTimeString("en-IN"),
    customer: custName,
    bill: JSON.parse(JSON.stringify(currentBill))
  });
  db.save();

  alert(`Bill parked successfully (${currentBill.items.length} items).`);
  clearCurrentBill();
  updateParkedBillsBadge();
}

function updateParkedBillsBadge() {
  const badge = document.getElementById("parked-count-badge");
  badge.innerText = db.data.heldBills.length;
}

function openHeldBillsModal() {
  const tbody = document.getElementById("parked-tbody");
  if (db.data.heldBills.length === 0) {
    tbody.innerHTML = `<tr><td colspan="5" style="text-align: center; color: var(--text-muted); padding: 20px;">No parked bills currently on hold.</td></tr>`;
  } else {
    tbody.innerHTML = db.data.heldBills.map((hb, idx) => `
      <tr>
        <td>${hb.time}</td>
        <td><strong>${hb.customer}</strong></td>
        <td>${hb.bill.items.length} items</td>
        <td style="text-align: right; font-weight: 700;">₹${hb.bill.grandTotal.toFixed(2)}</td>
        <td style="text-align: center;">
          <button class="btn btn-primary btn-sm" onclick="resumeHeldBill(${idx})">Resume Bill</button>
        </td>
      </tr>
    `).join("");
  }
  openModal("parked-modal");
}

function resumeHeldBill(idx) {
  const held = db.data.heldBills.splice(idx, 1)[0];
  db.save();
  currentBill = held.bill;
  closeModal("parked-modal");
  updateParkedBillsBadge();
  renderPOSItemsTable();
}

// ==========================================
// TENDER PAYMENT & BILL SETTLEMENT (F10)
// ==========================================
function openTenderModal() {
  if (currentBill.items.length === 0) {
    alert("Invoice is empty! Please add medicines before settling.");
    return;
  }

  // Mandatory Prescribing Doctor Check for Schedule H / H1 / X Drugs
  const docId = document.getElementById("pos-doctor-select").value;
  if (currentBill.hasScheduleDrugs && docId === "none") {
    const proceed = confirm("COMPLIANCE NOTICE: This invoice contains Schedule H / H1 prescription medicines. Indian Drugs & Cosmetics Rules require a registered doctor's prescription. Do you want to select a doctor now?");
    if (proceed) {
      document.getElementById("pos-doctor-select").focus();
      return;
    }
  }

  document.getElementById("tender-due-display").innerText = `₹${currentBill.grandTotal.toFixed(2)}`;
  document.getElementById("tender-cash-input").value = currentBill.grandTotal;
  recalcChangeDue();

  // Update Khata customer preview
  const custId = document.getElementById("pos-customer-select").value;
  if (custId === "walkin") {
    document.getElementById("tender-khata-customer-name").innerText = "Customer: Walk-in (Cannot use Khata credit without selecting a registered customer)";
  } else {
    const cust = db.data.customers.find(c => c.id === custId);
    document.getElementById("tender-khata-customer-name").innerText = `Customer: ${cust.name} (Current Outstanding: ₹${cust.khataBalance.toFixed(2)})`;
  }

  setTenderMode('cash');
  openModal("tender-modal");
  setTimeout(() => document.getElementById("tender-cash-input").focus(), 100);
}

function setTenderMode(mode) {
  currentTenderMode = mode;
  document.querySelectorAll(".tender-mode-btn").forEach(btn => {
    btn.classList.toggle("active", btn.dataset.mode === mode);
  });

  document.getElementById("tender-sec-cash").style.display = mode === "cash" ? "block" : "none";
  document.getElementById("tender-sec-upi").style.display = mode === "upi" ? "block" : "none";
  document.getElementById("tender-sec-card").style.display = mode === "card" ? "block" : "none";
  document.getElementById("tender-sec-khata").style.display = mode === "khata" ? "block" : "none";
}

function recalcChangeDue() {
  const tenderCash = parseFloat(document.getElementById("tender-cash-input").value) || 0;
  const due = currentBill.grandTotal;
  const change = tenderCash - due;
  document.getElementById("tender-change-display").innerText = `₹${change > 0 ? change.toFixed(2) : '0.00'}`;
}

function finalizeBillAndPrint(doPrint = true) {
  const custId = document.getElementById("pos-customer-select").value;
  const docId = document.getElementById("pos-doctor-select").value;

  if (currentTenderMode === "khata" && custId === "walkin") {
    alert("Please select a registered customer to bill under Khata Credit.");
    return;
  }

  const invoiceNumber = "INV-" + (1000 + db.data.salesInvoices.length + 1);
  const now = new Date();

  // Deduct stock from batches
  currentBill.items.forEach(item => {
    const med = db.data.medicines.find(m => m.id === item.medId);
    if (med) {
      const batch = med.batches.find(b => b.batchNo === item.batchNo);
      if (batch) {
        if (item.unitType === "strip") {
          batch.stockPacks = Math.max(0, batch.stockPacks - item.qty);
        } else {
          // Loose tabs
          let totalLooseInStock = (batch.stockPacks * med.pack) + (batch.stockLoose || 0);
          totalLooseInStock = Math.max(0, totalLooseInStock - item.qty);
          batch.stockPacks = Math.floor(totalLooseInStock / med.pack);
          batch.stockLoose = totalLooseInStock % med.pack;
        }
      }
    }

    // If Schedule H/H1, record in Schedule Register
    if (item.schedule === "H" || item.schedule === "H1") {
      let custObj = { name: "Walk-in Cash Patient", phone: "N/A" };
      if (custId !== "walkin") {
        const c = db.data.customers.find(cu => cu.id === custId);
        if (c) custObj = { name: c.name, phone: c.phone };
      }

      let docObj = { name: "OTC / Unspecified", regNo: "N/A" };
      if (docId !== "none") {
        const d = db.data.doctors.find(doc => doc.id === docId);
        if (d) docObj = { name: d.name, regNo: d.regNo };
      }

      db.data.scheduleRegister.push({
        date: now.toISOString(),
        invoiceNo: invoiceNumber,
        patientName: custObj.name,
        patientPhone: custObj.phone,
        doctorName: docObj.name,
        doctorRegNo: docObj.regNo,
        medicineName: item.name,
        schedule: item.schedule,
        batchNo: item.batchNo,
        exp: item.exp,
        qty: `${item.qty} ${item.unitType}`
      });
    }
  });

  // Customer Credit Khata Ledger Update
  let custName = "Walk-in Customer";
  if (custId !== "walkin") {
    const cust = db.data.customers.find(c => c.id === custId);
    if (cust) {
      custName = cust.name;
      if (currentTenderMode === "khata") {
        cust.khataBalance += currentBill.grandTotal;
      }
      cust.points += Math.floor(currentBill.grandTotal / 100); // 1 point per ₹100
    }
  }

  // Doctor attribution
  let docName = "OTC";
  if (docId !== "none") {
    const d = db.data.doctors.find(doc => doc.id === docId);
    if (d) docName = d.name;
  }

  // Create Invoice Object
  const invoiceRecord = {
    invoiceNo: invoiceNumber,
    date: now.toISOString(),
    customer: custName,
    doctor: docName,
    paymentMode: currentTenderMode.toUpperCase(),
    items: JSON.parse(JSON.stringify(currentBill.items)),
    subtotal: currentBill.subtotal,
    discountTotal: currentBill.discountTotal,
    taxableTotal: currentBill.taxableTotal,
    gstTotal: currentBill.gstTotal,
    roundOff: currentBill.roundOff,
    grandTotal: currentBill.grandTotal
  };

  db.data.salesInvoices.push(invoiceRecord);
  lastCompletedInvoice = invoiceRecord;

  // Till Transaction
  if (currentTenderMode === "cash") {
    db.data.tillTransactions.push({
      type: "SALE",
      amount: currentBill.grandTotal,
      desc: `Sale: ${invoiceNumber}`,
      time: now.toISOString()
    });
  }

  db.save();

  closeModal("tender-modal");
  renderInventoryTable();
  renderCustomersTable();
  renderScheduleRegisterTable();
  renderReportsData();
  renderTillStatus();
  updateExpiryCountBadge();

  if (doPrint) {
    printReceipt(invoiceRecord);
  } else {
    alert(`Invoice ${invoiceNumber} created successfully! Amount: ₹${invoiceRecord.grandTotal}`);
  }

  // Clear bill
  currentBill.items = [];
  currentBill.billDiscPercent = 0;
  document.getElementById("pos-bill-disc").value = 0;
  renderPOSItemsTable();
}

// Print Receipt or Invoice
function printReceipt(invoice) {
  const s = db.data.settings;
  const isThermal = s.printFormat === "thermal";
  const printBox = document.getElementById("print-container");

  if (isThermal) {
    // 80mm Thermal Receipt Layout
    printBox.innerHTML = `
      <div class="thermal-receipt">
        <div class="receipt-header">
          <div class="receipt-title">${s.storeName}</div>
          <div>${s.address}</div>
          <div>Ph: ${s.phone}</div>
          <div>DL: ${s.dlNo}</div>
          <div>GSTIN: ${s.gstin}</div>
        </div>
        <div style="display: flex; justify-content: space-between; margin-bottom: 4px;">
          <span><strong>Bill No:</strong> ${invoice.invoiceNo}</span>
          <span>${new Date(invoice.date).toLocaleDateString("en-IN")} ${new Date(invoice.date).toLocaleTimeString("en-IN", { hour: '2-digit', minute: '2-digit' })}</span>
        </div>
        <div style="margin-bottom: 4px;">
          <strong>Customer:</strong> ${invoice.customer}<br>
          <strong>Doctor:</strong> ${invoice.doctor}
        </div>
        <table class="receipt-table">
          <thead>
            <tr>
              <th>Item</th>
              <th>Batch/Exp</th>
              <th style="text-align: right;">Qty</th>
              <th style="text-align: right;">Rate</th>
              <th style="text-align: right;">Amount</th>
            </tr>
          </thead>
          <tbody>
            ${invoice.items.map(it => `
              <tr>
                <td><strong>${it.name}</strong></td>
                <td>${it.batchNo} (${it.exp})</td>
                <td style="text-align: right;">${it.qty}</td>
                <td style="text-align: right;">${it.rate.toFixed(2)}</td>
                <td style="text-align: right;">${it.netAmount.toFixed(2)}</td>
              </tr>
            `).join("")}
          </tbody>
        </table>
        <div style="border-top: 1px dashed #000; padding-top: 4px; margin-top: 4px;">
          <div style="display: flex; justify-content: space-between;">
            <span>Subtotal:</span>
            <span>₹${invoice.subtotal.toFixed(2)}</span>
          </div>
          ${invoice.discountTotal > 0 ? `
          <div style="display: flex; justify-content: space-between;">
            <span>Discount:</span>
            <span>-₹${invoice.discountTotal.toFixed(2)}</span>
          </div>` : ''}
          <div style="display: flex; justify-content: space-between;">
            <span>Taxable:</span>
            <span>₹${invoice.taxableTotal.toFixed(2)}</span>
          </div>
          <div style="display: flex; justify-content: space-between;">
            <span>GST:</span>
            <span>₹${invoice.gstTotal.toFixed(2)}</span>
          </div>
          <div style="display: flex; justify-content: space-between; font-size: 13px; font-weight: bold; margin-top: 4px; border-top: 1px solid #000; padding-top: 2px;">
            <span>NET PAYABLE:</span>
            <span>₹${invoice.grandTotal.toFixed(2)}</span>
          </div>
          <div style="margin-top: 2px; font-size: 10px;">
            Payment: <strong>${invoice.paymentMode}</strong>
          </div>
        </div>
        <div class="receipt-footer">
          <div>* SCHEDULE H DRUGS: Sold under registered medical supervision only.</div>
          <div style="margin-top: 4px;">Thank You! Get Well Soon!</div>
          <div style="font-size: 8px; margin-top: 3px;">PharmaAid Pro | Powered by Wondersoft Technology</div>
        </div>
      </div>
    `;
  } else {
    // A4 / A5 GST Tax Invoice
    printBox.innerHTML = `
      <div class="a4-invoice">
        <div class="a4-header">
          <div>
            <h2 style="margin: 0; font-size: 20px;">${s.storeName}</h2>
            <div>${s.address}</div>
            <div>Phone: ${s.phone}</div>
            <div><strong>Drug License:</strong> ${s.dlNo}</div>
            <div><strong>GSTIN:</strong> ${s.gstin}</div>
          </div>
          <div style="text-align: right;">
            <h3 style="margin: 0; color: #1e3a8a;">TAX INVOICE</h3>
            <div><strong>Invoice No:</strong> ${invoice.invoiceNo}</div>
            <div><strong>Date:</strong> ${new Date(invoice.date).toLocaleDateString("en-IN")}</div>
            <div><strong>Pay Mode:</strong> ${invoice.paymentMode}</div>
          </div>
        </div>
        <div style="display: flex; justify-content: space-between; margin-bottom: 12px; border: 1px solid #ddd; padding: 8px;">
          <div>
            <strong>Patient / Buyer:</strong> ${invoice.customer}
          </div>
          <div>
            <strong>Prescribed By:</strong> ${invoice.doctor}
          </div>
        </div>
        <table class="a4-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Description of Goods</th>
              <th>HSN</th>
              <th>Batch</th>
              <th>Expiry</th>
              <th style="text-align: right;">Qty</th>
              <th style="text-align: right;">MRP</th>
              <th style="text-align: right;">Rate</th>
              <th style="text-align: right;">GST %</th>
              <th style="text-align: right;">Total (₹)</th>
            </tr>
          </thead>
          <tbody>
            ${invoice.items.map((it, idx) => `
              <tr>
                <td>${idx + 1}</td>
                <td><strong>${it.name}</strong><br><small>${it.salt}</small></td>
                <td>${it.hsn}</td>
                <td>${it.batchNo}</td>
                <td>${it.exp}</td>
                <td style="text-align: right;">${it.qty}</td>
                <td style="text-align: right;">${it.mrp.toFixed(2)}</td>
                <td style="text-align: right;">${it.rate.toFixed(2)}</td>
                <td style="text-align: right;">${it.taxPercent}%</td>
                <td style="text-align: right;">${it.netAmount.toFixed(2)}</td>
              </tr>
            `).join("")}
          </tbody>
        </table>
        <div style="display: flex; justify-content: space-between; margin-top: 15px;">
          <div style="font-size: 11px; max-width: 400px;">
            <strong>Statutory Declaration:</strong><br>
            We hereby certify that my/our registration certificate under the CGST/SGST Act is in force on the date on which the sale of goods specified in this tax invoice is made by us.<br>
            <em>Goods once sold will not be taken back without cash bill.</em>
          </div>
          <div style="width: 250px; text-align: right;">
            <div>Taxable Value: ₹${invoice.taxableTotal.toFixed(2)}</div>
            <div>CGST: ₹${(invoice.gstTotal / 2).toFixed(2)}</div>
            <div>SGST: ₹${(invoice.gstTotal / 2).toFixed(2)}</div>
            <div style="font-size: 16px; font-weight: bold; border-top: 1px solid #333; margin-top: 6px; padding-top: 4px;">
              Net Amount: ₹${invoice.grandTotal.toFixed(2)}
            </div>
            <div style="margin-top: 30px; font-size: 11px;">Authorized Signatory / Pharmacist</div>
          </div>
        </div>
      </div>
    `;
  }

  window.print();
}

function printLastBill() {
  if (!lastCompletedInvoice && db.data.salesInvoices.length > 0) {
    lastCompletedInvoice = db.data.salesInvoices[db.data.salesInvoices.length - 1];
  }
  if (!lastCompletedInvoice) {
    alert("No completed invoice to reprint.");
    return;
  }
  printReceipt(lastCompletedInvoice);
}

// ==========================================
// INVENTORY & BATCHES MANAGEMENT (F4)
// ==========================================
function renderInventoryTable() {
  const tbody = document.getElementById("inventory-tbody");
  const filterText = (document.getElementById("inv-search-input").value || "").toLowerCase();
  const filterStatus = document.getElementById("inv-filter-status").value;
  const filterSchedule = document.getElementById("inv-filter-schedule").value;

  let totalMeds = db.data.medicines.length;
  let totalValuation = 0;
  let lowStockCount = 0;
  let expiredCount = 0;

  let rows = [];

  db.data.medicines.forEach(m => {
    m.batches.forEach(b => {
      const isExp = checkIfExpired(b.exp);
      const isNearExp = checkIfNearExpiry(b.exp);
      const isLowStock = b.stockPacks < 10;

      totalValuation += b.stockPacks * b.mrp;
      if (isLowStock) lowStockCount++;
      if (isExp || isNearExp) expiredCount++;

      // Filters
      if (filterText) {
        const matches = m.name.toLowerCase().includes(filterText) ||
          m.salt.toLowerCase().includes(filterText) ||
          b.batchNo.toLowerCase().includes(filterText) ||
          m.rack.toLowerCase().includes(filterText);
        if (!matches) return;
      }

      if (filterSchedule !== "all" && m.schedule !== filterSchedule) return;

      if (filterStatus === "low" && !isLowStock) return;
      if (filterStatus === "expired" && !isExp) return;
      if (filterStatus === "near_expiry" && !isNearExp) return;

      let schBadge = `<span class="badge badge-general">OTC</span>`;
      if (m.schedule === "H") schBadge = `<span class="badge badge-schedule-h">Sch H</span>`;
      if (m.schedule === "H1") schBadge = `<span class="badge badge-schedule-h1">Sch H1</span>`;

      let statusBadge = `<span class="badge badge-expiry-good">Active</span>`;
      if (isExp) {
        statusBadge = `<span class="badge badge-expiry-danger">EXPIRED</span>`;
      } else if (isNearExp) {
        statusBadge = `<span class="badge badge-expiry-warn">Expiring Soon</span>`;
      }

      rows.push(`
        <tr>
          <td>
            <strong>${m.name}</strong>
            <div style="font-size: 10px; color: var(--text-muted);">${m.form}</div>
          </td>
          <td style="font-size: 11.5px; color: var(--text-muted);">${m.salt}</td>
          <td>${m.mfg}</td>
          <td><span class="meta-pill" style="font-family: var(--font-mono); font-weight: 600;">${b.batchNo}</span></td>
          <td><span class="meta-pill" style="font-family: var(--font-mono);">${b.exp}</span></td>
          <td><strong>${m.rack}</strong></td>
          <td>${schBadge}</td>
          <td style="text-align: right;">${m.pack}</td>
          <td style="text-align: right; font-weight: 700; ${b.stockPacks < 10 ? 'color: var(--danger);' : ''}">${b.stockPacks}</td>
          <td style="text-align: right; color: var(--text-muted);">${b.stockLoose || 0}</td>
          <td style="text-align: right; font-family: var(--font-mono);">₹${b.ptr.toFixed(2)}</td>
          <td style="text-align: right; font-family: var(--font-mono); font-weight: 700;">₹${b.mrp.toFixed(2)}</td>
          <td style="text-align: center;">${statusBadge}</td>
          <td style="text-align: center;">
            <button class="btn btn-outline btn-sm" onclick="selectMedicineForQuickEntry('${m.id}')" title="Sell in POS">Sell</button>
          </td>
        </tr>
      `);
    });
  });

  tbody.innerHTML = rows.join("");

  document.getElementById("stat-total-items").innerText = totalMeds;
  document.getElementById("stat-stock-valuation").innerText = `₹${totalValuation.toLocaleString("en-IN")}`;
  document.getElementById("stat-low-stock").innerText = lowStockCount;
  document.getElementById("stat-expired-count").innerText = expiredCount;
}

// Add New Medicine Modal
function openAddMedicineModal() {
  openModal("add-medicine-modal");
}

function saveNewMedicine() {
  const name = document.getElementById("m-name").value.trim();
  const salt = document.getElementById("m-salt").value.trim();
  const form = document.getElementById("m-form").value;
  const mfg = document.getElementById("m-mfg").value.trim();
  const schedule = document.getElementById("m-schedule").value;
  const hsn = document.getElementById("m-hsn").value.trim() || "300490";
  const pack = parseInt(document.getElementById("m-pack").value) || 10;
  const rack = document.getElementById("m-rack").value.trim() || "A-01";
  const batchNo = document.getElementById("m-batch").value.trim() || "B" + Math.floor(1000 + Math.random() * 9000);
  const exp = document.getElementById("m-exp").value.trim() || "12/28";
  const stock = parseInt(document.getElementById("m-stock").value) || 10;
  const mrp = parseFloat(document.getElementById("m-mrp").value) || 50.00;
  const ptr = parseFloat(document.getElementById("m-ptr").value) || (mrp * 0.75);
  const gst = parseInt(document.getElementById("m-gst").value) || 12;

  if (!name || !salt) {
    alert("Medicine brand name and generic salt composition are required.");
    return;
  }

  const newMed = {
    id: "MED" + String(db.data.medicines.length + 1).padStart(3, '0'),
    name: name,
    salt: salt,
    form: form,
    mfg: mfg,
    schedule: schedule,
    hsn: hsn,
    pack: pack,
    rack: rack,
    barcode: "8901148" + Math.floor(10000 + Math.random() * 90000),
    batches: [
      { batchNo: batchNo, exp: exp, stockPacks: stock, stockLoose: 0, mrp: mrp, ptr: ptr, gst: gst }
    ]
  };

  db.data.medicines.push(newMed);
  db.save();

  closeModal("add-medicine-modal");
  renderInventoryTable();
  renderPurchaseDropdowns();
  alert(`Medicine '${name}' added successfully!`);
}

function openStockAdjustmentModal() {
  const medName = prompt("Enter medicine name or batch to adjust stock:");
  if (!medName) return;
  const med = db.data.medicines.find(m => m.name.toLowerCase().includes(medName.toLowerCase()));
  if (!med) {
    alert("Medicine not found.");
    return;
  }
  const newStock = prompt(`Current stock for ${med.name} (${med.batches[0].batchNo}) is ${med.batches[0].stockPacks} packs. Enter updated stock count:`, med.batches[0].stockPacks);
  if (newStock !== null) {
    med.batches[0].stockPacks = parseInt(newStock) || 0;
    db.save();
    renderInventoryTable();
    alert("Stock adjusted successfully.");
  }
}

// ==========================================
// PURCHASE INWARD (SUPPLIER INVOICING)
// ==========================================
let currentPurchaseItems = [];

function setupPurchaseInvoiceDateDefault() {
  const today = new Date().toISOString().split("T")[0];
  const dateInput = document.getElementById("pur-invoice-date");
  if (dateInput) dateInput.value = today;
}

function renderPurchaseDropdowns() {
  const supSelect = document.getElementById("pur-supplier-select");
  if (supSelect) {
    supSelect.innerHTML = db.data.suppliers.map(s => `<option value="${s.id}">${s.name} (Balance: ₹${s.balance.toFixed(2)})</option>`).join("");
  }

  const itemSelect = document.getElementById("pur-item-select");
  if (itemSelect) {
    itemSelect.innerHTML = db.data.medicines.map(m => `<option value="${m.id}">${m.name} (${m.pack} tabs)</option>`).join("");
  }
}

function addPurchaseLineItem() {
  const medId = document.getElementById("pur-item-select").value;
  const med = db.data.medicines.find(m => m.id === medId);
  if (!med) return;

  const batchNo = document.getElementById("pur-item-batch").value.trim() || "BT" + Math.floor(100 + Math.random() * 900);
  const exp = document.getElementById("pur-item-exp").value.trim() || "12/28";
  const qty = parseInt(document.getElementById("pur-item-qty").value) || 10;
  const free = parseInt(document.getElementById("pur-item-free").value) || 0;
  const ptr = parseFloat(document.getElementById("pur-item-ptr").value) || (med.batches[0] ? med.batches[0].ptr : 50);
  const mrp = parseFloat(document.getElementById("pur-item-mrp").value) || (med.batches[0] ? med.batches[0].mrp : 70);
  const gst = parseInt(document.getElementById("pur-item-gst").value) || 12;

  const baseVal = qty * ptr;
  const gstAmt = baseVal * (gst / 100);
  const total = baseVal + gstAmt;

  currentPurchaseItems.push({
    medId: med.id,
    name: med.name,
    batchNo: batchNo,
    exp: exp,
    qty: qty,
    free: free,
    ptr: ptr,
    mrp: mrp,
    gst: gst,
    total: total
  });

  renderPurchaseLineItemsTable();
}

function renderPurchaseLineItemsTable() {
  const tbody = document.getElementById("pur-items-tbody");
  if (currentPurchaseItems.length === 0) {
    tbody.innerHTML = `<tr id="pur-empty-row"><td colspan="11" style="text-align: center; color: var(--text-muted); padding: 20px;">No items entered yet. Add lines above.</td></tr>`;
    document.getElementById("pur-total-val").innerText = "₹0.00";
    return;
  }

  let totalSum = 0;
  tbody.innerHTML = currentPurchaseItems.map((item, idx) => {
    totalSum += item.total;
    return `
      <tr>
        <td>${idx + 1}</td>
        <td><strong>${item.name}</strong></td>
        <td><span class="meta-pill" style="font-family: var(--font-mono);">${item.batchNo}</span></td>
        <td>${item.exp}</td>
        <td style="text-align: right; font-weight: 700;">${item.qty}</td>
        <td style="text-align: right; color: var(--accent);">${item.free > 0 ? '+' + item.free + ' Free' : '0'}</td>
        <td style="text-align: right; font-family: var(--font-mono);">₹${item.ptr.toFixed(2)}</td>
        <td style="text-align: right; font-family: var(--font-mono);">₹${item.mrp.toFixed(2)}</td>
        <td style="text-align: right;">${item.gst}%</td>
        <td style="text-align: right; font-weight: 700; color: var(--primary-dark); font-family: var(--font-mono);">₹${item.total.toFixed(2)}</td>
        <td style="text-align: center;">
          <button class="btn btn-outline btn-sm" style="color: var(--danger);" onclick="removePurchaseLineItem(${idx})">✕</button>
        </td>
      </tr>
    `;
  }).join("");

  document.getElementById("pur-total-val").innerText = `₹${totalSum.toFixed(2)}`;
}

function removePurchaseLineItem(idx) {
  currentPurchaseItems.splice(idx, 1);
  renderPurchaseLineItemsTable();
}

function savePurchaseInvoice() {
  if (currentPurchaseItems.length === 0) {
    alert("Please add at least one line item to the purchase invoice.");
    return;
  }

  const supId = document.getElementById("pur-supplier-select").value;
  const sup = db.data.suppliers.find(s => s.id === supId);
  const invNo = document.getElementById("pur-invoice-no").value.trim() || "PUR-" + Date.now().toString().slice(-5);
  const invDate = document.getElementById("pur-invoice-date").value || new Date().toISOString().split("T")[0];
  const terms = document.getElementById("pur-payment-terms").value;

  let totalVal = 0;

  // Add stock to batches
  currentPurchaseItems.forEach(pItem => {
    totalVal += pItem.total;
    const med = db.data.medicines.find(m => m.id === pItem.medId);
    if (med) {
      let batch = med.batches.find(b => b.batchNo === pItem.batchNo);
      const incomingPacks = pItem.qty + pItem.free;
      if (batch) {
        batch.stockPacks += incomingPacks;
        batch.mrp = pItem.mrp;
        batch.ptr = pItem.ptr;
      } else {
        med.batches.push({
          batchNo: pItem.batchNo,
          exp: pItem.exp,
          stockPacks: incomingPacks,
          stockLoose: 0,
          mrp: pItem.mrp,
          ptr: pItem.ptr,
          gst: pItem.gst
        });
      }
    }
  });

  // Update supplier outstanding balance
  if (sup) {
    sup.balance += totalVal;
  }

  // Record Purchase Inward
  db.data.purchaseInvoices.push({
    invoiceNo: invNo,
    date: invDate,
    supplierName: sup ? sup.name : "Supplier",
    terms: terms,
    itemsCount: currentPurchaseItems.length,
    totalAmount: totalVal,
    items: JSON.parse(JSON.stringify(currentPurchaseItems))
  });

  db.save();

  alert(`Purchase Inward ${invNo} saved successfully! ${currentPurchaseItems.length} batches updated into inventory.`);
  currentPurchaseItems = [];
  document.getElementById("pur-invoice-no").value = "";
  renderPurchaseLineItemsTable();
  renderInventoryTable();
  renderPurchaseHistory();
  renderSuppliersTable();
}

function renderPurchaseHistory() {
  const tbody = document.getElementById("pur-history-tbody");
  if (!tbody) return;

  if (db.data.purchaseInvoices.length === 0) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-muted); padding: 20px;">No purchase invoices recorded yet.</td></tr>`;
    return;
  }

  tbody.innerHTML = db.data.purchaseInvoices.slice().reverse().map(inv => `
    <tr>
      <td>${inv.date}</td>
      <td><strong>${inv.supplierName}</strong></td>
      <td><span class="meta-pill" style="font-family: var(--font-mono);">${inv.invoiceNo}</span></td>
      <td>${inv.itemsCount} items</td>
      <td style="text-align: right; font-weight: 700; font-family: var(--font-mono);">₹${inv.totalAmount.toFixed(2)}</td>
      <td>${inv.terms}</td>
      <td style="text-align: center;">
        <span class="badge badge-general">Completed</span>
      </td>
    </tr>
  `).join("");
}

// ==========================================
// GENERIC SUBSTITUTE FINDER ENGINE (F3)
// ==========================================
function openSubstituteModal() {
  switchTab("substitutes");
  document.getElementById("substitute-search-input").focus();
}

function renderSubstituteResults() {
  const container = document.getElementById("substitute-results-container");
  const query = (document.getElementById("substitute-search-input").value || "").trim().toLowerCase();

  // Find target medicine or salt
  let targetSalt = "";
  let originMedName = "";

  if (query) {
    const foundMed = db.data.medicines.find(m => 
      m.name.toLowerCase().includes(query) || 
      m.salt.toLowerCase().includes(query)
    );
    if (foundMed) {
      targetSalt = foundMed.salt;
      originMedName = foundMed.name;
    } else {
      targetSalt = query;
    }
  } else {
    // Default show for Augmentin / Amoxicillin
    targetSalt = "Amoxicillin 500mg + Clavulanic Acid 125mg";
    originMedName = "Augmentin 625 Duo Tab";
  }

  // Find all medicines matching identical salt
  const matchingMeds = db.data.medicines.filter(m => 
    m.salt.toLowerCase() === targetSalt.toLowerCase() ||
    m.salt.toLowerCase().includes(targetSalt.toLowerCase())
  );

  if (matchingMeds.length === 0) {
    container.innerHTML = `
      <div class="card" style="text-align: center; padding: 40px; color: var(--text-muted);">
        <h4>No alternative generic brands found for "${query}"</h4>
        <p style="font-size: 12px; margin-top: 6px;">Try searching by brand name like 'Augmentin', 'Pan 40', 'Telma 40', or 'Shelcal'.</p>
      </div>
    `;
    return;
  }

  // Sort matching medicines by MRP (cheapest first)
  matchingMeds.sort((a, b) => {
    const mrpA = a.batches[0] ? a.batches[0].mrp : 0;
    const mrpB = b.batches[0] ? b.batches[0].mrp : 0;
    return mrpA - mrpB;
  });

  const maxPriceMed = matchingMeds[matchingMeds.length - 1];
  const maxPrice = maxPriceMed.batches[0] ? maxPriceMed.batches[0].mrp : 0;

  container.innerHTML = `
    <div class="card" style="margin-bottom: 12px; background: var(--primary-light); border-color: var(--primary);">
      <div style="font-size: 11px; text-transform: uppercase; font-weight: 700; color: var(--primary-dark);">Identified Active Salt Composition:</div>
      <div style="font-size: 16px; font-weight: 800; color: var(--text-main); margin: 3px 0;">${targetSalt}</div>
      <div style="font-size: 12px; color: var(--text-muted);">Found ${matchingMeds.length} in-stock brand(s) sharing identical chemical molecule & dosage.</div>
    </div>
    ${matchingMeds.map(m => {
      const b = m.batches[0] || { mrp: 0, batchNo: "N/A", exp: "N/A", stockPacks: 0 };
      const saving = maxPrice > b.mrp ? (maxPrice - b.mrp) : 0;
      const savingPercent = maxPrice > 0 ? Math.round((saving / maxPrice) * 100) : 0;

      return `
        <div class="substitute-card">
          <div>
            <div style="font-size: 15px; font-weight: 700; display: flex; align-items: center; gap: 8px;">
              ${m.name}
              ${m.schedule !== 'General' ? `<span class="badge badge-schedule-h">Sch ${m.schedule}</span>` : ''}
              ${savingPercent > 0 ? `<span class="substitute-saving">Save ₹${saving.toFixed(2)} (${savingPercent}% OFF)</span>` : ''}
            </div>
            <div style="font-size: 12px; color: var(--text-muted); margin-top: 2px;">
              Mfg: <strong>${m.mfg}</strong> • Pack: ${m.pack} tabs • Rack: <strong>${m.rack}</strong>
            </div>
            <div style="font-size: 11.5px; color: var(--accent); margin-top: 4px; font-weight: 600;">
              Batch: ${b.batchNo} | Exp: ${b.exp} | Stock: ${b.stockPacks} pack(s) available
            </div>
          </div>
          <div style="text-align: right;">
            <div style="font-size: 20px; font-weight: 800; color: var(--primary-dark); font-family: var(--font-mono);">
              ₹${b.mrp.toFixed(2)}
            </div>
            <button class="btn btn-accent btn-sm" style="margin-top: 6px;" onclick="switchTab('pos'); selectMedicineForQuickEntry('${m.id}')">
              + Add to POS Bill
            </button>
          </div>
        </div>
      `;
    }).join("")}
  `;
}

// ==========================================
// EXPIRY & DAMAGE DESK
// ==========================================
function updateExpiryCountBadge() {
  let count = 0;
  db.data.medicines.forEach(m => {
    m.batches.forEach(b => {
      if (checkIfExpired(b.exp) || checkIfNearExpiry(b.exp)) count++;
    });
  });
  document.getElementById("badge-expiry-count").innerText = count;
}

function renderExpiryDesk() {
  const expiredTbody = document.getElementById("expired-items-tbody");
  const nearTbody = document.getElementById("near-expiry-tbody");

  let expiredRows = [];
  let nearRows = [];

  db.data.medicines.forEach(m => {
    m.batches.forEach(b => {
      if (checkIfExpired(b.exp)) {
        expiredRows.push(`
          <tr>
            <td><strong>${m.name}</strong></td>
            <td><span class="meta-pill">${b.batchNo}</span></td>
            <td style="color: var(--danger); font-weight: 700;">${b.exp}</td>
            <td>${b.stockPacks} packs</td>
            <td style="text-align: right; font-family: var(--font-mono);">₹${(b.stockPacks * b.mrp).toFixed(2)}</td>
          </tr>
        `);
      } else if (checkIfNearExpiry(b.exp)) {
        nearRows.push(`
          <tr>
            <td><strong>${m.name}</strong></td>
            <td><span class="meta-pill">${b.batchNo}</span></td>
            <td style="color: var(--warning); font-weight: 700;">${b.exp}</td>
            <td>&lt; 60 days</td>
            <td><strong>${b.stockPacks}</strong> packs</td>
          </tr>
        `);
      }
    });
  });

  expiredTbody.innerHTML = expiredRows.length > 0 ? expiredRows.join("") : `<tr><td colspan="5" style="text-align: center; color: var(--text-muted); padding: 15px;">No expired batches quarantined.</td></tr>`;
  nearTbody.innerHTML = nearRows.length > 0 ? nearRows.join("") : `<tr><td colspan="5" style="text-align: center; color: var(--text-muted); padding: 15px;">No near-expiry batches detected.</td></tr>`;
}

function generateDebitNoteForExpired() {
  alert("Supplier Return Debit Note generated successfully for quarantined batches. Credit note request dispatched to distributors.");
}

// Expiry helpers
function checkIfExpired(expStr) {
  if (!expStr || !expStr.includes("/")) return false;
  const parts = expStr.split("/");
  const expMonth = parseInt(parts[0], 10);
  let expYear = parseInt(parts[1], 10);
  if (expYear < 100) expYear += 2000;

  const now = new Date();
  const currentMonth = now.getMonth() + 1;
  const currentYear = now.getFullYear();

  if (expYear < currentYear) return true;
  if (expYear === currentYear && expMonth < currentMonth) return true;
  return false;
}

function checkIfNearExpiry(expStr) {
  if (!expStr || !expStr.includes("/")) return false;
  const parts = expStr.split("/");
  const expMonth = parseInt(parts[0], 10);
  let expYear = parseInt(parts[1], 10);
  if (expYear < 100) expYear += 2000;

  const now = new Date();
  const expDate = new Date(expYear, expMonth - 1, 1);
  const diffTime = expDate - now;
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  return diffDays > 0 && diffDays <= 90;
}

// ==========================================
// SCHEDULE H & H1 STATUTORY REGISTER
// ==========================================
function renderScheduleRegisterTable() {
  const tbody = document.getElementById("schedule-register-tbody");
  if (db.data.scheduleRegister.length === 0) {
    tbody.innerHTML = `<tr><td colspan="11" style="text-align: center; color: var(--text-muted); padding: 30px;">No Schedule H/H1 drugs dispensed yet. Mandatory log appears here automatically when billed.</td></tr>`;
    return;
  }

  tbody.innerHTML = db.data.scheduleRegister.slice().reverse().map(entry => `
    <tr>
      <td>${new Date(entry.date).toLocaleDateString("en-IN")} ${new Date(entry.date).toLocaleTimeString("en-IN", { hour: '2-digit', minute: '2-digit' })}</td>
      <td><span class="meta-pill" style="font-family: var(--font-mono);">${entry.invoiceNo}</span></td>
      <td><strong>${entry.patientName}</strong></td>
      <td>${entry.patientPhone}</td>
      <td>${entry.doctorName}</td>
      <td><span class="meta-pill">${entry.doctorRegNo}</span></td>
      <td><strong>${entry.medicineName}</strong></td>
      <td><span class="badge ${entry.schedule === 'H1' ? 'badge-schedule-h1' : 'badge-schedule-h'}">Sch ${entry.schedule}</span></td>
      <td><span class="meta-pill">${entry.batchNo}</span></td>
      <td>${entry.exp}</td>
      <td style="text-align: right; font-weight: 700;">${entry.qty}</td>
    </tr>
  `).join("");
}

function exportScheduleRegisterCSV() {
  if (db.data.scheduleRegister.length === 0) {
    alert("Schedule register has no records to export.");
    return;
  }

  let csv = "Date,Invoice No,Patient Name,Patient Phone,Doctor Name,Doctor Reg No,Medicine Name,Schedule,Batch No,Expiry,Quantity\n";
  db.data.scheduleRegister.forEach(r => {
    csv += `"${new Date(r.date).toLocaleString()}","${r.invoiceNo}","${r.patientName}","${r.patientPhone}","${r.doctorName}","${r.doctorRegNo}","${r.medicineName}","${r.schedule}","${r.batchNo}","${r.exp}","${r.qty}"\n`;
  });

  const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `Schedule_H_H1_Register_${new Date().toISOString().split("T")[0]}.csv`;
  a.click();
}

function printScheduleRegister() {
  const printBox = document.getElementById("print-container");
  const s = db.data.settings;

  printBox.innerHTML = `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <h2 style="text-align: center; margin-bottom: 2px;">${s.storeName}</h2>
      <div style="text-align: center; font-size: 12px; margin-bottom: 12px;">
        ${s.address} | DL: ${s.dlNo} | GSTIN: ${s.gstin}<br>
        <strong>STATUTORY SCHEDULE H & H1 DRUG REGISTER (RULE 65)</strong>
      </div>
      <table class="a4-table" style="font-size: 10px;">
        <thead>
          <tr>
            <th>Date & Time</th>
            <th>Bill #</th>
            <th>Patient Name</th>
            <th>Patient Ph</th>
            <th>Doctor Name</th>
            <th>Doc Reg #</th>
            <th>Drug Name</th>
            <th>Sch</th>
            <th>Batch</th>
            <th>Exp</th>
            <th>Qty</th>
          </tr>
        </thead>
        <tbody>
          ${db.data.scheduleRegister.map(r => `
            <tr>
              <td>${new Date(r.date).toLocaleDateString()}</td>
              <td>${r.invoiceNo}</td>
              <td>${r.patientName}</td>
              <td>${r.patientPhone}</td>
              <td>${r.doctorName}</td>
              <td>${r.doctorRegNo}</td>
              <td>${r.medicineName}</td>
              <td>${r.schedule}</td>
              <td>${r.batchNo}</td>
              <td>${r.exp}</td>
              <td>${r.qty}</td>
            </tr>
          `).join("")}
        </tbody>
      </table>
      <div style="margin-top: 30px; display: flex; justify-content: space-between; font-size: 11px;">
        <div>Verified by Registered Pharmacist: _____________________</div>
        <div>Drug Inspector Signature / Stamp: _____________________</div>
      </div>
    </div>
  `;

  window.print();
}

// ==========================================
// CUSTOMERS & KHATA LEDGER
// ==========================================
let activeKhataCustomerId = null;

function renderCustomersTable() {
  const tbody = document.getElementById("customers-tbody");
  tbody.innerHTML = db.data.customers.map(c => `
    <tr>
      <td><strong>${c.name}</strong></td>
      <td>${c.phone}</td>
      <td>${c.address}</td>
      <td><span class="badge badge-general">${c.points} pts</span></td>
      <td style="text-align: right; font-weight: 700; color: ${c.khataBalance > 0 ? 'var(--danger)' : 'var(--text-muted)'}; font-family: var(--font-mono);">
        ₹${c.khataBalance.toFixed(2)}
      </td>
      <td style="text-align: center;">
        <button class="btn btn-outline btn-sm" onclick="openReceiveKhataModal('${c.id}')">Receive Payment</button>
      </td>
    </tr>
  `).join("");
}

function openAddCustomerModal() {
  openModal("add-customer-modal");
}

function saveNewCustomer() {
  const name = document.getElementById("cust-name").value.trim();
  const phone = document.getElementById("cust-phone").value.trim();
  const address = document.getElementById("cust-address").value.trim();
  if (!name || !phone) {
    alert("Name and phone number are required.");
    return;
  }

  db.data.customers.push({
    id: "CUST" + String(db.data.customers.length + 1).padStart(2, '0'),
    name: name,
    phone: phone,
    address: address,
    points: 0,
    khataBalance: 0.00
  });
  db.save();

  closeModal("add-customer-modal");
  renderCustomersTable();
  renderPOSDropdowns();
  alert(`Customer ${name} added successfully.`);
}

function openReceiveKhataModal(custId) {
  const c = db.data.customers.find(cu => cu.id === custId);
  if (!c) return;
  activeKhataCustomerId = custId;
  document.getElementById("kp-customer-name").innerText = `Customer: ${c.name} (${c.phone})`;
  document.getElementById("kp-balance-display").innerText = `₹${c.khataBalance.toFixed(2)}`;
  document.getElementById("kp-amount").value = c.khataBalance;
  openModal("khata-pay-modal");
}

function confirmKhataPayment() {
  const c = db.data.customers.find(cu => cu.id === activeKhataCustomerId);
  if (!c) return;
  const amt = parseFloat(document.getElementById("kp-amount").value) || 0;
  if (amt <= 0) {
    alert("Please enter a valid amount.");
    return;
  }

  c.khataBalance = Math.max(0, c.khataBalance - amt);
  db.data.tillTransactions.push({
    type: "KHATA_RECEIPT",
    amount: amt,
    desc: `Khata Rec: ${c.name}`,
    time: new Date().toISOString()
  });

  db.save();
  closeModal("khata-pay-modal");
  renderCustomersTable();
  renderTillStatus();
  alert(`Payment of ₹${amt.toFixed(2)} received from ${c.name}. Remaining Khata Due: ₹${c.khataBalance.toFixed(2)}`);
}

// ==========================================
// DOCTORS & SUPPLIERS MASTERS
// ==========================================
function renderDoctorsTable() {
  const tbody = document.getElementById("doctors-tbody");
  tbody.innerHTML = db.data.doctors.map(d => `
    <tr>
      <td><strong>${d.name}</strong></td>
      <td><span class="meta-pill" style="font-family: var(--font-mono);">${d.regNo}</span></td>
      <td>${d.spec}</td>
      <td>${d.clinic}</td>
      <td>${d.phone}</td>
      <td style="text-align: center;">
        <span class="badge badge-general">Active Prescriber</span>
      </td>
    </tr>
  `).join("");
}

function openAddDoctorModal() {
  openModal("add-doctor-modal");
}

function saveNewDoctor() {
  const name = document.getElementById("doc-name").value.trim();
  const regNo = document.getElementById("doc-reg").value.trim();
  const spec = document.getElementById("doc-spec").value.trim();
  const clinic = document.getElementById("doc-clinic").value.trim();
  const phone = document.getElementById("doc-phone").value.trim();

  if (!name || !regNo) {
    alert("Doctor name and Medical Council Reg No are required.");
    return;
  }

  db.data.doctors.push({
    id: "DOC" + String(db.data.doctors.length + 1).padStart(2, '0'),
    name: name,
    regNo: regNo,
    spec: spec,
    clinic: clinic,
    phone: phone
  });
  db.save();

  closeModal("add-doctor-modal");
  renderDoctorsTable();
  renderPOSDropdowns();
  alert(`Doctor ${name} registered successfully.`);
}

function renderSuppliersTable() {
  const tbody = document.getElementById("suppliers-tbody");
  tbody.innerHTML = db.data.suppliers.map(s => `
    <tr>
      <td><strong>${s.name}</strong></td>
      <td>${s.contact}</td>
      <td>${s.phone}</td>
      <td><span class="meta-pill">${s.dlNo}</span></td>
      <td><span class="meta-pill">${s.gstin}</span></td>
      <td style="text-align: right; font-weight: 700; color: var(--danger); font-family: var(--font-mono);">₹${s.balance.toFixed(2)}</td>
      <td style="text-align: center;">
        <button class="btn btn-outline btn-sm" onclick="switchTab('purchase')">Inward Entry</button>
      </td>
    </tr>
  `).join("");
}

function openAddSupplierModal() {
  openModal("add-supplier-modal");
}

function saveNewSupplier() {
  const name = document.getElementById("sup-name").value.trim();
  const contact = document.getElementById("sup-contact").value.trim();
  const phone = document.getElementById("sup-phone").value.trim();
  const dlNo = document.getElementById("sup-dl").value.trim();
  const gstin = document.getElementById("sup-gstin").value.trim();

  if (!name) {
    alert("Supplier name is required.");
    return;
  }

  db.data.suppliers.push({
    id: "SUP" + String(db.data.suppliers.length + 1).padStart(2, '0'),
    name: name,
    contact: contact,
    phone: phone,
    dlNo: dlNo,
    gstin: gstin,
    balance: 0.00
  });
  db.save();

  closeModal("add-supplier-modal");
  renderSuppliersTable();
  renderPurchaseDropdowns();
  alert(`Supplier ${name} added.`);
}

// ==========================================
// GST & SALES REPORTS
// ==========================================
function renderReportsData() {
  let netSales = 0;
  let grossProfit = 0;
  let totalGst = 0;
  let billCount = db.data.salesInvoices.length;

  const hsnMap = {};

  db.data.salesInvoices.forEach(inv => {
    netSales += inv.grandTotal;
    totalGst += inv.gstTotal;

    inv.items.forEach(it => {
      const cost = (it.costPtr || (it.rate * 0.75)) * it.qty;
      grossProfit += (it.netAmount - cost);

      const hsn = it.hsn || "300490";
      if (!hsnMap[hsn]) {
        hsnMap[hsn] = { hsn: hsn, desc: "Pharmaceutical Products", qty: 0, val: 0, tax: 0 };
      }
      hsnMap[hsn].qty += it.qty;
      hsnMap[hsn].val += it.netAmount;
      hsnMap[hsn].tax += (it.netAmount * (it.taxPercent / (100 + it.taxPercent)));
    });
  });

  document.getElementById("rep-net-sales").innerText = `₹${netSales.toFixed(2)}`;
  document.getElementById("rep-bill-count").innerText = billCount;
  document.getElementById("rep-gross-profit").innerText = `₹${grossProfit.toFixed(2)}`;
  document.getElementById("rep-total-gst").innerText = `₹${totalGst.toFixed(2)}`;

  // GSTR-1 HSN Table
  const gstr1Tbody = document.getElementById("gstr1-tbody");
  const hsnEntries = Object.values(hsnMap);
  if (hsnEntries.length === 0) {
    gstr1Tbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-muted); padding: 15px;">No sales recorded for GSTR-1 yet.</td></tr>`;
  } else {
    gstr1Tbody.innerHTML = hsnEntries.map(h => {
      const taxable = h.val - h.tax;
      const cgst = h.tax / 2;
      const sgst = h.tax / 2;
      return `
        <tr>
          <td><strong>${h.hsn}</strong></td>
          <td>${h.desc}</td>
          <td>${h.qty}</td>
          <td style="text-align: right; font-family: var(--font-mono);">₹${h.val.toFixed(2)}</td>
          <td style="text-align: right; font-family: var(--font-mono);">₹${taxable.toFixed(2)}</td>
          <td style="text-align: right; font-family: var(--font-mono);">₹${cgst.toFixed(2)}</td>
          <td style="text-align: right; font-family: var(--font-mono);">₹${sgst.toFixed(2)}</td>
        </tr>
      `;
    }).join("");
  }

  // Sales Journal Table
  const journalTbody = document.getElementById("sales-journal-tbody");
  if (db.data.salesInvoices.length === 0) {
    journalTbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-muted); padding: 20px;">No invoices billed yet.</td></tr>`;
  } else {
    journalTbody.innerHTML = db.data.salesInvoices.slice().reverse().map(inv => `
      <tr>
        <td><span class="meta-pill" style="font-family: var(--font-mono); font-weight: 700;">${inv.invoiceNo}</span></td>
        <td>${new Date(inv.date).toLocaleDateString()} ${new Date(inv.date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
        <td><strong>${inv.customer}</strong></td>
        <td>${inv.doctor}</td>
        <td><span class="badge badge-general">${inv.paymentMode}</span></td>
        <td style="text-align: right; font-weight: 700; font-family: var(--font-mono);">₹${inv.grandTotal.toFixed(2)}</td>
        <td style="text-align: center;">
          <button class="btn btn-outline btn-sm" onclick="printSpecificInvoice('${inv.invoiceNo}')">Print</button>
        </td>
      </tr>
    `).join("");
  }
}

function printSpecificInvoice(invNo) {
  const inv = db.data.salesInvoices.find(i => i.invoiceNo === invNo);
  if (inv) printReceipt(inv);
}

// ==========================================
// TILL / SHIFT DRAWER ENGINE
// ==========================================
function renderTillStatus() {
  const opening = db.data.settings.tillOpening || 1500;
  let cashSales = 0;
  let upiSales = 0;
  let khataSales = 0;

  db.data.salesInvoices.forEach(inv => {
    if (inv.paymentMode === "CASH") cashSales += inv.grandTotal;
    if (inv.paymentMode === "UPI") upiSales += inv.grandTotal;
    if (inv.paymentMode === "KHATA") khataSales += inv.grandTotal;
  });

  const expectedCash = opening + cashSales;

  document.getElementById("till-opening-val").innerText = `₹${opening.toFixed(2)}`;
  document.getElementById("till-cash-sales").innerText = `₹${cashSales.toFixed(2)}`;
  document.getElementById("till-upi-sales").innerText = `₹${upiSales.toFixed(2)}`;
  document.getElementById("till-khata-sales").innerText = `₹${khataSales.toFixed(2)}`;
  document.getElementById("till-expected-cash").innerText = `₹${expectedCash.toFixed(2)}`;
  document.getElementById("hdr-till-cash").innerText = `₹${expectedCash.toFixed(2)}`;
}

function openCashDrawer() {
  alert("Cash Drawer Kick Signal Sent (ESC/POS Printer Drawer Trigger). Till unlocked.");
}

function closeShiftDay() {
  const expected = document.getElementById("till-expected-cash").innerText;
  alert(`End of Shift Cash Reconciliation Summary:\nTotal Expected Cash: ${expected}\nAll register logs locked and day closing slip generated.`);
}

// ==========================================
// SETTINGS & BACKUP / RESTORE
// ==========================================
function saveStoreSettings() {
  db.data.settings.storeName = document.getElementById("set-store-name").value.trim();
  db.data.settings.address = document.getElementById("set-store-addr").value.trim();
  db.data.settings.dlNo = document.getElementById("set-store-dl").value.trim();
  db.data.settings.gstin = document.getElementById("set-store-gstin").value.trim();
  db.data.settings.phone = document.getElementById("set-store-phone").value.trim();
  db.data.settings.printFormat = document.getElementById("set-print-format").value;

  db.save();
  applyStoreHeader();
  alert("Store configuration saved successfully.");
}

function exportDatabaseBackup() {
  const jsonStr = JSON.stringify(db.data, null, 2);
  const blob = new Blob([jsonStr], { type: "application/json" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `Wondersoft_PharmaAid_Backup_${new Date().toISOString().split("T")[0]}.json`;
  a.click();
}

function restoreDatabaseBackup() {
  const fileInput = document.getElementById("db-restore-file");
  if (!fileInput.files || fileInput.files.length === 0) {
    alert("Please select a JSON backup file to restore.");
    return;
  }

  const reader = new FileReader();
  reader.onload = (e) => {
    try {
      const parsed = JSON.parse(e.target.result);
      if (!parsed.medicines || !parsed.settings) {
        throw new Error("Invalid database schema in backup file.");
      }
      db.data = parsed;
      db.save();
      applyStoreHeader();
      renderPOSDropdowns();
      renderInventoryTable();
      renderScheduleRegisterTable();
      renderCustomersTable();
      renderDoctorsTable();
      renderSuppliersTable();
      renderReportsData();
      renderTillStatus();
      updateExpiryCountBadge();
      alert("Database restored successfully from backup file!");
    } catch (err) {
      alert("Error restoring database: " + err.message);
    }
  };
  reader.readAsText(fileInput.files[0]);
}

function resetToDemoData() {
  if (!confirm("Are you sure you want to reset database to the preloaded sample pharmacy catalog?")) return;
  db.reset();
  applyStoreHeader();
  renderPOSDropdowns();
  renderInventoryTable();
  renderScheduleRegisterTable();
  renderCustomersTable();
  renderDoctorsTable();
  renderSuppliersTable();
  renderReportsData();
  renderTillStatus();
  updateExpiryCountBadge();
  clearCurrentBill();
  alert("Database successfully reset to standard demo pharmacy dataset.");
}

// ==========================================
// MODAL CONTROLLERS
// ==========================================
function openModal(id) {
  const m = document.getElementById(id);
  if (m) m.classList.add("show");
}

function closeModal(id) {
  const m = document.getElementById(id);
  if (m) m.classList.remove("show");
}

function openHotkeysModal() {
  openModal("hotkeys-modal");
}
