"""Export the reviewed ECS society directory for PostgreSQL COPY.

Usage: python export_csv.py ECS_Societies_Codes_Review.xlsx societies.csv
Requires openpyxl. This does not alter the source workbook.
"""

import csv
import hashlib
import re
import sys
from pathlib import Path

from openpyxl import load_workbook


EXPECTED_HEADERS = (
    "S.No", "Region", "Circle", "Block", "Society Type", "Society Name",
    "Bank Name", "Bank Account Number", "IFSC Code", "BRCode", "Region Code",
    "Circle Code", "Society Code", "Proposed Combined Society Code",
    "Pattern for Review",
)
OUTPUT_HEADERS = (
    "source_serial_number", "region", "circle", "block", "society_type",
    "society_name", "bank_name", "bank_account_number", "ifsc_code",
    "region_code", "circle_code", "society_code",
    "proposed_combined_society_code", "pattern_for_review",
)


def main(source: Path, destination: Path) -> None:
    workbook = load_workbook(source, read_only=True, data_only=True)
    sheet = workbook["ECS"]
    rows = sheet.values
    if tuple(next(rows)) != EXPECTED_HEADERS:
        raise ValueError("Unexpected ECS worksheet columns")

    seen_serials = set()
    seen_codes = set()
    count = 0
    with destination.open("w", encoding="utf-8", newline="") as output:
        writer = csv.writer(output)
        writer.writerow(OUTPUT_HEADERS)
        for source_row in rows:
            if all(value is None for value in source_row):
                continue
            if len(source_row) != len(EXPECTED_HEADERS):
                raise ValueError(f"Wrong column count on worksheet row {count + 2}")
            if source_row[9] is not None:
                raise ValueError(f"BRCode unexpectedly populated on worksheet row {count + 2}")
            selected = [value for index, value in enumerate(source_row) if index != 9]
            if any(value is None or str(value) == "" for value in selected):
                raise ValueError(f"Empty required value on worksheet row {count + 2}")

            serial = int(source_row[0])
            region_code = int(source_row[10])
            circle_code = int(source_row[11])
            society_code = int(source_row[12])
            combined = str(source_row[13])
            expected = f"{region_code:02d}-{circle_code:03d}-{society_code:04d}"
            if not re.fullmatch(r"\d{2}-\d{3}-\d{4}", combined) or combined != expected:
                raise ValueError(f"Inconsistent proposed code on worksheet row {count + 2}")
            if serial in seen_serials or combined in seen_codes:
                raise ValueError(f"Duplicate serial or proposed code on worksheet row {count + 2}")
            seen_serials.add(serial)
            seen_codes.add(combined)

            # The workbook contains free-text account and IFSC review entries;
            # preserve them exactly rather than interpreting them as bank details.
            writer.writerow([str(value) for value in selected])
            count += 1

    digest = hashlib.sha256(source.read_bytes()).hexdigest()
    print(f"rows={count} source_sha256={digest} csv={destination}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit("Usage: export_csv.py SOURCE.xlsx DESTINATION.csv")
    main(Path(sys.argv[1]), Path(sys.argv[2]))
