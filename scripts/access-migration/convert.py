"""Convert Tsis_MRF Access tables into CSVs shaped for the ECS PostgreSQL schema.

Usage: python3 -I convert.py <workdir> <mdb>
Writes <workdir>/out/*.csv, <workdir>/load.sql, <workdir>/report.md. Touches no database.
"""
import csv, collections, os, re, subprocess, sys

S, MDB = sys.argv[1], sys.argv[2]
OUT = os.path.join(S, 'out'); os.makedirs(OUT, exist_ok=True)
csv.field_size_limit(1 << 30)

BR = '11005'
PREFIX = 110050000000
MAXRUN = 9999999

# --- tables never touched ------------------------------------------------------------------
EXCLUDED = {'account_transactions', 'account_voctype', 'bank_master', 'business_day', 'gen_bank_name',
            'menu_forms', 'menu_main', 'menu_sub', 'pay_component_assignments', 'pay_components'}
# app/config tables with no Access source that we keep
KEEP = {'refresh_tokens', 'users', 'reports_master', 'reports_group', 'society_options', 'society_neft',
        'refer_constituency', 'map_general', 'map_suspenseaccounts', 'jl_max_limit', 'jl_loaneligible',
        'jl_dailymarketrate', 'locker_settings', 'locker_size_master', 'locker_status_master',
        'locker_rent_template', 'lockers', 'loan_schemes_group', 'loan_scheme_deduction_rules',
        'interestapplication_onmemexpiry', 'pay_vpf', 'mem_retirementage'}

# --- PG metadata ---------------------------------------------------------------------------
pg = collections.OrderedDict()
for l in open(os.path.join(S, 'survey/pg_meta.txt')):
    t, c, d, nul, dflt, pk = l.rstrip('\n').split('|')
    pg.setdefault(t, collections.OrderedDict())[c] = dict(type=d, notnull=(nul == 'NO'), default=dflt, pk=(pk == 'PK'))

# --- Access metadata -----------------------------------------------------------------------
acc_tables = subprocess.run(['mdb-tables', '-1', MDB], capture_output=True, text=True).stdout.splitlines()
acc_by_lower = {t.lower(): t for t in acc_tables}

def norm(s): return re.sub(r'[^a-z0-9]', '', s.lower())

# --- table mapping: pg_table -> access_table ----------------------------------------------
TABLE_MAP = {}
for t in pg:
    if t in EXCLUDED or t in KEEP: continue
    if t in acc_by_lower: TABLE_MAP[t] = acc_by_lower[t]
TABLE_MAP.update({
    'fin_voucher_trn': 'Fin_voucher_tr', 'fin_voucher_bank': 'Gen_Voc_Bank', 'fin_voucher_bank_trn': 'Gen_Voc_Bank_Tr',
    'loan_sanction_trn': 'Loan_Sanction_Tr', 'pay_slip_trn': 'Pay_Slip_Tr', 'pay_slip_loan_trn': 'Pay_Slip_Loan_Tr',
    'lien_trn': 'Lien_Tr', 'termdeposit_intcalc_calendar': 'TermDeposit_IntCalc_Calender',
    'deposit_masters': 'Deposit_Master', 'deposit_options': 'Depositor_Option',
    'deposit_roi_templates': 'Deposit_Roi_Template', 'mem_collection_priority': 'Mem_CollectionPriority',
    'emp_pfcalccalendar': 'Emp_PFCalcCalender',
})

# --- column overrides: pg_col -> access col name (str) or callable(row dict, lowercased keys) --------
def neg(col): return lambda r: None if r.get(col) in (None, '') else (not truthy(r[col]))
def total(*cols): return lambda r: sum(num(r.get(c)) for c in cols)
EMP = {'mem_id': 'emp_id'}
COLMAP = {
    'mem_master': {'caste_id': 'casteid', 'alternative_mobileno': 'residencephoneno', 'designation_id': 'designationid',
                   'office_id': 'officeid', 'existing_memberno': 'existingmemberno', 'suretymem_id': 'suritymem_id',
                   'section_name': 'section', 'ifsccode': 'ifccode'},
    'emp_pf': EMP, 'pay_att': {'mem_id': 'emp_id', 'att_id': 'att_tr_id'}, 'pay_slip': EMP, 'pay_leave_eligible': EMP, 'pay_retirement': EMP,
    'emp_qualification': EMP,
    'pay_slip_trn': {'mem_id': 'emp_id', 'pay_component_type': 'all_ded_type'},
    'fin_ledger_subgrp': {'subgrp_delete': 'subgp_delete'},
    'loan_sanction': {'suretymem_id': 'suritymem_id', 'suretysharecapitalamount': 'suritysharecapitalamount'},
    'mem_address': {'mem_village_id': 'mem_villge_id'},
    'jl_ornments': {'loan_id': 'jl_id'},
    'lien_trn': {'lientr_oe': 'lien_oe'},
    'sbca_master': {'scheme_id': 'dm_id'},
    'sbca_schemes': {'sbca_led_id': 'sbca_ledid', 'sbca_int_led_id': 'sbca_intledid'},
    'pay_gen_info': {'usr_id': 'pay_info_login_id', 'yr_id': 'pay_info_yr_id'},
    'mem_demand_status': {'id': 'demstatus_id', 'office_id': 'officeid', 'is_appropriated': 'isappropriated',
                          'is_recovery_saved': 'isrecoverysaved', 'is_active': neg('demstatus_delete')},
    'mem_demand_stop': {'id': 'stopdemand_id', 'demand_date': 'demanddate', 'is_active': neg('stopdemand_delete')},
    'deposit_trn': {'trn_id': 'dd_id', 'deposit_id': 'dm_id', 'trn_date': 'ddtrn_date', 'due_date': 'dddue_date',
                    'demand_amount': 'demand_amt', 'receipt_amount': 'receipt_amt', 'paid_amount': 'paid_amt',
                    'interest_calculated_amount': 'interestcalculated_amt',
                    'interest_calculated_date': 'interestcalculated_date', 'interest_paid_amount': 'interestpaid_amt',
                    'penal_interest_calculated_amount': 'penalinterestcalculated_amt',
                    'penal_interest_calculated_date': 'penalinterestcalculated_date',
                    'penal_interest_received_amount': 'penalinterestcollected_amt', 'is_opening_entry': 'dd_oe',
                    'is_active': neg('dd_delete'), 'pble_master_id': 'pblemaster_id'},
    'deposit_masters': {'deposit_id': 'dm_id', 'deposit_name': 'dmname', 'led_id': 'dmled_id',
                        'interest_led_id': 'dmintled_id', 'pi_led_id': 'dmpiled_id', 'percentage': 'dmpercentage',
                        'maximum_amount': 'dmmaximumamount', 'fixed_amount': 'dmfixedamount',
                        'frequency': 'dmfrequency', 'is_active': neg('dm_delete'),
                        'percentage_towards_deposit': 'percentageofinteresttransfertodeposit',
                        'percentage_towards_other_ledger': 'percentageofinteresttransfertoledger',
                        'other_led_id': 'transfereeledger_id', 'is_arrear_demand_applicable': 'arreardemandapplication',
                        'interest_calculation_period': 'interestcalcperiod'},
    'deposit_options': {'option_id': 'do_id', 'deposit_id': 'dm_id', 'wef': 'do_wef', 'is_active': neg('do_delete')},
    'deposit_roi_templates': {'roi_id': 'dmroi_id', 'deposit_id': 'dm_id', 'wef': 'dmroi_wef', 'interest_rate': 'dm_roi',
                              'penal_interest_rate': 'dm_pi', 'is_active': neg('dmroi_delete')},
    'mem_collection_priority': {'deposit_id': 'dm_id', 'is_active': neg('priority_delete')},
    'fin_voucher_bank_trn': {'fvb_tr_id': 'gvb_tr_id', 'fvb_id': 'gvb_id', 'fvb_date': 'gvb_date',
                             'fvb_amt_adjusted': 'gvb_amt_adjusted', 'fvbtr_delete': 'gvbtr_delete'},
    'mem_demand': {
        'id': 'memdemand_id', 'calculated_date': 'demandcalculateddate', 'recovery_date': 'recoverydate',
        'demand_type': 'demandtype', 'prl_schedule': 'prl_sched', 'non_od_prl': 'nonodprl', 'loan_outstanding': 'loanos',
        'loan_pi_arrear': 'piarreardemand', 'loan_int_arrear': 'intarreardemand', 'loan_prl_arrear': 'prlarreardemand',
        'loan_pi_current': 'picurrentdemand', 'loan_int_current': 'intcurrentdemand', 'loan_prl_current': 'prlcurrentdemand',
        'loan_total': total('pitotaldemand', 'inttotaldemand', 'prltotaldemand'),
        'deposit_id': 'dm_id', 'deposit_arrear': 'arreardepositamount', 'deposit_current': 'currentdepositamount',
        'deposit_pi_arrear': 'arrearpiondeposit', 'deposit_pi_current': 'currentpiondeposit',
        'deposit_total': total('totaldepositamount', 'totalpiondeposit'),
        'debtor_led_id': 'suspenseled_id', 'debtor_arrear': 'arrearsuspenseamount', 'debtor_current': 'currentsuspenseamount',
        'debtor_total': 'totalsuspenseamount',
        'rd_instalment': 'rdinstalmentamount', 'rd_noof_instalments': 'rdnoofinstalments', 'rd_demand': 'rddemandamount',
        'rd_pi_arrear': 'pionrdarrearamount', 'rd_pi_calc': 'pionrdcalcamount',
        'rd_pi_application_date': 'pionrdapplicationdate', 'rd_total': 'rdtotaldemandamount',
        'dueby_led_id': 'duebydemandled_id', 'dueby_arrear': 'arrearduebydemandamount',
        'dueby_current': 'currentduebydemandamount', 'dueby_total': 'totalduebydemandamount',
        'total_demand': 'totaldemandamount', 'restricted_demand': 'restricteddemand',
        'loan_pi_arrear_collection': 'piarrearcollection', 'loan_int_arrear_collection': 'intarrearcollection',
        'loan_prl_arrear_collection': 'prlarrearcollection', 'loan_pi_current_collection': 'picurrentcollection',
        'loan_int_current_collection': 'intcurrentcollection', 'loan_prl_current_collection': 'prlcurrentcollection',
        'loan_total_collection': total('pitotalcollection', 'inttotalcollection', 'prltotalcollection'),
        'deaposit_arrear_collection': 'arreardepositcollection', 'deposit_current_collection': 'currentdepositcollection',
        'deposit_arrear_pi_collection': 'arrearpiondepositcollection',
        'deposit_current_pi_collection': 'currentpiondepositcollection',
        'deposit_total_collection': total('totaldepositcollection', 'totalpiondepositcollection'),
        'debtor_arrear_collection': 'arrearsuspensecollection', 'debtor_current_collection': 'currentsuspensecollection',
        'debtor_total_collection': 'totalsuspensecollection',
        'rd_receipt': 'rdreceiptamount', 'rd_pi_receipt': 'pionrdreceiptamount', 'rd_total_receipt': 'rdtotalreceiptamount',
        'creditor_led_id': 'suspenseduebyled_id', 'creditor_collection': 'totalduebycollection',
        'dueby_arrear_collection': 'arrearduebycollectionondemand',
        'dueby_current_collection': 'currentduebycollectionondemand',
        'dueby_total_collection': 'totalduebycollectionondemand',
        'total_collection': 'totalcollectionamount',
        'is_active': neg('memdemand_delete'), 'stop_demand': 'stopdemand', 'is_appropriated': 'isappropriated',
        'is_recovery_saved': 'isrecoverysaved',
        'loan_pi_arrear2': 'piarreardemand2', 'loan_int_arrear2': 'intarreardemand2', 'loan_prl_arrear2': 'prlarreardemand2',
        'loan_pi_current2': 'picurrentdemand2', 'loan_int_current2': 'intcurrentdemand2',
        'loan_prl_current2': 'prlcurrentdemand2',
        'loan_total2': total('pitotaldemand2', 'inttotaldemand2', 'prltotaldemand2'),
        'deposit_arrear2': 'arreardepositamount2', 'deposit_current2': 'currentdepositamount2',
        'deposit_pi_arrear2': 'arrearpiondeposit2', 'deposit_pi_current2': 'currentpiondeposit2',
        'deposit_total2': total('totaldepositamount2', 'totalpiondeposit2'),
        'debtor_arrear2': 'arrearsuspenseamount2', 'debtor_current2': 'currentsuspenseamount2',
        'debtor_total2': 'totalsuspenseamount2',
        'rd_instalment2': 'rdinstalmentamount', 'rd_noof_instalments2': 'rdnoofinstalments2', 'rd_demand2': 'rddemandamount2',
        'rd_pi_arrear2': 'pionrdarrearamount2', 'rd_pi_calc2': 'pionrdcalcamount2',
        'rd_pi_application_date2': 'pionrdapplicationdate2', 'rd_total2': 'rdtotaldemandamount2',
        'total_demand2': 'totaldemandamount2',
    },
}
# fin_voucher_bank: gvb_* -> fvb_*
COLMAP['fin_voucher_bank'] = {c: 'gvb_' + c[4:] for c in pg['fin_voucher_bank'] if c.startswith('fvb_')}

# --- helpers --------------------------------------------------------------------------------
NUMT = {'numeric', 'bigint', 'integer', 'smallint', 'double precision', 'real'}
def truthy(v): return str(v).strip().lower() in ('1', '-1', 'true', 't', 'yes', 'y')
def num(v):
    if v in (None, ''): return 0.0
    try: return float(v)
    except ValueError: return 0.0
def is_id(col, typ):
    return typ in ('numeric', 'bigint') and (col == 'id' or col.endswith('_id') or col in ('referid', 'calc_yrid'))
def zero(typ):
    if typ in NUMT: return '0'
    if typ == 'boolean': return 'f'
    if typ == 'date': return '1900-01-01'
    if typ.startswith('timestamp'): return '1900-01-01 00:00:00'
    return ''

report = []; sql_tables = []; problems = []
for t, at in TABLE_MAP.items():
    cols = pg[t]
    p = subprocess.Popen(['mdb-export', '-D', '%Y-%m-%d %H:%M:%S', '-T', '%Y-%m-%d %H:%M:%S', '-b', 'strip', MDB, at],
                         stdout=subprocess.PIPE, text=True, encoding='utf-8', errors='replace')
    rd = csv.reader(p.stdout)
    hdr = [h.lower() for h in next(rd, [])]
    by_norm = {norm(h): h for h in hdr}
    over = COLMAP.get(t, {})
    plan = collections.OrderedDict(); used = set(); unmapped = []
    for c, m in cols.items():
        if c == 'brcode': plan[c] = ('const', BR); continue
        if c == 'voc_status': plan[c] = ('const', 'V'); continue
        if c in over:
            src = over[c]
            if callable(src): plan[c] = ('fn', src); continue
            if src in hdr: plan[c] = ('col', src); used.add(src); continue
            problems.append(f'{t}.{c}: override source {src} not in {at}')
        if c in hdr: plan[c] = ('col', c); used.add(c); continue
        if norm(c) in by_norm: plan[c] = ('col', by_norm[norm(c)]); used.add(by_norm[norm(c)]); continue
        if m['pk']: plan[c] = ('rownum', None); unmapped.append(c + '(generated 1..n)'); continue
        if m['default']: unmapped.append(c + '(db default)'); continue
        if m['notnull']: plan[c] = ('const', zero(m['type'])); unmapped.append(c + f'(filled {zero(m["type"])!r})'); continue
        unmapped.append(c + '(NULL)')
    for c, src in over.items():
        if callable(src):
            for h in hdr:
                pass
    stats = collections.Counter(); overflow = []; pkseen = set(); pkcols = [c for c in cols if cols[c]['pk']]
    outpath = os.path.join(OUT, t + '.csv')
    with open(outpath, 'w', newline='', encoding='utf-8') as f:
        w = csv.writer(f)
        n = 0
        for raw in rd:
            n += 1
            r = dict(zip(hdr, raw))
            out = []
            for c, (kind, src) in plan.items():
                typ = cols[c]['type']
                if kind == 'const': v = src
                elif kind == 'rownum': v = str(n)
                elif kind == 'fn':
                    v = src(r)
                    v = None if v is None else ('t' if v is True else 'f' if v is False else repr(v) if isinstance(v, float) else str(v))
                else: v = r.get(src, '')
                if v is None or v == '':
                    v = None
                    if cols[c]['notnull'] and typ != 'boolean':
                        lit = re.match(r"^'?(-?[\w.: -]*?)'?(::[\w ]+)?$", cols[c]['default'] or '')
                        v = lit.group(1) if (lit and cols[c]['default'] and 'nextval' not in cols[c]['default']) else zero(typ)
                        stats['notnull_filled:' + c] += 1
                    elif cols[c]['notnull'] and typ == 'boolean':
                        v = 'f'
                    elif typ in ('character varying', 'text') and kind == 'col':
                        v = ''  # keep as empty string like Access
                if v is not None:
                    if typ == 'boolean': v = 't' if truthy(v) else 'f'
                    elif typ in NUMT:
                        if str(v).lower() in ('true', 'false'): v = '1' if truthy(v) else '0'
                        if typ in ('integer', 'smallint', 'bigint') or is_id(c, typ):
                            try:
                                fv = float(v); v = str(int(fv)) if fv == int(fv) else v
                            except ValueError:
                                problems.append(f'{t}.{c}: non-numeric {v!r}'); v = '0'
                        if is_id(c, typ):
                            iv = int(float(v))
                            if iv < 0 or iv > MAXRUN: overflow.append((c, iv)); stats['id_overflow:' + c] += 1
                            else: v = str(PREFIX + iv)
                    elif typ == 'date': v = str(v)[:10]
                    elif typ in ('character varying', 'text'):
                        v = str(v)
                out.append('\\N' if v is None else v)
            if pkcols:
                key = tuple(out[list(plan).index(c)] for c in pkcols if c in plan)
                if key in pkseen: stats['dup_pk'] += 1
                pkseen.add(key)
            w.writerow(out)
    p.wait()
    unused = [h for h in hdr if h not in used and not any(callable(s) for s in over.values())]
    fn_srcs = set()
    report.append((t, at, n, list(plan), unmapped, [h for h in hdr if h not in used], dict(stats), overflow[:5]))
    sql_tables.append((t, list(plan)))

# --- tables to empty (orphans) --------------------------------------------------------------
orphans = [t for t in pg if t not in EXCLUDED and t not in KEEP and t not in TABLE_MAP]

with open(os.path.join(S, 'load.sql'), 'w') as f:
    f.write('\\set ON_ERROR_STOP on\nBEGIN;\nSET session_replication_role = replica;\n')
    for t in list(TABLE_MAP) + orphans:
        f.write(f'DELETE FROM {t};\n')
    for t, cl in sql_tables:
        f.write(f"\\copy {t} ({', '.join(cl)}) FROM '{os.path.join(OUT, t + '.csv')}' WITH (FORMAT csv, NULL '\\N')\n")
    f.write('COMMIT;\n')
    for t in list(TABLE_MAP) + orphans: f.write(f'ANALYZE {t};\n')

with open(os.path.join(S, 'report.md'), 'w') as f:
    f.write('# Tsis_MRF -> ECS conversion dry run\n\n')
    f.write(f'Loaded tables: {len(TABLE_MAP)}; emptied (no Access source): {orphans}\n\nKept untouched: {sorted(EXCLUDED | KEEP)}\n\n')
    f.write('Access tables with no ECS target (not loaded): ' + str([a for a in acc_tables if a not in TABLE_MAP.values()]) + '\n\n')
    for t, at, n, cl, unm, unused, st, ov in report:
        f.write(f'## {t} <- {at}: {n} rows\n- ECS cols with no source: {unm}\n- Access cols dropped: {unused}\n')
        if st: f.write(f'- stats: {st}\n')
        if ov: f.write(f'- ID OVERFLOW sample: {ov}\n')
    f.write('\n## Problems\n' + '\n'.join(problems) + '\n')
print('done', len(TABLE_MAP), 'tables;', len(problems), 'problems')
for t, at, n, cl, unm, unused, st, ov in report:
    flag = ' OVERFLOW' if ov else ''
    dup = f" DUP_PK={st.get('dup_pk')}" if st.get('dup_pk') else ''
    print(f'{t:34} {n:>8}{flag}{dup}')
print('PROBLEMS:', problems[:20])
print('ORPHANS:', orphans)
