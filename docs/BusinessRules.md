# System Business Rules Specification

## 1. Double-Entry Accounting Ledger Rules
- **Journal Entries must balance**: For every `JournalEntry`, the sum of `Debit` lines must exactly equal the sum of `Credit` lines before posting is allowed.
  $$\sum 	ext{Debit} = \sum 	ext{Credit}$$
- **Immutability of Posted Entries**: Once a `JournalEntry` is marked `is_posted = true`, it becomes strictly read-only. Corrections must be handled by posting a new reversing entry.

## 2. Cashbox & Safe Rules
- **Zero Balance Prevention**: The safe cannot process a withdrawal transaction if the withdrawal amount exceeds the current cash balance of the selected currency (EGP or SAR).
- **Attributed Capital Deposits**: Adding a partner capital investment (`PartnershipCapital`) must automatically trigger a matching cash deposit (`SafeTransaction`) under their name.

## 3. Booking Confirmation Rules
- **Automatic Confirmation**: A booking transitions from `Draft` to `Confirmed` status automatically once the passenger's payment record covers the booking's `SellingPrice`.
