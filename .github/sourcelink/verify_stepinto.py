#!/usr/bin/env python3
"""Drive netcoredbg to prove SourceLink "step into" (F11) works end-to-end.

Sets a breakpoint in the consumer, runs to it, issues a step-into, and inspects
the resulting stack frame. Succeeds (exit 0) only if the step landed in the
expected library source file with the library's symbols loaded — i.e. a debugger
resolves real library source, not a decompiled placeholder. If SourceLink or the
symbol package were broken, the step would land with no source mapping and this
fails.

Usage:
    verify_stepinto.py <netcoredbg> <consumer.dll> <Program.cs:LINE> <expected-src.cs>
"""
import subprocess
import sys
import re
import threading
import queue
import time

if len(sys.argv) != 5:
    print(__doc__)
    sys.exit(2)

debugger, consumer_dll, break_spec, expected_src = sys.argv[1:5]

proc = subprocess.Popen(
    [debugger, "--interpreter=mi", "--", "dotnet", consumer_dll],
    stdin=subprocess.PIPE,
    stdout=subprocess.PIPE,
    stderr=subprocess.STDOUT,
    text=True,
    bufsize=1,
)

events: "queue.Queue[str]" = queue.Queue()


def _reader():
    for line in proc.stdout:
        events.put(line.rstrip("\n"))
    events.put(None)


threading.Thread(target=_reader, daemon=True).start()


def send(command):
    print(">>>", command, flush=True)
    proc.stdin.write(command + "\n")
    proc.stdin.flush()


def wait_for(needles, timeout=60):
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            line = events.get(timeout=max(0.1, deadline - time.time()))
        except queue.Empty:
            return None
        if line is None:
            return None
        print("dbg>", line, flush=True)
        if any(n in line for n in needles):
            return line
    return None


def fail(reason):
    print("RESULT=FAIL reason=" + reason, flush=True)
    try:
        send("-gdb-exit")
    except Exception:
        pass
    sys.exit(1)


send("-break-insert " + break_spec)
wait_for(["^done", "^error"], 15)
send("-exec-run")

# netcoredbg halts at the managed entry point first; continue to the breakpoint.
stop = wait_for(["*stopped"], 60)
if stop and 'reason="entry-point-hit"' in stop:
    send("-exec-continue")
    stop = wait_for(["*stopped"], 60)

if not stop or "breakpoint-hit" not in stop:
    fail("breakpoint-not-hit")

# Step into (F11). Retry a couple of times in case the first step stays on the
# call line before descending into the callee.
resolved = None
for _ in range(4):
    send("-exec-step")
    stop = wait_for(["*stopped"], 30)
    if not stop:
        break
    match = re.search(r'frame=\{[^}]*?file="([^"]+)"[^}]*?fullname="([^"]+)"', stop)
    if match and match.group(1).endswith(expected_src):
        resolved = (match.group(1), match.group(2), stop)
        break

if not resolved:
    fail("did-not-step-into-" + expected_src)

send("-gdb-exit")

source_file, fullname, stop = resolved
line_match = re.search(r'line="(\d+)"', stop)
print("RESOLVED_FILE=" + source_file, flush=True)
print("RESOLVED_FULLNAME=" + fullname, flush=True)
print("RESOLVED_LINE=" + (line_match.group(1) if line_match else "?"), flush=True)
print("RESULT=PASS", flush=True)
sys.exit(0)
