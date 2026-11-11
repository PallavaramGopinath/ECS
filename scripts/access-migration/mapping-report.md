# Tsis_MRF -> ECS conversion dry run

Loaded tables: 77; emptied (no Access source): ['jl_processingfees', 'loan_members', 'locker_allotments', 'locker_closures', 'locker_rent_adjustments', 'staging_balance', 'staging_details', 'staging_history', 'staging_master']

Kept untouched: ['account_transactions', 'account_voctype', 'bank_master', 'business_day', 'gen_bank_name', 'interestapplication_onmemexpiry', 'jl_dailymarketrate', 'jl_loaneligible', 'jl_max_limit', 'loan_scheme_deduction_rules', 'loan_schemes_group', 'locker_rent_template', 'locker_settings', 'locker_size_master', 'locker_status_master', 'lockers', 'map_general', 'map_suspenseaccounts', 'mem_retirementage', 'menu_forms', 'menu_main', 'menu_sub', 'pay_component_assignments', 'pay_components', 'pay_vpf', 'refer_constituency', 'refresh_tokens', 'reports_group', 'reports_master', 'society_neft', 'society_options', 'users']

Access tables with no ECS target (not loaded): ['Bank_Master', 'Borr_Details', 'Borr_Inst', 'Borr_roi', 'Borr_Trn', 'Borr_Trn_Tr', 'Chequebook_Details', 'Deposit_Roi_Template1', 'fd_gen_template', 'fd_in_lien', 'Fin_db_ob', 'Fin_GL', 'Fin_GL_Tr', 'Fin_PNL_BS', 'Fin_PNL_BS_Adj', 'Fin_PNL_BS_Tr', 'Fin_Report', 'Fin_RNC', 'Gen_Bank_Name', 'IFSC_CODE', 'JL_Ornments_Master', 'JL_prefix', 'JL_StockReg', 'loan_cls', 'loan_cls_trn', 'Loan_dcb', 'Loan_Elig_AD_Details', 'Loan_Elig_AD_Master', 'Loan_Elig_Outstanding', 'Loan_Eligibility_Template', 'Loan_Ledger', 'loan_mem', 'Loan_MIS', 'Loan_Overdue', 'Loan_PI_Rate', 'Loan_Pur_Agency', 'Loan_Pur_Grp', 'Loan_Pur_SubGrp', 'Loan_RestrictedDemand', 'Loan_SchemeCode', 'Loan_Schemes_Deductables', 'Map_Fees', 'Map_Fin', 'Map_General_Sus', 'Map_Mem_Sus', 'Mem_Demand_Report', 'Mem_Div', 'Mem_Div_Trn', 'Mem_Fees', 'Mem_Fees_Tr', 'Mem_GI', 'Mem_GI_Master', 'Mem_GI_Trn', 'Mem_PassBookStatement', 'Mem_Payable_Report', 'Mem_Report', 'Mem_SCFromExcelsheet', 'Mem_Section_Master', 'Mem_Trade_Master', 'Mem_TSIS_Schedule', 'MemberSalary', 'Paste Errors', 'Pay_Advance_Scheme', 'Pay_Advance_Trn', 'Pay_Ass_All_Ded', 'Pay_Association', 'pay_slip_rep', 'PortingData', 'PortingData_ImportErrors', 'PortingData_MRF', 'Receipt_Cheque_Details', 'refer_caste', 'Refer_Modules', 'refer_occ', 'Refer_Religion', 'Refer_Sec_Level', 'Refer_Supervisor', 'Refer_Users', 'Reports', 'Reports_Group', 'rpt_can', 'rpt_can_tr', 'RsInWords', 'User_SecurityDetails', 'Z_AccNos', 'Z_AccNos2', 'Z_Add', 'Borr_Disb', 'Fin_PNL', 'Fin_RNC_Tr', 'Gen_Template', 'Loan_demand', 'Loan_Pur_Mast', 'Mem_Div_Stat', 'Mem_PassBookStatement_Tr', 'Mem_PR', 'Mem_Transfer_Account', 'MemberSalary_Template', 'Name AutoCorrect Save Failures', 'refer_comm', 'Refer_Rights', 'round_template', 'Transactions', 'Z_AdmnDt']

## deposit_trn <- Deposit_Trn: 743094 rows
- ECS cols with no source: []
- Access cols dropped: ['td_id', 'ddstatus', 'dd_delete', 'acc_id', 'bal_int']
- stats: {'id_overflow:voc_id': 9298}
- ID OVERFLOW sample: [('voc_id', -1), ('voc_id', -1), ('voc_id', -1), ('voc_id', -1), ('voc_id', -1)]
## emp_job_history <- Emp_Job_History: 3 rows
- ECS cols with no source: []
- Access cols dropped: []
## emp_master <- Emp_Master: 2 rows
- ECS cols with no source: []
- Access cols dropped: ['emp_religion_id', 'emp_caste_id', 'emp_community_id']
## emp_pf <- Emp_pf: 203 rows
- ECS cols with no source: ["pf_type(filled '0')"]
- Access cols dropped: []
## emp_qualification <- Emp_Qualification: 0 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_gen_template <- Fin_Gen_Template: 49 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_ledger <- Fin_Ledger: 158 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_ledger_fnl <- Fin_Ledger_Fnl: 4 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_ledger_grp <- Fin_Ledger_Grp: 29 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_ledger_subgrp <- Fin_ledger_SubGrp: 59 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_ledger_trn <- Fin_Ledger_Trn: 1792 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_voucher <- Fin_Voucher: 9737 rows
- ECS cols with no source: []
- Access cols dropped: []
- stats: {'notnull_filled:voc_pmt_slno': 130, 'notnull_filled:voc_amt': 130, 'notnull_filled:voc_slno': 409}
## fin_yr_master <- Fin_Yr_Master: 12 rows
- ECS cols with no source: []
- Access cols dropped: []
## jl_details <- JL_Details: 0 rows
- ECS cols with no source: ['jewelsimagepath(NULL)', "marketratepergram(filled '0')", "eligiblepercentageonmarketvalue(filled '0')"]
- Access cols dropped: []
## jl_ornments <- JL_Ornments: 0 rows
- ECS cols with no source: ["jlo_delete(filled 'f')"]
- Access cols dropped: []
## lien <- Lien: 0 rows
- ECS cols with no source: ["lien_oe(filled 'f')"]
- Access cols dropped: []
## loan_disb <- Loan_Disb: 6366 rows
- ECS cols with no source: []
- Access cols dropped: []
## loan_eligibility <- Loan_Eligibility: 2618 rows
- ECS cols with no source: []
- Access cols dropped: []
- stats: {'notnull_filled:societydeductions': 2618, 'notnull_filled:periodofloan': 2618}
## loan_inst <- Loan_Inst: 6466 rows
- ECS cols with no source: []
- Access cols dropped: []
## loan_master <- Loan_Master: 6366 rows
- ECS cols with no source: ["securitytype(filled '0')"]
- Access cols dropped: ['emp_id', 'disb_amt', 'disb_date', 'loan_os', 'nonprlod_amt', 'prl_schedule', 'prl_demand', 'prlod_amt', 'intod_amt', 'pidue_amt', 'ioddue_amt', 'intdue_amt', 'picoll_amt', 'iodcoll_amt', 'intcoll_amt', 'prlcoll_amt']
- stats: {'notnull_filled:securityfacevalue': 409, 'notnull_filled:drawingpower': 409}
## loan_repayment_schedule <- Loan_Repayment_Schedule: 0 rows
- ECS cols with no source: ["isdemandraised(filled 'f')", "demand_delete(filled 'f')", "voc_id(filled '0')", "usr_id(filled '0')", "yr_id(filled '0')"]
- Access cols dropped: []
## loan_roi <- Loan_roi: 7082 rows
- ECS cols with no source: []
- Access cols dropped: []
## loan_roi_template <- Loan_roi_template: 12 rows
- ECS cols with no source: []
- Access cols dropped: []
## loan_sanction <- Loan_Sanction: 6347 rows
- ECS cols with no source: ["da(filled '0')", "lumpsum(filled '0')", "fwd(filled '0')"]
- Access cols dropped: []
- stats: {'notnull_filled:loanelig_id': 6347, 'notnull_filled:sl_prlos': 209, 'notnull_filled:loan_id': 1078, 'notnull_filled:voc_id': 1078}
## loan_schemes <- Loan_Schemes: 14 rows
- ECS cols with no source: ["grp_id(filled '0')"]
- Access cols dropped: ['pur_id', 'subgrp_id', 'scheme_code', 'mmbl', 'ismmblmandatory']
- stats: {'notnull_filled:staffloan_int_type': 4, 'notnull_filled:adoptloaneligibility': 10, 'notnull_filled:deductpreviousloans': 10, 'notnull_filled:matchsharecapital': 10, 'notnull_filled:adoptloanlimit': 10, 'notnull_filled:issuritymemberrequired': 10, 'notnull_filled:loannostartwith': 1}
## loan_trn <- Loan_Trn: 219134 rows
- ECS cols with no source: []
- Access cols dropped: ['prl_os', 'prl_od', 'int_bal', 'iod_bal', 'pi_bal']
- stats: {'id_overflow:voc_id': 54}
- ID OVERFLOW sample: [('voc_id', -1), ('voc_id', -1), ('voc_id', -1), ('voc_id', -1), ('voc_id', -1)]
## map_banks <- Map_Banks: 3 rows
- ECS cols with no source: ['accountno(NULL)']
- Access cols dropped: []
## mem_address <- Mem_address: 0 rows
- ECS cols with no source: ['address_id(generated 1..n)', "address_delete(filled 'f')"]
- Access cols dropped: []
## mem_demand <- Mem_Demand: 452896 rows
- ECS cols with no source: ["dueby_arrear2(filled '0')", "dueby_current2(filled '0')", "dueby_total2(filled '0')"]
- Access cols dropped: ['pitotaldemand', 'inttotaldemand', 'prltotaldemand', 'totaldepositamount', 'totalpiondeposit', 'pitotalcollection', 'inttotalcollection', 'prltotalcollection', 'totaldepositcollection', 'totalpiondepositcollection', 'penalinterestcalculatedamount', 'interestcalculatedamount', 'memdemand_delete', 'prl_schedforstopdemand', 'intcalcforstopdemand', 'perno', 'pername', 'pitotaldemand2', 'inttotaldemand2', 'prltotaldemand2', 'totaldepositamount2', 'totalpiondeposit2', 'pionrdtotalamount', 'pionrdtotalamount2', 'rdreceiptamount2', 'pionrdreceiptamount2', 'totaldemand', 'totalloanarreardemand', 'totalloancurrentdemand', 'totalrddemand', 'totaldepositdemand']
- stats: {'notnull_filled:rd_pi_application_date2': 452896, 'notnull_filled:dueby_led_id': 185748, 'notnull_filled:dueby_arrear': 185748, 'notnull_filled:dueby_current': 185748, 'notnull_filled:dueby_total': 185748, 'notnull_filled:dueby_arrear_collection': 185748, 'notnull_filled:dueby_current_collection': 185748, 'notnull_filled:dueby_total_collection': 185748, 'notnull_filled:restricted_demand': 46851}
## mem_demand_master <- Mem_Demand_Master: 155 rows
- ECS cols with no source: ["societytype(filled '0')", "scheme_id(filled '0')"]
- Access cols dropped: []
## mem_demand_status <- Mem_Demand_Status: 465 rows
- ECS cols with no source: []
- Access cols dropped: ['demstatus_delete']
## mem_demand_stop <- Mem_Demand_Stop: 49 rows
- ECS cols with no source: []
- Access cols dropped: ['stopdemand_delete']
## mem_master <- Mem_Master: 1303 rows
- ECS cols with no source: ["prearea_id(filled '0')", "perarea_id(filled '0')", "grosspay(filled '0')", 'aadharcardpath(NULL)', 'salutation(NULL)', 'relation_type(NULL)', 'constituency_id(NULL)']
- Access cols dropped: ['existingmemberid', 'surityperno', 'memnotmp', 'empno', 'pay_level']
- stats: {'notnull_filled:memberstatus': 1, 'id_overflow:suretymem_id': 90}
- ID OVERFLOW sample: [('suretymem_id', -2), ('suretymem_id', -2), ('suretymem_id', -2), ('suretymem_id', -2), ('suretymem_id', -2)]
## mem_passbook <- Mem_PassBook: 898 rows
- ECS cols with no source: ['passbook_id(generated 1..n)', "trnslno(filled '0')"]
- Access cols dropped: []
## mem_payable <- Mem_Payable: 381952 rows
- ECS cols with no source: ["interest_paid(filled '0')"]
- Access cols dropped: []
- stats: {'notnull_filled:acc_id': 149800}
## mem_payable_master <- Mem_Payable_Master: 51 rows
- ECS cols with no source: []
- Access cols dropped: []
## mem_payable_notneft <- Mem_Payable_NotNEFT: 0 rows
- ECS cols with no source: []
- Access cols dropped: ['memberno']
## mem_trn <- Mem_Trn: 26034 rows
- ECS cols with no source: []
- Access cols dropped: []
- stats: {'notnull_filled:demand_amt': 7001}
## pay_all_ded <- Pay_All_Ded: 86 rows
- ECS cols with no source: []
- Access cols dropped: []
- stats: {'notnull_filled:all_status': 74, 'notnull_filled:all_percentage': 74, 'notnull_filled:all_maxamount': 74}
## pay_all_ded_master <- Pay_All_Ded_Master: 9 rows
- ECS cols with no source: []
- Access cols dropped: []
## pay_att <- Pay_Att: 171 rows
- ECS cols with no source: []
- Access cols dropped: ['att_id']
## pay_basicpay <- Pay_BasicPay: 24 rows
- ECS cols with no source: []
- Access cols dropped: []
## pay_da_template <- Pay_DA_Template: 62 rows
- ECS cols with no source: []
- Access cols dropped: []
## pay_gen_info <- Pay_Gen_Info: 9 rows
- ECS cols with no source: ["pay_info_delete(filled 'f')"]
- Access cols dropped: []
## pay_init <- Pay_init: 186 rows
- ECS cols with no source: []
- Access cols dropped: []
## pay_leave_eligible <- Pay_Leave_Eligible: 0 rows
- ECS cols with no source: ["lv_delete(filled 'f')"]
- Access cols dropped: []
## pay_pf_roitemplate <- Pay_PF_ROITemplate: 1 rows
- ECS cols with no source: []
- Access cols dropped: []
## pay_pf_template <- Pay_PF_Template: 1 rows
- ECS cols with no source: []
- Access cols dropped: []
- stats: {'notnull_filled:exgratia_led_id': 1, 'notnull_filled:bonus_led_id': 1}
## pay_retirement <- Pay_Retirement: 0 rows
- ECS cols with no source: ["ret_delete(filled 'f')"]
- Access cols dropped: []
## pay_slip <- Pay_Slip: 218 rows
- ECS cols with no source: ['payslip_id(generated 1..n)']
- Access cols dropped: []
- stats: {'notnull_filled:pay_sls': 211, 'notnull_filled:pay_exgratia': 214, 'notnull_filled:pay_bonus': 214, 'notnull_filled:pay_pp_earned': 15, 'notnull_filled:pay_gradepay_earned': 15}
## pay_template <- Pay_Template: 1 rows
- ECS cols with no source: ['paytemplate_id(generated 1..n)']
- Access cols dropped: []
## refer_area <- refer_area: 0 rows
- ECS cols with no source: ["district_id(filled '0')", "area_delete(filled 'f')"]
- Access cols dropped: []
## refer_data <- refer_data: 14 rows
- ECS cols with no source: ["referdelete(filled 'f')"]
- Access cols dropped: []
## refer_district <- refer_district: 1 rows
- ECS cols with no source: []
- Access cols dropped: []
## refer_taluk <- refer_taluk: 1 rows
- ECS cols with no source: []
- Access cols dropped: []
## sbca_master <- SBCA_Master: 0 rows
- ECS cols with no source: []
- Access cols dropped: []
## sbca_schemes <- SBCA_Schemes: 0 rows
- ECS cols with no source: ["interestcalcperiod(filled '0')"]
- Access cols dropped: []
## termdeposit_fctemplate <- TermDeposit_FcTemplate: 1 rows
- ECS cols with no source: []
- Access cols dropped: []
## termdeposit_loaneligibletemplate <- TermDeposit_LoanEligibleTemplate: 2 rows
- ECS cols with no source: ["td_delete(filled 'f')"]
- Access cols dropped: []
## termdeposit_master <- TermDeposit_Master: 11 rows
- ECS cols with no source: ['renewaltd_no(NULL)']
- Access cols dropped: []
- stats: {'notnull_filled:renewaltd_id': 6}
## termdeposit_members <- TermDeposit_Members: 11 rows
- ECS cols with no source: ['tdmem_id(generated 1..n)']
- Access cols dropped: []
## termdeposit_roi_template <- TermDeposit_roi_Template: 15 rows
- ECS cols with no source: []
- Access cols dropped: []
- stats: {'notnull_filled:usr_id': 15, 'notnull_filled:yr_id': 15}
## termdeposit_schemes <- TermDeposit_Schemes: 6 rows
- ECS cols with no source: []
- Access cols dropped: []
## termdeposit_trn <- TermDeposit_Trn: 23 rows
- ECS cols with no source: ["tdcalc_id(filled '0')"]
- Access cols dropped: []
## fin_voucher_trn <- Fin_voucher_tr: 47370 rows
- ECS cols with no source: ["loan_id(filled '0')", "outstanding(filled '0')", "balance(filled '0')"]
- Access cols dropped: []
## fin_voucher_bank <- Gen_Voc_Bank: 7345 rows
- ECS cols with no source: []
- Access cols dropped: []
## fin_voucher_bank_trn <- Gen_Voc_Bank_Tr: 0 rows
- ECS cols with no source: []
- Access cols dropped: []
## loan_sanction_trn <- Loan_Sanction_Tr: 36151 rows
- ECS cols with no source: []
- Access cols dropped: []
- stats: {'notnull_filled:loanelig_id': 36151, 'notnull_filled:recscheme_id': 36151}
## pay_slip_trn <- Pay_Slip_Tr: 539 rows
- ECS cols with no source: ['component_id(NULL)']
- Access cols dropped: []
## pay_slip_loan_trn <- Pay_Slip_Loan_Tr: 222 rows
- ECS cols with no source: []
- Access cols dropped: []
## lien_trn <- Lien_Tr: 0 rows
- ECS cols with no source: []
- Access cols dropped: []
## termdeposit_intcalc_calendar <- TermDeposit_IntCalc_Calender: 4 rows
- ECS cols with no source: []
- Access cols dropped: []
## deposit_masters <- Deposit_Master: 3 rows
- ECS cols with no source: []
- Access cols dropped: ['dm_delete', 'isinteresttransfertodeposit']
## deposit_options <- Depositor_Option: 374 rows
- ECS cols with no source: []
- Access cols dropped: ['isdo_closed', 'do_closeddate', 'do_delete']
## deposit_roi_templates <- Deposit_Roi_Template: 3 rows
- ECS cols with no source: []
- Access cols dropped: ['dmroi_delete']
## mem_collection_priority <- Mem_CollectionPriority: 20 rows
- ECS cols with no source: ["usr_id(filled '0')"]
- Access cols dropped: ['priority_delete']
## emp_pfcalccalendar <- Emp_PFCalcCalender: 0 rows
- ECS cols with no source: []
- Access cols dropped: []

## Problems

