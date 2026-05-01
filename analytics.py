"""
Metric 1 – Death Cause Breakdown
==================================
Reads Death/Respawn event data from CSV and generates a pie chart
showing breakdown by cause: Hit Spike, Caught by Guard, Energy Depleted.

USAGE
  python3 analytics.py

Make sure the CSV file is in the same folder as this script.
"""

import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches

# ── CONFIG ────────────────────────────────────────────────────────────────────
CSV_FILE    = "analytics_gold.csv"
OUTPUT_FILE = "death_cause_breakdown.png"
# ─────────────────────────────────────────────────────────────────────────────

COLORS = {
    "Hit Spike":       "#4A90D9",
    "Caught by Guard": "#E8922A",
    "Energy Depleted": "#5BBF4E",
}
BG = "#0d0d1a"

# Load & filter only Death/Respawn events
df     = pd.read_csv(CSV_FILE)
deaths = df[df["Event name"] == "Death/Respawn"]
counts = deaths["Event Data"].value_counts()
print("Death counts:\n", counts.to_string())

labels = counts.index.tolist()
values = counts.values.tolist()
colors = [COLORS.get(l, "#888888") for l in labels]

# Plot
fig, ax = plt.subplots(figsize=(7, 6), facecolor=BG)
ax.set_facecolor(BG)

wedges, _, autotexts = ax.pie(
    values,
    colors=colors,
    autopct="%1.1f%%",
    startangle=90,
    pctdistance=0.72,
    wedgeprops=dict(linewidth=2.5, edgecolor=BG),
)
for at in autotexts:
    at.set(fontsize=13, fontweight="bold", color="white", fontfamily="monospace")

patches = [
    mpatches.Patch(color=COLORS.get(l, "#888"), label=f"{l}  ({int(v)})")
    for l, v in zip(labels, values)
]
ax.legend(
    handles=patches, loc="lower center", bbox_to_anchor=(0.5, -0.12),
    ncol=len(labels), frameon=False, fontsize=10, labelcolor="#cccccc",
)

ax.set_title(
    "Death Cause Breakdown",
    fontsize=17, fontweight="bold",
    color="#ffffff", fontfamily="monospace", pad=18,
)

fig.tight_layout()
fig.savefig(OUTPUT_FILE, dpi=150, bbox_inches="tight", facecolor=BG)
print(f"\n[SAVED] {OUTPUT_FILE}")
plt.show()