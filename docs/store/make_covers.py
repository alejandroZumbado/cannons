"""Generate CrazyGames cover images (1920x1080, 800x1200, 800x800) from the game's own art.

Why: CrazyGames Basic Launch requires 3 covers; we build them locally with PIL
(no paid AI) from Assets/UI art + the in-game Pirates font, so they can be
regenerated any time. Run from the repo root: python docs/store/make_covers.py
Output: docs/store/graphics/crazygames/
"""
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parents[2]
MENU_ART = ROOT / "Assets/UI/BGMenu.png"     # wide coastal scene (has faint ghost text on the left, covered by the title)
ICON_ART = ROOT / "Assets/UI/iconApp.png"    # square cannon + tower at sunset (rounded frame, cropped off below)
FONT = ROOT / "Assets/Fonts/Pirates.otf"     # same font as the in-game UI
OUT = ROOT / "docs/store/graphics/crazygames"

TITLE = "CANNONS"
SUBTITLE = "PIRATE DEFENSE"
GOLD = (255, 214, 64)
WHITE = (255, 255, 255)
OUTLINE = (30, 16, 6)

# inner square of the icon, without the rounded frame and blurred border
ICON_CROP = (110, 110, 914, 914)


def load(path: Path) -> Image.Image:
    # fail loudly with the missing path instead of a bare PIL traceback
    if not path.exists():
        sys.exit(f"Missing source file: {path}")
    return Image.open(path).convert("RGB")


def cover_fit(img: Image.Image, size: tuple, anchor_x: float = 0.5) -> Image.Image:
    # scale to fill `size` keeping aspect, then crop; anchor_x picks which horizontal part survives
    w, h = size
    scale = max(w / img.width, h / img.height)
    resized = img.resize((round(img.width * scale), round(img.height * scale)), Image.LANCZOS)
    left = round((resized.width - w) * anchor_x)
    top = (resized.height - h) // 2
    return resized.crop((left, top, left + w, top + h))


def shade(img: Image.Image, box: tuple, alpha: int) -> None:
    # darken a soft-edged band so the title reads on any background
    overlay = Image.new("L", img.size, 0)
    ImageDraw.Draw(overlay).rectangle(box, fill=alpha)
    overlay = overlay.filter(ImageFilter.GaussianBlur(60))
    img.paste(Image.new("RGB", img.size, (0, 0, 0)), (0, 0), overlay)


def draw_text(img: Image.Image, text: str, center: tuple, size: int, color: tuple, stroke: int) -> None:
    # centered text with a dark outline (same look as the in-game labels)
    font = ImageFont.truetype(str(FONT), size)
    ImageDraw.Draw(img).text(center, text, font=font, fill=color, anchor="mm",
                             stroke_width=stroke, stroke_fill=OUTLINE)


def title_block(img: Image.Image, cx: int, cy: int, title_size: int) -> None:
    # "CANNONS" big in gold + "PIRATE DEFENSE" under it in white
    draw_text(img, TITLE, (cx, cy), title_size, GOLD, max(4, title_size // 14))
    draw_text(img, SUBTITLE, (cx, cy + round(title_size * 0.78)), round(title_size * 0.38), WHITE,
              max(3, title_size // 30))


def landscape(menu: Image.Image) -> Image.Image:
    # 1920x1080: coastal scene, title over the top-left (hides the art's ghost text)
    img = cover_fit(menu, (1920, 1080), anchor_x=0.0)
    # subtitle sits right on the ghost text (y ~350-415 after scaling); stronger shade hides the rest
    shade(img, (0, 80, 1050, 520), 200)
    title_block(img, 520, 215, 230)
    return img


def portrait(icon: Image.Image) -> Image.Image:
    # 800x1200: blurred art fills the canvas, sharp square art in the middle, title on top
    art = icon.crop(ICON_CROP)
    img = cover_fit(art, (800, 1200)).filter(ImageFilter.GaussianBlur(25))
    img.paste(art.resize((760, 760), Image.LANCZOS), (20, 330))
    shade(img, (0, 0, 800, 320), 170)
    title_block(img, 400, 140, 150)
    return img


def square(icon: Image.Image) -> Image.Image:
    # 800x800: square art with the title across the bottom
    img = icon.crop(ICON_CROP).resize((800, 800), Image.LANCZOS)
    shade(img, (0, 560, 800, 800), 170)
    title_block(img, 400, 650, 120)
    return img


def main() -> None:
    menu, icon = load(MENU_ART), load(ICON_ART)
    if not FONT.exists():
        sys.exit(f"Missing font: {FONT}")
    OUT.mkdir(parents=True, exist_ok=True)
    for name, image in (("cover_1920x1080.png", landscape(menu)),
                        ("cover_800x1200.png", portrait(icon)),
                        ("cover_800x800.png", square(icon))):
        image.save(OUT / name, optimize=True)
        print(f"wrote {OUT / name} {image.size}")


if __name__ == "__main__":
    main()
