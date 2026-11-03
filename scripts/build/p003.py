#!/usr/bin/env python3
"""Reproduce P00.3 configure/test/APK checks with the local pinned Unity Editor."""
import argparse
from datetime import datetime
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from zoneinfo import ZoneInfo

ROOT = Path(__file__).resolve().parents[2]
VERSION = "6000.6.7f1"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("step", choices=["configure", "editmode", "playmode", "android"])
    parser.add_argument("--editor", type=Path, default=Path(os.environ.get(
        "NARD_UNITY_EDITOR", str(Path.home() / "Unity/Hub/Editor" / VERSION / "Editor/Unity"))))
    args = parser.parse_args()
    artifacts = ROOT / "build/P00.3"
    artifacts.mkdir(parents=True, exist_ok=True)
    log = artifacts / (args.step + ".raw.log")
    command = [str(args.editor), "-batchmode", "-nographics", "-projectPath", str(ROOT / "unity"),
               "-logFile", str(log)]
    xml = artifacts / (args.step + ".xml")
    if args.step in ["editmode", "playmode"]:
        # Do not pass -quit: Unity Test Framework owns termination of test runs.
        command += ["-runTests", "-testPlatform", "EditMode" if args.step == "editmode" else "PlayMode",
                    "-testResults", str(xml)]
        xml.unlink(missing_ok=True)
    else:
        if args.step == "android":
            command += ["-buildTarget", "Android"]
        command += ["-executeMethod", "Nard.Build.P003Build." +
                    ("Configure" if args.step == "configure" else "BuildAndroid"), "-quit"]
    env = {**os.environ, "NARD_BUILD_OUTPUT": str(artifacts / "NARD-P00.3.apk")}
    started = datetime.now(ZoneInfo("Asia/Baku")).isoformat(timespec="seconds")
    try:
        result = subprocess.run(command, cwd=ROOT, env=env, capture_output=True, text=True, timeout=1800)
        exit_code = result.returncode
        (artifacts / (args.step + ".process.raw.log")).write_text(result.stdout + result.stderr)
    except subprocess.TimeoutExpired:
        exit_code = 124
    text = log.read_text(errors="replace") if log.exists() else ""
    diagnostics = sorted(set(re.findall(r"^Assets/.*(?:error|warning) CS.*$", text, re.M)))
    summary = {"step": args.step, "editor_version": VERSION, "started_at": started,
               "exit_code": exit_code, "status": "PASS" if exit_code == 0 else "FAIL",
               "raw_log": str(log.relative_to(ROOT)), "compiler_diagnostics": diagnostics}
    if args.step in ["editmode", "playmode"]:
        if xml.exists():
            report = ET.parse(xml).getroot()
            summary["tests"] = {key: report.attrib.get(key) for key in ["result", "total", "passed", "failed", "skipped"]}
            if report.attrib.get("result") != "Passed" or int(report.attrib.get("total", "0")) == 0:
                summary["status"] = "FAIL"
        else:
            summary["status"] = "FAIL"
            summary["reason"] = "No NUnit XML produced"
    if args.step == "android":
        apk = artifacts / "NARD-P00.3.apk"
        summary["apk_exists"] = apk.exists()
        summary["apk_bytes"] = apk.stat().st_size if apk.exists() else 0
        if not apk.exists(): summary["status"] = "FAIL"
    (artifacts / (args.step + ".json")).write_text(json.dumps(summary, indent=2) + "\n")
    print(json.dumps(summary, indent=2))
    return 0 if summary["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
