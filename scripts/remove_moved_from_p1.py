import json
from pathlib import Path
import shutil

p2_path = Path(r"c:\Users\tenma\Desktop\KeioSchool\InteractiveSmartHome\InteractiveSmartHome\InteractiveSmartHome\Assets\EXPERIMENT\VOICE_LOG\P2\4_D_patched.json")
p1_path = Path(r"c:\Users\tenma\Desktop\KeioSchool\InteractiveSmartHome\InteractiveSmartHome\InteractiveSmartHome\Assets\EXPERIMENT\VOICE_LOG\P1\4_D.json")

if not p2_path.exists():
    raise SystemExit(f"P2 file not found: {p2_path}")
if not p1_path.exists():
    raise SystemExit(f"P1 file not found: {p1_path}")

# Backup P1
backup = p1_path.with_name(p1_path.stem + "_pre_remove_moved.json")
shutil.copy(p1_path, backup)

# Load P2 and collect attemptIds
with p2_path.open('r', encoding='utf-8') as f:
    p2 = json.load(f)

p2_attempt_ids = set()
for task in p2:
    for att in task.get('taskAttempts', []):
        aid = att.get('attemptId')
        if aid:
            p2_attempt_ids.add(aid)

# Load P1 and remove attempts present in p2_attempt_ids
with p1_path.open('r', encoding='utf-8') as f:
    p1 = json.load(f)

removed_total = 0
tasks_changed = []
for task in p1:
    attempts = task.get('taskAttempts', [])
    new_attempts = []
    removed_in_task = 0
    for att in attempts:
        aid = att.get('attemptId')
        if aid in p2_attempt_ids:
            removed_in_task += 1
        else:
            new_attempts.append(att)
    if removed_in_task:
        tasks_changed.append((task.get('taskId'), removed_in_task, len(attempts), len(new_attempts)))
    removed_total += removed_in_task

    task['taskAttempts'] = new_attempts
    task['taskAttemptCount'] = len(new_attempts)

    # recompute totalElapsedTime = sum of unique attempt times
    unique = {}
    for att in new_attempts:
        aid = att.get('attemptId')
        try:
            t = float(att.get('taskElapsedTime', 0) or 0)
        except Exception:
            t = 0.0
        if aid not in unique:
            unique[aid] = t
    task['totalElapsedTime'] = float(sum(unique.values()))

    if new_attempts:
        task['finalId'] = new_attempts[-1].get('attemptId', '')
    else:
        task['finalId'] = ''

# Write back P1
with p1_path.open('w', encoding='utf-8') as f:
    json.dump(p1, f, ensure_ascii=False, indent=2)

# Summary
print(f"backup_written={backup}")
print(f"p1_updated={p1_path}")
print(f"removed_total={removed_total}")
print(f"tasks_changed={len(tasks_changed)}")
for tid, removed, before, after in tasks_changed:
    print(f"task {tid}: removed={removed}, before={before}, after={after}")
