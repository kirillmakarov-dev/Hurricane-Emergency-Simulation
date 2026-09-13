from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
WIDTH, HEIGHT = 1920, 1080


def font(name: str, size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(Path("C:/Windows/Fonts") / name), size)


background = Image.open(ROOT / "key-art-background.png").convert("RGB")
background = background.resize((WIDTH, HEIGHT), Image.Resampling.LANCZOS)
canvas = background.convert("RGBA")

# Reinforce the title zone while keeping the generated storm scene visible.
shade = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
shade_pixels = shade.load()
for x in range(1120):
    alpha = int(118 * (1.0 - x / 1120.0) ** 1.5)
    for y in range(HEIGHT):
        shade_pixels[x, y] = (4, 25, 34, alpha)
canvas.alpha_composite(shade)

draw = ImageDraw.Draw(canvas)
white = (248, 252, 251, 255)
aqua = (94, 208, 196, 255)
yellow = (242, 189, 69, 255)
soft = (218, 232, 230, 255)
chip_fill = (8, 68, 75, 210)
chip_outline = (94, 208, 196, 130)

draw.rounded_rectangle((96, 158, 176, 166), radius=4, fill=yellow)
draw.text((198, 134), "INTERACTIVE LEARNING GAME", font=font("segoeuib.ttf", 27), fill=aqua)

title_font = font("segoeuib.ttf", 88)
draw.text((96, 242), "HURRICANE", font=title_font, fill=white, stroke_width=1, stroke_fill=(0, 0, 0, 60))
draw.text((96, 338), "EMERGENCY", font=title_font, fill=white, stroke_width=1, stroke_fill=(0, 0, 0, 60))
draw.text((96, 434), "SIMULATION", font=title_font, fill=aqua, stroke_width=1, stroke_fill=(0, 0, 0, 60))

subtitle_font = font("segoeui.ttf", 32)
draw.text((100, 575), "Plan safely. Watch the outcome.", font=subtitle_font, fill=soft)
draw.text((100, 618), "Learn by doing.", font=subtitle_font, fill=soft)

chip_font = font("segoeuib.ttf", 22)
chips = ["UNITY", "C#", "10 LESSONS", "WEBGL"]
x = 100
for label in chips:
    box = draw.textbbox((0, 0), label, font=chip_font)
    chip_width = box[2] - box[0] + 38
    draw.rounded_rectangle((x, 704, x + chip_width, 752), radius=24, fill=chip_fill, outline=chip_outline, width=2)
    draw.text((x + 19, 713), label, font=chip_font, fill=white)
    x += chip_width + 14

draw.text((100, 924), "HURRICANE READY", font=font("segoeuib.ttf", 20), fill=aqua)
draw.text((100, 958), "EDUCATIONAL GAME • PORTFOLIO PROJECT", font=font("segoeui.ttf", 18), fill=(210, 228, 226, 230))

png_path = ROOT / "hurricane-emergency-portfolio-cover.png"
jpg_path = ROOT / "hurricane-emergency-portfolio-cover.jpg"
canvas.convert("RGB").save(png_path, quality=95)
canvas.convert("RGB").save(jpg_path, quality=94, optimize=True)
print(png_path)
print(jpg_path)
