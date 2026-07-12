from PIL import Image, ImageDraw, ImageFont

img_path = r"d:\Git\KM\KuranMealApp\Resources\AppIcon\appicon.png"
save_path = r"d:\Git\KM\KuranMealApp\Resources\Splash\splash_text.png"

img = Image.open(img_path).convert("RGBA")
width, height = img.size

new_height = int(height * 1.3)
new_img = Image.new("RGBA", (width, new_height), (249, 246, 238, 0)) # transparent

# Paste original image centered horizontally, at top vertically
new_img.paste(img, (0, 0))

draw = ImageDraw.Draw(new_img)

font_size = int(width * 0.12) # 12% of width
try:
    font = ImageFont.truetype("arialbd.ttf", font_size)
except:
    font = ImageFont.load_default()

text = "Kur'an Meali"
text_bbox = draw.textbbox((0, 0), text, font=font)
text_width = text_bbox[2] - text_bbox[0]
text_height = text_bbox[3] - text_bbox[1]

text_x = (width - text_width) / 2
# Place text below the original icon image
text_y = height + (new_height - height - text_height) / 2 - (text_height/2)

# Use a nice dark blue/slate color
draw.text((text_x, text_y), text, font=font, fill="#2c3e50")

new_img.save(save_path)
print(f"Saved splash screen with text to {save_path}")
