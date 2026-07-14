from PIL import Image, ImageDraw, ImageFont

# Create a transparent image: width=600, height=200
img = Image.new("RGBA", (600, 200), (0,0,0,0))
draw = ImageDraw.Draw(img)

text = "Kur'an Meali"
font_size = 80
try:
    font = ImageFont.truetype("arialbd.ttf", font_size)
except:
    font = ImageFont.load_default()

text_bbox = draw.textbbox((0, 0), text, font=font)
text_width = text_bbox[2] - text_bbox[0]
text_height = text_bbox[3] - text_bbox[1]

text_x = (600 - text_width) / 2
text_y = (200 - text_height) / 2

draw.text((text_x, text_y), text, font=font, fill="#2c3e50")

save_path = r"d:\Git\KM\KuranMealApp\Platforms\Android\Resources\drawable\splash_branding.png"
import os
os.makedirs(os.path.dirname(save_path), exist_ok=True)
img.save(save_path)
print("Saved branding image to", save_path)
