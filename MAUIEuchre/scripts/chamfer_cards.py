"""
Chamfer card face images to match the cardback's rounded corners.

Extracts the alpha mask from cardback.png and applies it to every
cardface*.png, converting them from palette mode (P) to RGBA with
transparent corners matching the cardback.
"""

import os
import glob
from PIL import Image

IMAGES_DIR = os.path.join(os.path.dirname(__file__), "..", "Resources", "Images")

def get_alpha_mask(cardback_path):
    """Extract the alpha channel from cardback.png as a greyscale image."""
    img = Image.open(cardback_path).convert("RGBA")
    return img.split()[3]  # Alpha channel

def chamfer_card(card_path, alpha_mask):
    """Apply the cardback's alpha mask to a card face image."""
    card = Image.open(card_path).convert("RGBA")
    if card.size != alpha_mask.size:
        raise ValueError(f"{card_path} size {card.size} doesn't match mask size {alpha_mask.size}")
    
    # Apply the alpha mask: where cardback is transparent, card should be too
    r, g, b, a = card.split()
    # Use minimum of existing alpha and mask alpha (card faces are fully opaque,
    # so this effectively just applies the mask)
    new_alpha = Image.composite(a, Image.new("L", a.size, 0), alpha_mask)
    card.putalpha(new_alpha)
    card.save(card_path, "PNG")
    return card_path

def main():
    cardback_path = os.path.join(IMAGES_DIR, "cardback.png")
    if not os.path.exists(cardback_path):
        print(f"ERROR: cardback.png not found at {cardback_path}")
        return

    alpha_mask = get_alpha_mask(cardback_path)
    print(f"Alpha mask size: {alpha_mask.size}")

    pattern = os.path.join(IMAGES_DIR, "cardface*.png")
    card_files = sorted(glob.glob(pattern))
    print(f"Found {len(card_files)} card face images")

    for card_path in card_files:
        chamfer_card(card_path, alpha_mask)
        print(f"  Chamfered: {os.path.basename(card_path)}")

    print(f"\nDone. {len(card_files)} images updated.")

if __name__ == "__main__":
    main()
