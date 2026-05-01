# ---------------------------------------
# Google Sheets → Level-wise Analytics (Live, Clean)
# ---------------------------------------

import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import time
import os

# --- CONFIG ---
SHEET_ID = "1bH7NL1rU-_flxZLbIErJ4rLwyHGp3P8y1qTasrStaz8"
URL = f"https://docs.google.com/spreadsheets/d/{SHEET_ID}/export?format=csv"

ANALYTIC_NAME = "Reboost_vs_Stamina"

LIVE_MODE = True
REFRESH_INTERVAL = 10  # seconds

BASE_DIR = "GameAnalytics"

# Ensure folder exists
os.makedirs(BASE_DIR, exist_ok=True)

# ---------------------------------------
# LOAD + PARSE
# ---------------------------------------
def load_and_parse():
    df = pd.read_csv(URL)
    df = df[df["Event name"] == ANALYTIC_NAME]

    levels, reboosts, stamina = [], [], []

    for data in df["Event Data"].dropna():
        try:
            parts = data.split(",")

            level = parts[0].replace("Level:", "").strip()
            r = int(parts[1].replace("Reboosts:", "").strip())
            s = float(parts[2].replace("FinalStamina:", "").strip())

            levels.append(level)
            reboosts.append(r)
            stamina.append(s)

        except:
            print("Skipping bad row:", data)

    return pd.DataFrame({
        "Level": levels,
        "Reboosts": reboosts,
        "Stamina": stamina
    })

# ---------------------------------------
# 1. GROUPED BAR
# ---------------------------------------
def plot_grouped_bar(df_plot):
    if df_plot.empty:
        return

    grouped_mean = df_plot.groupby("Level").mean()
    grouped_std = df_plot.groupby("Level").std()

    x = np.arange(len(grouped_mean.index))
    width = 0.35

    plt.figure(figsize=(8, 5))

    plt.bar(x - width/2, grouped_mean["Reboosts"], width,
            yerr=grouped_std["Reboosts"], capsize=5, label="Avg Reboosts")

    plt.bar(x + width/2, grouped_mean["Stamina"], width,
            yerr=grouped_std["Stamina"], capsize=5, label="Avg Stamina")

    plt.xticks(x, grouped_mean.index)
    plt.xlabel("Level")
    plt.ylabel("Value")
    plt.title("Reboosts vs Final Stamina")
    plt.legend()
    plt.grid(axis='y')

    plt.tight_layout()
    plt.savefig(f"{BASE_DIR}/grouped_bar.png")
    plt.close()

# ---------------------------------------
# 2. SCATTER
# ---------------------------------------
def plot_scatter(df_plot):
    if df_plot.empty:
        return

    plt.figure(figsize=(6, 5))

    for level in df_plot["Level"].unique():
        subset = df_plot[df_plot["Level"] == level]
        plt.scatter(subset["Reboosts"], subset["Stamina"], label=level, alpha=0.7)

    plt.xlabel("Reboosts")
    plt.ylabel("Final Stamina")
    plt.title("Player Behavior by Level")
    plt.legend()
    plt.grid()

    plt.tight_layout()
    plt.savefig(f"{BASE_DIR}/scatter.png")
    plt.close()

# ---------------------------------------
# 3. BOX PLOT
# ---------------------------------------
def plot_box(df_plot):
    if df_plot.empty:
        return

    plt.figure(figsize=(6, 5))

    df_plot.boxplot(column="Stamina", by="Level")

    plt.title("Stamina Distribution by Level")
    plt.suptitle("")
    plt.xlabel("Level")
    plt.ylabel("Stamina")

    plt.tight_layout()
    plt.savefig(f"{BASE_DIR}/boxplot.png")
    plt.close()

# ---------------------------------------
# MAIN
# ---------------------------------------
def run_once():
    df_plot = load_and_parse()

    if df_plot.empty:
        print("No data yet.")
        return

    print(f"Data points: {len(df_plot)}")

    plot_grouped_bar(df_plot)
    plot_scatter(df_plot)
    plot_box(df_plot)

    print("Graphs updated.")

if LIVE_MODE:
    while True:
        try:
            run_once()
            time.sleep(REFRESH_INTERVAL)
        except Exception as e:
            print("Error:", e)
            time.sleep(REFRESH_INTERVAL)
else:
    run_once()