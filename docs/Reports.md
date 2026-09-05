# Financial & Operational Reports

The ERP generates dynamic spreadsheets and financial summaries directly from database records.

## 1. Profit Distribution Spreadsheet
- **Source**: Aggregates passenger booking costs, markups, and external visa profit margins.
- **SQL Logic**: Calculates net margins per booking:
  $$	ext{NetProfit} = 	ext{SellingPrice} - (	ext{NetCost} + 	ext{BarcodeCost} + 	ext{CompanyMarkup})$$
- **Attribution**: Distributes shares according to shareholder ratios registered in `partnership_capital`.

## 2. Cash Flow Log (Safe Report)
- **Source**: Queries `safe_transactions` table.
- **Columns**: Transaction ID, Date, Inflow/Outflow type, Currency (EGP/SAR), exchange rate, and running balance.
