# Business Overview and Preliminary Functional Specification
## Employees' Thrift and Credit Cooperative Societies — Tamil Nadu, India

**Project technology:** Blazor + PostgreSQL  
**Document status:** Initial business preamble and discovery draft  
**Purpose:** To establish a common understanding of society operations before preparing the detailed Software Requirements Specification (SRS).

> **Important:** This document is a preliminary business and software-design draft, not a legal opinion or a statement that every society follows identical rules. The registered bye-laws, applicable Tamil Nadu Co-operative Societies Act and Rules, Registrar's circulars, government orders, audit requirements, and the society's approved policies must be checked before implementation. Configurable rules should be used wherever societies may differ.

---

## 1. Preamble: About the Society

An Employees' Co-operative Thrift and Credit Society is a member-based cooperative established to encourage thrift among eligible employees and provide credit facilities to its members. Such societies may be formed around a department, organisation, institution, company, or another eligible employee group, subject to the society's registration conditions and bye-laws.

Members generally contribute share capital and may make recurring thrift/savings contributions. Subject to the applicable rules and the society's approved policies, the society may provide loans to eligible members. Loan repayments are commonly collected through monthly salary deductions made by the employer/pay-disbursing authority under the required member authorisation and applicable legal procedure. The employer remits the deducted amounts to the society, which must reconcile the remittance and allocate each member's payment to the appropriate dues.

The society is a separate registered cooperative entity. Its management may be through an elected board or through an administrator/special officer where applicable. Its operations must be supported by proper member records, approved resolutions, accounting records, internal controls, statutory returns, and audit trails.

The society's fundamental objectives normally include:

1. Encouraging regular savings and financial discipline among members.
2. Providing loans to eligible members on approved terms.
3. Maintaining members' share capital and thrift balances accurately.
4. Collecting loan instalments, interest, and other authorised dues.
5. Reconciling employer salary deductions with amounts actually remitted.
6. Maintaining transparent accounts and protecting members' interests.
7. Producing reliable financial statements, registers, and audit information.

### 1.1 Legal and regulatory context

The primary legal framework includes the **Tamil Nadu Co-operative Societies Act, 1983**, the **Tamil Nadu Co-operative Societies Rules, 1988**, the society's registered bye-laws, and applicable government/Registrar orders and circulars. Section 48 of the Act addresses salary, wage, and gratuity deductions where the required agreement and procedure apply. Section 66 addresses restrictions on lending, including the general rule that a registered society should not lend to a non-member, subject to the statutory provisions and exceptions.

Tamil Nadu also published the **Tamil Nadu Employees Co-operative Thrift and Credit Societies Common Cadre Service Rules, 2024**, applicable to the Secretary posts/classes specified in those rules.

**Implementation requirement:** Before coding a legal limit, interest rate, deduction priority, eligibility condition, or statutory form, confirm the current authoritative provision and the particular society's approved bye-laws. Do not hard-code a value merely because it is used by one society.

---

## 2. Typical Participants and Responsibilities

| Participant | Typical responsibility |
|---|---|
| Applicant/member | Submits admission and loan requests, provides declarations and authorisations, checks statements, and raises discrepancies. |
| Society clerk/operator | Enters member details, receipts, loan applications, demands, remittances, and supporting documents. |
| Secretary/manager | Supervises daily operations, checks eligibility and balances, prepares reports, and manages correspondence. |
| Maker | Creates a transaction or batch but cannot finally approve their own restricted transactions unless the approved role policy expressly permits it. |
| Checker/authoriser | Verifies, approves, rejects, or returns transactions for correction. |
| Administrator/Special Officer/Board | Exercises the management powers applicable to the society and approves matters within its authority. |
| Employer/pay-disbursing office | Processes authorised salary deductions, supplies deduction details, and remits the deducted amounts. |
| Accountant | Reviews postings, bank reconciliation, ledgers, trial balance, and financial statements. |
| Auditor/inspector | Reviews records, supporting documents, exceptions, and audit observations according to the applicable audit process. |
| System administrator | Manages users, roles, master data, configuration, backups, and technical controls without bypassing financial approval controls. |

The application should support role-based permissions and maker-checker approval. The precise role matrix must be approved by the society.

---

## 3. End-to-End Business Cycle

A typical operational cycle is:

1. Configure society details, financial year, branches/units if applicable, employer/pay offices, products, ledgers, and approval limits.
2. Receive and process member admission applications.
3. Register approved members and collect share capital and other permitted contributions.
4. Maintain thrift/savings balances according to the society's rules and product design.
5. Receive loan applications, verify eligibility, obtain approval, execute documents, and disburse approved loans.
6. Calculate instalment schedules, interest, due dates, and member-wise monthly demands.
7. Generate employer-wise salary deduction schedules and send them to the relevant employer/pay office.
8. Import or enter the employer's actual deduction statement and remittance.
9. Match the remittance to the employer statement and member-wise demand.
10. Appropriate the received amount to the member's loan instalments and other authorised dues according to the approved allocation rules.
11. Identify short deductions, excess deductions, non-deductions, unidentified receipts, and unpaid employer remittances.
12. Update member loan balances, interest, arrears, receipts, and accounting ledgers.
13. Reconcile cash/bank, prepare period-end statements, close the financial period, and produce audit reports.

---

## 4. Functional Activities

### 4.1 Society setup and master data

The system should maintain:

- Society name, registration number/date, address, contact information, and applicable registration/classification details.
- Financial year, accounting period, opening balances, and day/period-close controls where required.
- Registered bye-law parameters and approved operating policies.
- Employer, department, office, payroll unit, and salary-disbursing authority details.
- Member categories, designations, employment status, and eligible employee groups.
- Share types, share value, minimum/maximum holdings where applicable, and transfer/refund rules.
- Thrift/savings products, contribution frequencies, withdrawal rules, and interest rules where applicable.
- Loan products, limits, interest calculation methods, repayment frequencies, security/guarantor requirements, and approval levels.
- Chart of accounts, bank accounts, receipt/payment modes, transaction series, and document templates.
- Users, roles, authorisation limits, and maker-checker workflow.

All financial and eligibility parameters should be effective-dated where a change can affect existing transactions.

### 4.2 Admission of members

Typical workflow:

1. Receive a membership application and assign an application number.
2. Capture name, date of birth, address, contact details, employee number, department, designation, date of joining, salary/pay office, nominee details, and required identity/document details.
3. Confirm that the applicant satisfies the society's membership eligibility and is not already registered under the same member-identification rules.
4. Record required declarations, member consent, and salary-deduction authorisation where applicable.
5. Submit the application for verification and approval.
6. On approval, assign a unique member number and activate the member record.
7. Collect the admission fee, share capital, and initial thrift contribution, if applicable.
8. Issue a membership confirmation/receipt and update the member register.

**Controls and exceptions**

- Prevent accidental duplicate membership using configurable checks such as employee number, employer, and other approved identifiers.
- Do not treat a duplicate warning as conclusive where legitimate changes or re-employment are possible; provide an authorised review process.
- Maintain application, approval, rejection, and reactivation history.
- Record a member's status, such as Applicant, Active, Suspended, Retired, Resigned, Deceased, or Closed, only as appropriate under the society's rules.
- Member cessation must not automatically erase outstanding loans or other liabilities. Settlement and recovery should follow the applicable rules.

### 4.3 Share capital

The society should maintain share transactions separately from thrift deposits and loan balances.

Typical activities:

- Define share value and permitted share categories.
- Collect initial and subsequent share subscriptions.
- Issue a receipt and update the member's share ledger/register.
- Record authorised share transfers, if permitted.
- Calculate eligible share holdings and produce member-wise and society-wide summaries.
- Process share refunds or settlement on cessation only after required checks and approvals.
- Preserve the original transaction and approval trail for corrections.

**Accounting principle:** Share capital is not the same as a savings/thrift deposit, loan repayment, or society income. Map each transaction to the appropriate ledger based on the approved chart of accounts and accounting policy.

### 4.4 Thrift/savings contributions

If the society operates a thrift or savings scheme, configure its rules separately from share capital.

Possible functions:

- Set member-wise or category-wise recurring contribution amounts.
- Include the contribution in the employer's monthly deduction demand, where authorised.
- Record direct member deposits where allowed.
- Allocate employer remittances to the member's thrift account.
- Maintain opening balance, contributions, withdrawals, interest credits, adjustments, and closing balance.
- Apply scheme-specific withdrawal, lien, interest, and closure rules.
- Provide a member statement and society-wide reconciliation.

The software specification must establish whether thrift is compulsory or voluntary, whether contributions earn interest, and whether withdrawals are allowed during membership.

### 4.5 Loan application, scrutiny, and sanction

The loan module should support configurable loan products rather than assuming that all loans use the same terms.

Suggested workflow:

1. Create the loan application against an active member.
2. Select loan type and capture requested amount, purpose, term, repayment frequency, and proposed instalment.
3. Retrieve relevant member data, including share holdings, thrift balance, existing loans, outstanding dues, salary/pay details, and prior repayment history where maintained.
4. Check configurable eligibility criteria, exposure limits, existing arrears, guarantor/security requirements, and any salary-deduction constraints.
5. Record scrutiny remarks and supporting documents.
6. Route the application to the authorised sanctioning officer/committee.
7. Record sanction, rejection, or return for clarification, including sanctioned amount, rate, term, conditions, and approval reference.
8. Generate the sanction order, loan agreement, repayment schedule, and other required documents.
9. Complete required security, guarantor, and salary-deduction documentation.
10. Disburse the loan through the approved payment mode and post the accounting entries.

**Controls**

- A submitted application must not be treated as sanctioned.
- A sanctioned loan must not be disbursed twice.
- Changes to the sanctioned amount, interest rate, or repayment terms after approval must follow an authorised amendment process.
- Preserve the sanction history and the repayment schedule applicable to each loan.
- Apply lending restrictions and eligibility criteria according to the Act, Rules, bye-laws, and approved policy.

### 4.6 Loan repayment schedule and monthly demand

For each disbursed loan, maintain:

- Principal disbursed and disbursement date.
- Interest rate, rate type, calculation method, effective dates, and day-count convention if relevant.
- Repayment term, instalment frequency, first due date, and maturity date.
- Instalment-wise principal, interest, total due, due date, and status.
- Amount received, amount appropriated, balance due, overdue amount, and arrears.
- Moratorium, rescheduling, prepayment, overdue interest, penal charges, or waiver only if permitted and approved.

The calculation method must be explicit. For example, reducing-balance and flat-rate calculations produce different results. The application must not silently assume one method for all products.

### 4.7 Monthly employer demand generation

The society may collect authorised monthly deductions through one or more employer/pay-disbursing offices.

Typical process:

1. Select the deduction month and relevant employer/pay office.
2. Retrieve active members mapped to that office.
3. Calculate each member's due amounts based on the approved schedule and current balances.
4. Include the relevant authorised components, such as:
   - Loan principal instalment.
   - Loan interest instalment.
   - Thrift contribution.
   - Share subscription, if applicable.
   - Other dues only where permitted and authorised.
5. Apply the approved priority and deduction-limit rules.
6. Generate a member-wise demand statement and employer-wise summary.
7. Verify and authorise the demand batch.
8. Export or print the prescribed file/statement and record dispatch details.
9. Retain the exact demand version sent to the employer.

Demand status examples: Draft, Verified, Approved, Sent, Partially Recovered, Reconciled, Cancelled/Amended. A sent demand should not be overwritten silently; changes should be represented by a traceable amendment or adjustment.

### 4.8 Employer salary deductions and remittance

The employer/pay office may return a statement showing actual deductions, non-deductions, partial deductions, and the amount remitted.

The application should support:

- Entry or import of the employer's recovery statement.
- Employer reference number, payroll month, deduction date, remittance date, bank reference, and total remitted.
- Member-wise actual deduction against each demanded component.
- Reason codes for non-recovery or short recovery, such as insufficient salary, leave without pay, retirement, transfer, suspension, member not found, or other employer-reported reason.
- Matching the employer's total statement with the amount received in the society's bank account.
- Recording bank charges or remittance differences separately, if applicable.
- Unmatched and unidentified amounts in a suspense/unallocated-receipt workflow.
- Acknowledgement/receipt to the employer and retention of the supporting statement.

**Important legal workflow:** Section 48 of the Tamil Nadu Co-operative Societies Act addresses deductions from salary, wages, or gratuity where the required member agreement and procedure apply. The current Act and Rules must be consulted for applicable limits, remittance timelines, and registers. The software should not assume every member or employer is automatically subject to deduction without the necessary legal basis and documentation.

### 4.9 Appropriation of recoveries

Appropriation means allocating the amount actually recovered to the correct member account and due component. It is a key control and must not be treated as merely receiving one lump-sum amount from an employer.

Recommended workflow:

1. Receive or import the employer's member-wise deduction statement.
2. Record the total remittance and match it with the bank credit.
3. Match each member's actual deduction with the relevant monthly demand.
4. Apply the society-approved allocation rule to the received amount.
5. Update the member's loan schedule, principal outstanding, interest due, thrift balance, share account, and other authorised dues as applicable.
6. Post the corresponding accounting entries.
7. Mark the demand items as fully recovered, partly recovered, not recovered, or over-recovered.
8. Generate a member-wise appropriation statement and employer-wise reconciliation report.
9. Issue or make available member credits/receipts as required by the applicable procedure.
10. Route exceptions to an authorised user for investigation and resolution.

**Do not hard-code an appropriation sequence without approval.** For example, whether a receipt is allocated first to interest, principal, overdue dues, thrift, or another component must be determined by the governing rules, loan agreement, bye-laws, and approved accounting policy. Keep the allocation method configurable by product where legally and operationally permitted.

The Tamil Nadu Co-operative Societies Rules contain provisions concerning the crediting of amounts deducted from a member's salary and the furnishing of particulars to the member. Confirm the current wording and applicable procedure before finalising system behaviour.

### 4.10 Recovery exceptions and arrears

The system should identify and report:

- Demand raised but no deduction reported.
- Deduction reported but no corresponding remittance received.
- Remittance received but no member-wise statement supplied.
- Actual deduction lower or higher than demand.
- Employer total differs from the sum of member-wise deductions.
- Duplicate recovery line or duplicate bank receipt.
- Member transferred to another office or employer.
- Member retired, resigned, deceased, or otherwise ceased employment.
- Loan overdue despite salary deduction.
- Unallocated or suspense receipts.
- Demand cancellation or amendment after dispatch.

Each exception should have a status, owner, remarks, supporting documents, and resolution history. Unmatched amounts must not be distributed arbitrarily across members.

### 4.11 Direct receipts and other recoveries

If allowed by the society's policies, the application may accept direct member payments through cash, bank transfer, cheque, or approved digital payment methods.

The receipt must identify:

- Member and loan/account.
- Receipt date and value date where applicable.
- Payment mode and transaction reference.
- Amount and component allocation.
- User who entered the receipt and approving user, where required.
- Reversal/cancellation reference if corrected.

Prevent double collection by checking whether the same instalment has already been appropriated through payroll or another receipt.

### 4.12 Loan closure, member cessation, and settlement

Typical functions include:

- Calculate a settlement statement as of a specified date.
- Include outstanding principal, accrued interest, overdue amounts, and other authorised charges or credits.
- Record prepayment or final settlement with approval.
- Reconcile any deductions received after closure and process refunds or adjustments under the approved rules.
- Record death, retirement, resignation, transfer, or other cessation events.
- Process nominee/legal-heir claims and share/thrift settlements only after the required documentation and approvals.
- Maintain outstanding-liability follow-up where the member ceases employment but still owes the society.
- Close a member's operational accounts only when settlement and legal requirements are satisfied.

### 4.13 Accounting and financial control

The system should support a double-entry accounting model, subject to the society's approved accounting framework.

Likely accounting functions:

- Chart of accounts and ledger mapping.
- Cash book, bank book, journal, receipt and payment vouchers.
- Member-wise subsidiary ledgers for share capital, thrift, loans, interest, and other balances.
- Loan disbursement and recovery postings.
- Employer remittance reconciliation.
- Bank reconciliation.
- Trial balance, income and expenditure account, balance sheet, and other required statements.
- Period-end accruals, provisions, depreciation or other adjustments where applicable.
- Opening balances and controlled year-end carry-forward.
- Reversal and correction entries with audit trail.

The exact journal entries depend on the society's accounting policy and whether the transaction is a share subscription, thrift deposit, loan principal recovery, interest recovery, or another item. These should be approved by the society's accountant/auditor before implementation.

### 4.14 Governance, audit, and statutory reporting

Potential functions include:

- Board/committee resolution register and sanction references.
- Member register and member-wise ledgers.
- Loan application, sanction, security, guarantor, and recovery registers.
- Share capital and thrift registers.
- Employer-wise demand, deduction, and remittance registers.
- Cash book, bank book, general ledger, and trial balance.
- Arrears and overdue reports.
- Financial statements and statutory returns applicable to the society.
- Audit query/observation tracking, reply, rectification, and closure.
- User activity log and transaction approval history.
- Data export in the formats required by the relevant authority.

The application must not assume that one report format is mandatory for every society. Maintain a report catalogue mapped to the society's applicable requirements and the latest prescribed formats.

---

## 5. Proposed Application Modules

| Module | Main functions |
|---|---|
| 1. Society Configuration | Society profile, financial year, policies, transaction numbering, approval limits |
| 2. Master Data | Employer/pay office, member categories, loan products, thrift products, ledgers, payment modes |
| 3. Membership | Application, scrutiny, approval, member register, nomination, status changes |
| 4. Share Capital | Share subscription, transfers where allowed, statements, settlement/refund |
| 5. Thrift/Savings | Contributions, withdrawals, interest, balances, member statements |
| 6. Loan Management | Application, eligibility, sanction, documentation, disbursement, repayment schedule |
| 7. Demand Management | Monthly calculations, employer-wise batches, approval, dispatch and amendments |
| 8. Employer Recovery | Recovery statement import/entry, actual deductions, remittance and matching |
| 9. Appropriation | Component-wise allocation, member credit, loan schedule update, exception management |
| 10. Direct Receipts | Member payments, loan receipts, allocation and duplicate-payment checks |
| 11. Arrears & Settlement | Overdue tracking, notices, settlement, cessation and closure |
| 12. General Accounts | Vouchers, ledgers, cash/bank books, bank reconciliation, trial balance and statements |
| 13. Reports & Audit | Registers, statutory reports, audit observations, exception and reconciliation reports |
| 14. Security & Administration | Users, roles, maker-checker, audit log, backup and configuration |

---

## 6. Suggested Reports

### Membership and share capital
- Member register and active/inactive member list.
- New admissions, rejected applications, and pending applications.
- Member-wise share capital statement.
- Share capital collection summary and settlement report.
- Member cessation and nominee/settlement pending report.

### Thrift/savings
- Member-wise thrift ledger and balance statement.
- Month-wise contribution demand and collection.
- Withdrawal and interest-credit register, where applicable.
- Thrift balance reconciliation.

### Loans
- Loan application pending/sanctioned/rejected report.
- Loan disbursement register.
- Member-wise loan ledger and outstanding balance.
- Instalment schedule and month-wise demand.
- Overdue, arrears, and ageing report.
- Loan closure and settlement statement.
- Guarantor/security register, where applicable.

### Employer demand and recovery
- Employer-wise monthly demand.
- Member-wise salary deduction schedule.
- Demand versus actual recovery comparison.
- Employer-wise deduction and remittance register.
- Remittance received but not appropriated.
- Salary deduction reported but remittance pending.
- Partial/non-recovery reasons.
- Excess/duplicate/unidentified recovery report.
- Monthly appropriation statement by member and loan.

### Accounts and audit
- Daily receipt/payment summary.
- Cash book, bank book, general ledger, and trial balance.
- Bank reconciliation statement.
- Interest income and loan balance summaries.
- Financial statements and applicable statutory returns.
- Audit trail, pending approvals, reversals, and exception register.

---

## 7. Initial Database Design Areas for PostgreSQL

This is a conceptual starting point, not a final schema. Names and columns should be agreed after the workflow is confirmed.

| Entity/table area | Purpose |
|---|---|
| `society_master` | Society profile and registration information |
| `financial_year` / `accounting_period` | Financial year and period controls |
| `employer_master` / `pay_office_master` | Employer and payroll-disbursing office |
| `member_application` | Admission requests and scrutiny |
| `member_master` | Approved member records |
| `member_nominee` | Nomination details and history |
| `share_product` / `member_share_txn` | Share product and member share movements |
| `thrift_product` / `member_thrift_txn` | Thrift rules and transactions |
| `loan_product` | Loan terms and calculation settings |
| `loan_application` | Loan request, scrutiny, and sanction workflow |
| `loan_account` | Disbursed loan account |
| `loan_schedule` | Instalment-wise principal, interest, due dates, and status |
| `employer_demand_batch` | Monthly employer demand header |
| `employer_demand_line` | Member-wise demand components |
| `employer_recovery_batch` | Employer recovery statement and remittance header |
| `employer_recovery_line` | Actual member-wise deductions and components |
| `recovery_allocation` | Mapping of each receipt/deduction to the relevant member and due component |
| `receipt_header` / `receipt_line` | Direct or other receipts and component breakdown |
| `voucher_header` / `voucher_line` | Double-entry accounting vouchers |
| `ledger_master` | Chart of accounts |
| `bank_reconciliation` | Bank statement matching |
| `approval_history` | Maker-checker decisions and comments |
| `document_register` | Supporting documents and references |
| `audit_log` | Who changed what and when |
| `report_run_log` | Optional record of generated official reports |

### Suggested design principles

1. Use primary keys that are stable and separate from human-readable member/loan numbers.
2. Add unique constraints for society-defined member numbers, loan numbers, and transaction references.
3. Use foreign keys for relationships and check constraints for valid amounts/statuses where appropriate.
4. Use `numeric`/`decimal` types for money; do not use floating-point types for financial calculations.
5. Store transaction date, accounting date/value date where needed, created/approved users, and timestamps.
6. Use database transactions for receipt allocation and ledger posting so that a partial posting cannot leave inconsistent balances.
7. Avoid deleting posted financial transactions. Use authorised reversal/correction entries with references to the original transaction.
8. Preserve the demand version sent to the employer and the original recovery statement imported from the employer.
9. Maintain a clear separation between **demanded**, **deducted by employer**, **remitted**, **received in bank**, **appropriated to member**, and **posted to accounts**. These are different events and must be separately traceable.
10. Keep interest methods, rounding rules, allocation priorities, deduction limits, and approval thresholds configurable and effective-dated.
11. Use least-privilege role-based access, maker-checker controls, and immutable or protected audit history.
12. Store identity documents and personal data securely, restrict access, and avoid exposing full identity numbers in ordinary screens and reports.
13. Consider a `society_id` in the data model if the application will serve multiple independent societies. Apply tenant isolation in APIs, queries, permissions, and database access—not only by hiding UI elements.
14. Decide whether the application will be a standalone system or must integrate with the Government of Tamil Nadu's existing cooperative digital platform. Confirm current integration specifications before designing imports/exports.

---

## 8. Important Business Rules to Confirm Before Finalising the SRS

The following questions should be answered by the society's Secretary, accountant, auditor, and relevant cooperative department representative.

### Membership and contributions
- Who is eligible to become a member?
- What are the admission fee, share value, minimum shareholding, and initial contribution?
- Is thrift contribution compulsory, voluntary, or product-based?
- Are direct contributions permitted in addition to salary deductions?
- What is the approved procedure when a member retires, resigns, transfers, or dies?

### Loans
- Which loan products are offered?
- What are the maximum amount, eligibility rules, repayment term, interest method, and rates?
- Are guarantors, sureties, collateral, or insurance required?
- Who may sanction each type/amount of loan?
- What are the rules for arrears, overdue interest, penalties, waivers, restructuring, prepayment, and closure?
- What happens when a member's salary is insufficient for the full deduction?

### Employer demands and remittances
- Which employers/pay offices are covered, and how are member transfers handled?
- What is the monthly cutoff date for generating and sending demands?
- What file layout or prescribed form does each employer accept?
- How are partial deductions and multiple cooperative deductions handled?
- What legal deduction limits and priority rules apply to this society?
- How are deductions made by the employer but remitted late treated and reported?
- What date is used for member credit and interest calculations where deduction and remittance dates differ?
- Who resolves mismatches and approves adjustments?

### Accounting and audit
- Which chart of accounts and accounting conventions are prescribed or approved?
- What are the approved journal entries for share capital, thrift, loan disbursement, principal recovery, interest recovery, and employer remittance?
- What reports, registers, returns, and audit formats are currently required?
- What is the year-end closing and opening-balance procedure?
- What is the approval process for corrections, reversals, and write-offs?

### Technology and operations
- Is the system for one society or multiple societies?
- Will members have a self-service portal or receive SMS/email statements?
- Will employer deductions be imported from Excel/CSV, a payroll interface, or manually entered?
- Is integration required with banks, payroll systems, or a government cooperative platform?
- What are the requirements for Tamil/English screens, data migration, backups, disaster recovery, hosting, and retention?
- Who owns and approves configuration changes and production access?

---

## 9. Recommended Implementation Phases

### Phase 1 — Discovery and rule approval
Collect the registered bye-laws, current forms, sample member register, share/thrift ledgers, loan application and sanction formats, employer demand forms, recovery statements, accounting vouchers, audit reports, and sample financial statements. Confirm each workflow with the society.

### Phase 2 — Foundation and membership
Build society configuration, user/role administration, employer master, member admission, member register, share capital, and opening-balance migration.

### Phase 3 — Loans and recovery
Build loan products, application/sanction, disbursement, repayment schedules, demand generation, employer statement import, remittance matching, appropriation, arrears, and settlement.

### Phase 4 — Accounting and reporting
Complete voucher posting, ledgers, cash/bank books, bank reconciliation, trial balance, financial statements, registers, and audit reports. Validate accounting entries with the society's accountant/auditor.

### Phase 5 — Testing, migration, and rollout
Test normal and exceptional cases, migrate reconciled opening balances, train users, run parallel verification where feasible, obtain sign-off, and establish backup, support, and change-control procedures.

---

## 10. Essential Acceptance Scenarios

Before go-live, demonstrate at least these scenarios:

1. A new member is admitted, approved, and assigned a unique member number.
2. Share capital and thrift contributions are collected and reflected in separate member ledgers.
3. A loan is scrutinised, sanctioned, documented, and disbursed exactly once.
4. The instalment schedule agrees with an independently verified calculation.
5. A monthly employer demand is generated, checked, approved, and preserved as sent.
6. An employer deducts the full amount and remits it; the system reconciles and appropriates it correctly.
7. An employer deducts only part of the amount; the balance remains due and is reported accurately.
8. A deduction statement is received but the bank remittance is missing; the item remains unresolved rather than being treated as cash received.
9. A bank remittance arrives without member-wise details; it remains unallocated until matched and approved.
10. An overpayment or duplicate recovery is detected and handled through an authorised correction/refund process.
11. A member retires or dies with an outstanding loan; the system follows the approved settlement/recovery process.
12. A posted transaction is corrected through a reversal or adjustment while preserving the audit trail.
13. Member sub-ledgers reconcile to the general ledger.
14. A user without approval permission cannot approve their own restricted transaction.
15. Reports reproduce the same totals as the underlying ledgers and can be traced back to source transactions.

---

## 11. Source References for Business and Legal Validation

These references are starting points. Check the latest amended text, rules, circulars, and society-specific bye-laws before relying on a provision.

1. **Tamil Nadu Co-operative Societies Act, 1983 — India Code PDF**  
   https://www.indiacode.nic.in/bitstream/123456789/13260/1/the_tamilnadu_cooperative_societies_act-1983.pdf

2. **Tamil Nadu Co-operative Societies Act and Rules — Registrar of Co-operative Societies, Tamil Nadu**  
   Act: https://rcs.tn.gov.in/uploads/act_rules/tnsc_act.pdf  
   Rules: https://rcs.tn.gov.in/uploads/act_rules/tnsc_rules.pdf

3. **Tamil Nadu Employees Co-operative Thrift and Credit Societies Common Cadre Service Rules, 2024 — Tamil Nadu Government Gazette**  
   https://www.stationeryprinting.tn.gov.in/gazette/2024/8_III_1a_2024.pdf

4. **Kootturavu — Government of Tamil Nadu cooperative digital platform**  
   https://kootturavu.tn.gov.in/  
   Confirm whether the society is expected to use this platform and what integration or migration requirements apply before committing to a separate system.

5. **Government document describing Employees' Cooperative Societies**  
   https://tnega.tn.gov.in/cmsimages/TNeGAOTTNCOOS20202021/12092020120917-2.Pdf

---

## 12. Closing Statement

The proposed application should provide a complete, traceable member-credit workflow from admission through share/thrift maintenance, loan sanction and disbursement, employer demand, salary deduction, remittance, appropriation, arrears management, accounting, and audit reporting.

The central design principle is to maintain a reliable link between **the member, the approved obligation, the demand sent, the amount actually deducted, the amount remitted, the amount received, the amount appropriated, and the accounting entry**. This separation will help prevent incorrect member balances, duplicate recovery, unallocated receipts, and reconciliation differences.

The next deliverable should be a detailed SRS containing screen-wise requirements, field definitions, validations, workflow/status diagrams, approval matrix, calculation rules, database relationships, accounting-entry examples, reports, and acceptance tests.
