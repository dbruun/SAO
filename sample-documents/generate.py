#!/usr/bin/env python3
"""
generate.py – Regenerate the sample driver's licence PNG test documents.

Usage:
    pip install Pillow
    python3 sample-documents/generate.py
"""

from __future__ import annotations

import os
import random
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    raise SystemExit("Pillow is required: pip install Pillow")

OUTPUT_DIR = Path(__file__).parent

CARD_W, CARD_H = 850, 540
BG_COLOR = (240, 248, 255)
HEADER_COLOR = (0, 70, 140)
TEXT_COLOR = (20, 20, 20)
LABEL_COLOR = (80, 80, 80)
LINE_COLOR = (0, 70, 140)


def _load_font(size: int, bold: bool = True) -> ImageFont.FreeTypeFont:
    bold_paths = [
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
        "/usr/share/fonts/dejavu/DejaVuSans-Bold.ttf",
    ]
    regular_paths = [
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
        "/usr/share/fonts/dejavu/DejaVuSans.ttf",
    ]
    candidates = bold_paths if bold else regular_paths
    for path in candidates:
        if os.path.exists(path):
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def draw_license(
    filename: str,
    name: str,
    address: str,
    dob: str,
    license_number: str,
    scenario_label: str,
) -> None:
    badge_colors = {
        "PASS": (0, 160, 80),
        "REVIEW": (200, 130, 0),
        "FAIL": (190, 30, 30),
    }
    badge_color = badge_colors.get(scenario_label, (100, 100, 100))

    img = Image.new("RGB", (CARD_W, CARD_H), BG_COLOR)
    draw = ImageDraw.Draw(img)

    # ── Header bar ──────────────────────────────────────────────────────────
    draw.rectangle([(0, 0), (CARD_W, 80)], fill=HEADER_COLOR)
    draw.text((20, 12), "SPRINGFIELD STATE  •  DRIVER LICENSE",
              font=_load_font(28), fill="white")
    draw.text((20, 46), "SAMPLE DOCUMENT – NOT VALID FOR OFFICIAL USE",
              font=_load_font(16, bold=False), fill=(200, 220, 255))

    # ── Scenario badge ───────────────────────────────────────────────────────
    draw.rectangle([(CARD_W - 160, 10), (CARD_W - 10, 68)],
                   fill=badge_color, outline="white", width=2)
    badge_font = _load_font(26)
    bbox = draw.textbbox((0, 0), scenario_label, font=badge_font)
    bw = bbox[2] - bbox[0]
    draw.text((CARD_W - 160 + (150 - bw) // 2, 28), scenario_label,
              font=badge_font, fill="white")

    # ── Photo placeholder ────────────────────────────────────────────────────
    draw.rectangle([(20, 100), (200, 300)], fill=(200, 210, 220),
                   outline=(100, 120, 140), width=2)
    draw.text((55, 190), "PHOTO", font=_load_font(14, bold=False),
              fill=(100, 120, 140))

    # ── License number box ───────────────────────────────────────────────────
    draw.rectangle([(20, 310), (200, 360)], fill=(220, 230, 240),
                   outline=LINE_COLOR, width=1)
    draw.text((30, 315), "LICENSE #", font=_load_font(13, bold=False),
              fill=LABEL_COLOR)
    draw.text((30, 333), license_number, font=_load_font(16), fill=TEXT_COLOR)

    # ── Barcode placeholder ──────────────────────────────────────────────────
    draw.rectangle([(20, 370), (200, 420)], fill=(230, 230, 230),
                   outline=(160, 160, 160), width=1)
    rng = random.Random(len(name))
    for x in range(25, 195, 5):
        h = rng.randint(8, 45)
        draw.rectangle([(x, 375), (x + 2, 375 + h)], fill=(30, 30, 30))
    draw.text((55, 425), "PDF417", font=_load_font(11, bold=False),
              fill=LABEL_COLOR)

    # ── Vertical divider ─────────────────────────────────────────────────────
    draw.line([(220, 100), (220, 500)], fill=LINE_COLOR, width=2)

    # ── Data fields ──────────────────────────────────────────────────────────
    lf = _load_font(16, bold=False)
    vf = _load_font(20)
    y = 110

    fields = [
        ("FULL NAME", name),
        ("DATE OF BIRTH", dob),
        ("ADDRESS", address),
        ("CLASS", "DL-C"),
        ("EXPIRES", "06/15/2028"),
        ("ISS", "06/15/2023"),
    ]

    for label, value in fields:
        draw.text((235, y), label, font=lf, fill=LABEL_COLOR)
        y += 22
        words = value.split()
        line = ""
        for word in words:
            test = (line + " " + word).strip()
            bbox = draw.textbbox((0, 0), test, font=vf)
            if bbox[2] - bbox[0] > 590:
                draw.text((235, y), line, font=vf, fill=TEXT_COLOR)
                y += 28
                line = word
            else:
                line = test
        if line:
            draw.text((235, y), line, font=vf, fill=TEXT_COLOR)
            y += 36
        else:
            y += 8

    # ── Footer bar ───────────────────────────────────────────────────────────
    draw.rectangle([(0, CARD_H - 40), (CARD_W, CARD_H)], fill=HEADER_COLOR)
    draw.text(
        (20, CARD_H - 28),
        f"SCENARIO: {scenario_label}  •  For testing with MockDocumentExtractionService only",
        font=_load_font(14, bold=False),
        fill=(200, 220, 255),
    )

    # ── Outer border ─────────────────────────────────────────────────────────
    draw.rectangle([(0, 0), (CARD_W - 1, CARD_H - 1)], outline=HEADER_COLOR, width=3)

    out_path = OUTPUT_DIR / filename
    img.save(out_path, "PNG")
    print(f"Created: {out_path}")


DOCUMENTS = [
    dict(
        filename="pass-license.png",
        name="John Michael Smith",
        address="123 Main Street, Springfield, IL 62701",
        dob="1985-06-15",
        license_number="S123-4567-8901",
        scenario_label="PASS",
    ),
    dict(
        filename="review-license.png",
        name="John Michael Smith",
        address="123 Main Street Apt 2B, Springfield, IL 62701",
        dob="1985-06-15",
        license_number="S123-4567-8901",
        scenario_label="REVIEW",
    ),
    dict(
        filename="fail-license.png",
        name="Jane Doe",
        address="999 Oak Avenue, Chicago, IL 60601",
        dob="1990-01-01",
        license_number="D999-0000-1111",
        scenario_label="FAIL",
    ),
]

if __name__ == "__main__":
    for doc in DOCUMENTS:
        draw_license(**doc)
    print("Done.")
