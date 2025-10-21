import json
from pathlib import Path
import shutil

# Paths
src = Path(r"c:\Users\tenma\Desktop\KeioSchool\InteractiveSmartHome\InteractiveSmartHome\InteractiveSmartHome\Assets\EXPERIMENT\VOICE_LOG\P2\4_D_patched.json")
if not src.exists():
    raise SystemExit(f"Source file not found: {src}")

backup = src.with_name(src.stem + "_pre_clean.json")
shutil.copy(src, backup)

with src.open('r', encoding='utf-8') as f:
    data = json.load(f)

# Test command patterns to remove (strip whitespace and full-width dash variants)
TEST_PREFIXES = ("こんにちは", "こんにち")
TEST_EXACT = ("あいうえお",)

removed_total = 0
tasks_changed = []

for task in data:
    attempts = task.get('taskAttempts', [])
    new_attempts = []
    removed_in_task = 0
    for att in attempts:
        cmd = att.get('userCommand', '')
        norm = cmd.strip()
        is_test = False
        if any(norm.startswith(p) for p in TEST_PREFIXES):
            is_test = True
        if norm in TEST_EXACT:
            is_test = True
        if is_test:
            removed_in_task += 1
        else:
            new_attempts.append(att)
    if removed_in_task:
        tasks_changed.append((task.get('taskId'), removed_in_task, len(attempts), len(new_attempts)))
    removed_total += removed_in_task

    # Update taskAttempts
    task['taskAttempts'] = new_attempts

    # Recompute taskAttemptCount as number of attempts array
    task['taskAttemptCount'] = len(new_attempts)

    # Recompute totalElapsedTime as sum of unique attemptIds' taskElapsedTime
    unique = {}
    for att in new_attempts:
        aid = att.get('attemptId')
        try:
            t = float(att.get('taskElapsedTime', 0) or 0)
        except Exception:
            t = 0.0
        # keep first-seen value for an attemptId
        if aid not in unique:
            unique[aid] = t
    total = sum(unique.values())
    task['totalElapsedTime'] = float(total)

    # finalId: last attemptId if any, else empty string
    if new_attempts:
        task['finalId'] = new_attempts[-1].get('attemptId', '')
    else:
        task['finalId'] = ""

# Write cleaned file (overwrite original patched file)
with src.open('w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)

# Print summary
print(f"backup_written={backup}")
print(f"cleaned_written={src}")
print(f"removed_total={removed_total}")
print(f"tasks_changed={len(tasks_changed)}")
for tid, removed, old_len, new_len in tasks_changed:
    print(f"task {tid}: removed={removed}, before={old_len}, after={new_len}")
