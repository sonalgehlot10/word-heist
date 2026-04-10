from __future__ import annotations

import json
from pathlib import Path
from typing import Any, Dict, List

import matplotlib.pyplot as plt
import pandas as pd

CSV_FILE = "Analytics.csv"
OUTPUT_DIR = Path("checkpoint_piecharts")


def find_column(df: pd.DataFrame, candidates: List[str]) -> str:
    normalized = {c.lower().strip(): c for c in df.columns}

    for cand in candidates:
        key = cand.lower().strip()
        if key in normalized:
            return normalized[key]

    for cand in candidates:
        key = cand.lower().strip()
        for norm_name, original in normalized.items():
            if key in norm_name:
                return original

    raise KeyError(f"Could not find any of these columns: {candidates}. Found: {list(df.columns)}")


def parse_event_data(text: Any) -> Dict[str, Any] | None:
    if text is None:
        return None

    s = str(text).strip()
    if not s:
        return None

    if (s.startswith("'") and s.endswith("'")) or (s.startswith('"') and s.endswith('"')):
        s = s[1:-1].strip()

    for candidate in (s, s.replace('""', '"')):
        try:
            return json.loads(candidate)
        except json.JSONDecodeError:
            pass

    return None


def load_segments(csv_file: str) -> pd.DataFrame:
    df = pd.read_csv(csv_file)

    event_col = find_column(df, ["Event Name", "Event", "event_name"])
    data_col = find_column(df, ["Event Data", "Data", "event_data"])

    rows: List[Dict[str, Any]] = []

    for _, row in df.iterrows():
        if str(row[event_col]).strip() != "CheckpointTime/Segment":
            continue

        payload = parse_event_data(row[data_col])
        if not payload:
            continue

        try:
            rows.append(
                {
                    "sessionId": payload.get("sessionId", ""),
                    "level": payload.get("level", "Unknown"),
                    "checkpointNumber": int(payload.get("checkpointNumber", 0)),
                    "checkpointName": payload.get("checkpointName", ""),
                    "durationSeconds": float(payload.get("durationSeconds", 0.0)),
                    "reason": payload.get("reason", ""),
                }
            )
        except (TypeError, ValueError):
            continue

    segs = pd.DataFrame(rows)
    if segs.empty:
        raise ValueError("No CheckpointTime/Segment rows found in the CSV.")
    return segs


def safe_name(text: str) -> str:
    return "".join(ch if ch.isalnum() or ch in "-_" else "_" for ch in text)


def build_pie_charts(segs: pd.DataFrame) -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    summary = (
        segs.groupby(["level", "checkpointNumber"], as_index=False)["durationSeconds"]
        .sum()
        .sort_values(["level", "checkpointNumber"])
    )

    for level, level_df in summary.groupby("level"):
        level_df = level_df.sort_values("checkpointNumber")
        labels = [f"CP {int(n)}" for n in level_df["checkpointNumber"]]
        values = level_df["durationSeconds"].tolist()

        if not values or sum(values) <= 0:
            continue

        total = sum(values)

        def autopct_fmt(pct: float) -> str:
            seconds = pct * total / 100.0
            return f"{pct:.1f}%\n({seconds:.1f}s)"

        fig, ax = plt.subplots(figsize=(8, 8), dpi=160)
        ax.pie(
            values,
            labels=labels,
            autopct=autopct_fmt,
            startangle=90,
            counterclock=False,
            textprops={"fontsize": 10},
        )
        ax.set_title(f"Time spent per checkpoint - {level}")
        ax.axis("equal")
        fig.tight_layout()

        out_path = OUTPUT_DIR / f"{safe_name(level)}.png"
        fig.savefig(out_path, bbox_inches="tight")
        plt.close(fig)

        print(f"Saved {out_path.resolve()}")


def main() -> None:
    segs = load_segments(CSV_FILE)
    print(f"Loaded {len(segs)} checkpoint segments from {segs['level'].nunique()} level(s).")
    build_pie_charts(segs)


if __name__ == "__main__":
    main()