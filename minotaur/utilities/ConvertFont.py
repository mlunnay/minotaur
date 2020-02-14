import clr

clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework")
clr.AddReferenceByPartialName("WindowsBase")
clr.AddReferenceByPartialName("IronPython")

clr.AddReferenceToFileAndPath("..\\External Resources\\OpenTK.dll")

from System import *
from sys import *
from System.IO import *
from System.Drawing import *
from System.Drawing.Imaging import BitmapData, ImageLockMode
import System.Drawing.Imaging.PixelFormat as PF
from OpenTK import *
from OpenTK.Graphics import GraphicsContext, GraphicsMode
from OpenTK.Graphics.OpenGL import *
from OpenTK.Graphics.OpenGL import GL
from System.Text import Encoding
from struct import *
import argparse
import os.path
from ContentWriter import ContentWriter

class BitmapFont:
    def __init__(self):
        self.characterSet = CharacterSet()

class CharacterSet:
    def __init__(self):
        self.lineHeight = 0
        self.base = 0
        self.renderedSize = 0
        self.paddingUp = 0
        self.paddingRight = 0
        self.paddingDown = 0
        self.paddingLeft = 0
        self.characters = []
    
    def GetCharacterByID(self, id):
        character = [x for x in self.characters if x.id == id]
        if len(character) == 1:
            return character[0]
        else:
            return None
        
class Character:
    def __init__(self):
        self.id = -1
        self.x = 0
        self.y = 0
        self.width = 0
        self.height = 0
        self.xOffset = 0
        self.yOffset = 0
        self.xAdvance = 0
        self.kerning = {}

class Serializer:
    def __init__(self, id):
        self.id = Guid(id)
        
window = NativeWindow()
context = GraphicsContext(GraphicsMode.Default, window.WindowInfo)
context.MakeCurrent(window.WindowInfo)
context.LoadAll()

parser = argparse.ArgumentParser(description='Convert an font file to MEB font format.')
parser.add_argument('input', help='the font file to convert')
parser.add_argument('-o', metavar='OUTPUT', help='file to output MEB font to.')

args = parser.parse_args()

if args.o:
    outfile = args.o
else:
    outfile = os.path.splitext(os.path.split(args.input)[1])[0] + '.meb'

# load and read the fnt file
f = open(args.input, 'r')
inputPath = os.path.split(args.input)[0]
fontImagePath = ""
font = BitmapFont()
for line in f.readlines():
    tokens = line.strip().split()
    if tokens[0] == "info":
        for token in tokens[1:]:
            data = token.split("=", 1)
            if data[0] == "size":
                font.characterSet.renderedSize = int(data[1])
            elif data[0] == "padding":
                up, right, down, left = [int(x) for x in data[1].split(',')]
                font.characterSet.paddingUp = up
                font.characterSet.paddingRight = right
                font.characterSet.paddingDown = down
                font.characterSet.paddingLeft = left
    elif tokens[0] == "common":
        for token in tokens[1:]:
            data = token.split("=", 1)
            if data[0] == "lineHeight":
                font.characterSet.lineHeight = int(data[1])
            elif data[0] == "base":
                font.characterSet.base = int(data[1])
    elif tokens[0] == "page":
        if tokens[1] != "id=0":
            continue
        fontImagePath = os.path.join(inputPath, tokens[2].split('=', 1)[1][1:-1])
    elif tokens[0] == "char":
        char = Character()
        for token in tokens[1:]:
            data = token.split("=", 1)
            if data[0] == "id":
                char.id = int(data[1])
            elif data[0] == "x":
                char.x = int(data[1])
            elif data[0] == "y":
                char.y = int(data[1])
            elif data[0] == "width":
                char.width = int(data[1])
            elif data[0] == "height":
                char.height = int(data[1])
            elif data[0] == "xoffset":
                char.xOffset = int(data[1])
            elif data[0] == "yoffset":
                char.yOffset = int(data[1])
            elif data[0] == "xadvance":
                char.xAdvance = int(data[1])
        font.characterSet.characters.append(char)
    elif tokens[0] == "kerning":
        for token in tokens[1:]:
            data = token.split("=", 1)
            if data[0] == "first":
                index = int(data[1])
            elif data[0] == "second":
                second = int(data[1])
            elif data[0] == "amount":
                amount = int(data[1])
        char = font.characterSet.GetCharacterByID(index)
        if char:
            char.kerning[second] = ammount

# build the MEB font file
f = File.Create(outfile)
writer = ContentWriter(f)      

# get and write the image data
bmp = Bitmap(fontImagePath)
bmp_data = bmp.LockBits(Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PF.Format32bppArgb)
texID = GL.GenTexture()
GL.BindTexture(TextureTarget.Texture2D, texID)
GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
bmp.UnlockBits(bmp_data);
data = Array.CreateInstance(Byte, bmp.Width * bmp.Height * 4)
GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data)
# we just need grayscale image for the font
img = Array.CreateInstance(Byte, bmp.Width * bmp.Height)
for i in xrange(bmp.Width * bmp.Height):
    img[i] = data[i * 4]
    
writer.typeList.append(Serializer('1f6057f0-d13f-42ae-9e6b-4011fad823fd'))
writer.typeList.append(Serializer('e5e29004-6f77-4be4-a9c6-c3eb363ca021'))
writer.WriteInt(1)  # first type
writer.WriteInt(2)  # second type
writer.WriteUInt32(4)   # R8 format
writer.WriteInt(bmp.Width)
writer.WriteInt(bmp.Height)
writer.WriteInt(1)  # single image no mipmaps
writer.WriteUInt32(bmp.Width * bmp.Height)  # data size
writer.WriteByteArray(img)
writer.WriteInt(font.characterSet.lineHeight)
writer.WriteInt(font.characterSet.base)
writer.WriteInt(font.characterSet.renderedSize)
writer.WriteInt(font.characterSet.paddingUp)
writer.WriteInt(font.characterSet.paddingRight)
writer.WriteInt(font.characterSet.paddingDown)
writer.WriteInt(font.characterSet.paddingLeft)
writer.WriteInt(len(font.characterSet.characters))
for c in font.characterSet.characters:
    writer.WriteInt(c.id)
    writer.WriteInt(c.x)
    writer.WriteInt(c.y)
    writer.WriteInt(c.width)
    writer.WriteInt(c.height)
    writer.WriteInt(c.xOffset)
    writer.WriteInt(c.yOffset)
    writer.WriteInt(c.xAdvance)
    writer.WriteInt(len(c.kerning))
    for k, v in c.kerning:
        writer.WriteInt(k)
        writer.WriteInt(v)

writer.Flush()
f.Close()
        