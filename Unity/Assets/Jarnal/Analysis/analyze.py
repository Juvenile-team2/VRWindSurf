"""
Jarnal experiment log analyzer.

Usage:
    python analyze.py <cube_position.csv> [--camera <camerarig_position.csv>]

Output:
    <csv_dir>/figures/
        wave_Y.png        -- Cube Y position (wave surface trace)
        rotation_X.png    -- Roll (rot_x) over time
        rotation_Y.png    -- Yaw  (rot_y) over time
        rotation_Z.png    -- Pitch(rot_z) over time
        rotation_integrated.png -- Cumulative angle per axis
"""

import argparse
import sys
from pathlib import Path

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
from scipy.integrate import cumulative_trapezoid

# ── helpers ──────────────────────────────────────────────────────────────────

def wrap180(angles: np.ndarray) -> np.ndarray:
    """Unity eulerAngles は 0-360。解析用に -180〜180 に変換する。"""
    return (angles + 180) % 360 - 180


def load_cube(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)
    df.columns = df.columns.str.strip()
    for col in ["rot_x", "rot_y", "rot_z"]:
        df[col] = wrap180(df[col].values)
    return df


def load_camera(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)
    df.columns = df.columns.str.strip()
    for col in ["rot_x", "rot_y", "rot_z"]:
        df[col] = wrap180(df[col].values)
    return df


def ensure_figures_dir(csv_path: str) -> Path:
    out = Path(csv_path).parent / "figures"
    out.mkdir(exist_ok=True)
    return out


# ── individual plots ──────────────────────────────────────────────────────────

AXIS_LABEL = {"x": "X (Roll)", "y": "Y (Yaw)", "z": "Z (Pitch)"}
COLOR      = {"x": "#e05c5c", "y": "#5ca0e0", "z": "#5ce07a"}


def plot_wave_y(df: pd.DataFrame, fig_dir: Path):
    fig, ax = plt.subplots(figsize=(12, 4))
    ax.plot(df["time_sec"], df["world_y"], color="#5ca0e0", linewidth=0.8)
    ax.set_title("Wave surface trace — Cube Y position")
    ax.set_xlabel("Time (s)")
    ax.set_ylabel("World Y (m)")
    ax.xaxis.set_minor_locator(ticker.AutoMinorLocator())
    ax.yaxis.set_minor_locator(ticker.AutoMinorLocator())
    ax.grid(True, which="major", linestyle="--", alpha=0.5)
    ax.grid(True, which="minor", linestyle=":",  alpha=0.25)
    fig.tight_layout()
    path = fig_dir / "wave_Y.png"
    fig.savefig(path, dpi=150)
    plt.close(fig)
    print(f"  saved: {path}")


def plot_rotation_axis(df: pd.DataFrame, axis: str, fig_dir: Path):
    col = f"rot_{axis}"
    fig, ax = plt.subplots(figsize=(12, 4))
    ax.plot(df["time_sec"], df[col], color=COLOR[axis], linewidth=0.8)
    ax.axhline(0, color="black", linewidth=0.5, linestyle="--")
    ax.set_title(f"Rotation {AXIS_LABEL[axis]} over time")
    ax.set_xlabel("Time (s)")
    ax.set_ylabel("Angle (deg)")
    ax.xaxis.set_minor_locator(ticker.AutoMinorLocator())
    ax.yaxis.set_minor_locator(ticker.AutoMinorLocator())
    ax.grid(True, which="major", linestyle="--", alpha=0.5)
    ax.grid(True, which="minor", linestyle=":",  alpha=0.25)
    fig.tight_layout()
    path = fig_dir / f"rotation_{axis.upper()}.png"
    fig.savefig(path, dpi=150)
    plt.close(fig)
    print(f"  saved: {path}")


def plot_rotation_integrated(df: pd.DataFrame, fig_dir: Path):
    t = df["time_sec"].values
    fig, axes = plt.subplots(3, 1, figsize=(12, 9), sharex=True)
    fig.suptitle("Cumulative rotation (∫ω dt) per axis")

    for ax, axis in zip(axes, ["x", "y", "z"]):
        col = f"rot_{axis}"
        # 角速度の近似: Δangle / Δt
        dangle = np.gradient(df[col].values, t)
        cumulative = cumulative_trapezoid(dangle, t, initial=0)
        ax.plot(t, cumulative, color=COLOR[axis], linewidth=0.8)
        ax.axhline(0, color="black", linewidth=0.5, linestyle="--")
        ax.set_ylabel(f"{AXIS_LABEL[axis]} (deg)")
        ax.grid(True, linestyle="--", alpha=0.5)

    axes[-1].set_xlabel("Time (s)")
    fig.tight_layout()
    path = fig_dir / "rotation_integrated.png"
    fig.savefig(path, dpi=150)
    plt.close(fig)
    print(f"  saved: {path}")


def plot_rotation_overview(df: pd.DataFrame, fig_dir: Path):
    """3軸まとめて1枚に表示する概観グラフ。"""
    t = df["time_sec"].values
    fig, axes = plt.subplots(3, 1, figsize=(12, 9), sharex=True)
    fig.suptitle("Rotation X / Y / Z over time")

    for ax, axis in zip(axes, ["x", "y", "z"]):
        ax.plot(t, df[f"rot_{axis}"].values, color=COLOR[axis], linewidth=0.8)
        ax.axhline(0, color="black", linewidth=0.5, linestyle="--")
        ax.set_ylabel(f"{AXIS_LABEL[axis]} (deg)")
        ax.grid(True, linestyle="--", alpha=0.5)

    axes[-1].set_xlabel("Time (s)")
    fig.tight_layout()
    path = fig_dir / "rotation_overview.png"
    fig.savefig(path, dpi=150)
    plt.close(fig)
    print(f"  saved: {path}")


def plot_camera_overview(df: pd.DataFrame, fig_dir: Path):
    """CameraRig の位置と回転の概観。"""
    t = df["time_sec"].values
    fig, axes = plt.subplots(2, 1, figsize=(12, 8), sharex=True)
    fig.suptitle("CameraRig (Head) — World position Y & Rotation")

    axes[0].plot(t, df["world_y"], color="#5ca0e0", linewidth=0.8)
    axes[0].set_ylabel("World Y (m)")
    axes[0].grid(True, linestyle="--", alpha=0.5)

    for axis in ["x", "y", "z"]:
        axes[1].plot(t, df[f"rot_{axis}"].values, color=COLOR[axis],
                     linewidth=0.8, label=AXIS_LABEL[axis])
    axes[1].axhline(0, color="black", linewidth=0.5, linestyle="--")
    axes[1].set_ylabel("Angle (deg)")
    axes[1].legend(loc="upper right", fontsize=8)
    axes[1].grid(True, linestyle="--", alpha=0.5)

    axes[-1].set_xlabel("Time (s)")
    fig.tight_layout()
    path = fig_dir / "camera_overview.png"
    fig.savefig(path, dpi=150)
    plt.close(fig)
    print(f"  saved: {path}")


# ── main ──────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description="Jarnal log analyzer")
    parser.add_argument("cube_csv", help="cube_position.csv path")
    parser.add_argument("--camera", default=None,
                        help="camerarig_position.csv path (optional)")
    args = parser.parse_args()

    print(f"Loading: {args.cube_csv}")
    cube = load_cube(args.cube_csv)
    fig_dir = ensure_figures_dir(args.cube_csv)
    print(f"Output : {fig_dir}")

    plot_wave_y(cube, fig_dir)
    for axis in ["x", "y", "z"]:
        plot_rotation_axis(cube, axis, fig_dir)
    plot_rotation_integrated(cube, fig_dir)
    plot_rotation_overview(cube, fig_dir)

    if args.camera:
        print(f"Loading camera: {args.camera}")
        cam = load_camera(args.camera)
        plot_camera_overview(cam, fig_dir)

    print("Done.")


if __name__ == "__main__":
    main()
