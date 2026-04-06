#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Debug script - Kiểm tra cấu trúc CSV source"""

import csv

input_file = r'c:\Users\DELL\Documents\GitHub\20_HTQLGH\Scenario New - Test Scenario.csv'

with open(input_file, 'r', encoding='utf-8-sig') as f:
    reader = csv.DictReader(f)
    
    for i, row in enumerate(reader, 1):
        if i <= 40:  # Chỉ in 40 dòng đầu
            test_id = row.get('Test Case ID', '').strip()
            scenario_lv1 = row.get('Scenario LV1', '').strip()
            scenario_lv2 = row.get('Scenario ID LV2', '').strip()
            scenario_desc = row.get('Scenario Description', '').strip()
            desc_test = row.get('Description Testcase', '').strip()
            
            if test_id:
                print(f"Row {i}: ID={test_id}, LV1={scenario_lv1}, LV2={scenario_lv2}, Desc={desc_test[:50]}")
