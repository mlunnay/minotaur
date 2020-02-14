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
from System.Runtime.InteropServices import Marshal
from OpenTK import *
from OpenTK.Graphics import GraphicsContext, GraphicsMode
from OpenTK.Graphics.OpenGL import *
from OpenTK.Graphics.OpenGL import GL
from System.Text import Encoding
from struct import *
import argparse
import os.path
from ContentWriter import ContentWriter

class Serializer:
    def __init__(self, id):
        self.id = Guid(id)

window = NativeWindow()
context = GraphicsContext(GraphicsMode.Default, window.WindowInfo)
context.MakeCurrent(window.WindowInfo)
context.LoadAll()

parser = argparse.ArgumentParser(description='Convert an image file to MEB image format.')
parser.add_argument('input', help='the image file to convert')
parser.add_argument('-o', metavar='OUTPUT', help='file to output MEB image to.')

args = parser.parse_args()

if args.o:
    outfile = args.o
else:
    outfile = os.path.splitext(os.path.split(args.input)[1])[0] + '.meb'

bmp = Bitmap(args.input)
bmp_data = bmp.LockBits(Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PF.Format32bppArgb)
texID = GL.GenTexture()
GL.BindTexture(TextureTarget.Texture2D, texID)
GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
bmp.UnlockBits(bmp_data);
data = Array.CreateInstance(Byte, bmp.Width * bmp.Height * 4)
GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data)


f = File.Create(outfile)
writer = ContentWriter(f)
writer.typeList.append(Serializer('e5e29004-6f77-4be4-a9c6-c3eb363ca021'))
writer.WriteInt(1)  # first type
writer.WriteUInt32(1)   # RGBA format
writer.WriteInt(bmp.Width)
writer.WriteInt(bmp.Height)
writer.WriteInt(1)  # single image no mipmaps
writer.WriteUInt32(bmp.Width * bmp.Height * 4)  # data size
writer.WriteByteArray(data)
writer.Flush()
f.Close()




