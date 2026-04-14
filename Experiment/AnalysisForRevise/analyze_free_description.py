import csv
import os

CONDITION_NAMES = {
    "1": "SpatialReference Only",
    "2": "SpatialReference + Pointing",
    "3": "Pointing Only",
    "4": "Label",
}

INPUT_FILE = os.path.join(os.path.dirname(__file__), "Result_New.csv")
OUTPUT_FILE = os.path.join(os.path.dirname(__file__), "FreeDescription_by_Condition.csv")


def main():
    # Read all data rows
    with open(INPUT_FILE, encoding="utf-8-sig", newline="") as f:
        reader = csv.reader(f)
        rows = list(reader)

    # First row is header; data starts at row index 1
    data_rows = [row for row in rows[1:] if len(row) >= 45]

    # Group by condition
    groups: dict[str, list[dict]] = {k: [] for k in CONDITION_NAMES}

    for row in data_rows:
        condition = row[2].strip()
        if condition not in groups:
            continue
        entry = {
            "ID": row[1].strip(),
            "Condition": condition,
            "ConditionName": CONDITION_NAMES[condition],
            "FreeDescription": row[43].strip(),
            "FreeDescription2": row[44].strip(),
        }
        # Skip rows where both descriptions are empty
        if entry["FreeDescription"] or entry["FreeDescription2"]:
            groups[condition].append(entry)

    # Write output CSV: sorted by condition, with blank separator rows between groups
    with open(OUTPUT_FILE, "w", encoding="utf-8-sig", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["Condition", "ConditionName", "ID", "FreeDescription", "FreeDescription2"])

        for cond_key in sorted(groups.keys()):
            cond_name = CONDITION_NAMES[cond_key]
            entries = groups[cond_key]

            # Section header row
            writer.writerow([])
            writer.writerow([f"=== Condition {cond_key}: {cond_name} ===", "", "", "", ""])

            if entries:
                for entry in entries:
                    writer.writerow([
                        entry["Condition"],
                        entry["ConditionName"],
                        entry["ID"],
                        entry["FreeDescription"],
                        entry["FreeDescription2"],
                    ])
            else:
                writer.writerow(["", "", "(記述なし)", "", ""])

    print(f"Output saved to: {OUTPUT_FILE}")

    # Print summary
    for cond_key in sorted(groups.keys()):
        print(f"  Condition {cond_key} ({CONDITION_NAMES[cond_key]}): {len(groups[cond_key])} entries")


if __name__ == "__main__":
    main()
