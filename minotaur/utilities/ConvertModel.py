import clr

clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework")
clr.AddReferenceByPartialName("WindowsBase")
clr.AddReferenceByPartialName("IronPython")
clr.AddReferenceByPartialName("System.Core")
clr.AddReferenceToFileAndPath("..\\External Resources\\OpenTK.dll")
clr.AddReferenceToFileAndPath("..\\External Resources\\AssimpNet.dll")

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
import os
import os.path
from ContentWriter import ContentWriter
import Assimp
from System.Linq import Enumerable
from System.Collections.Generic import List
    
class CustomWriter(ContentWriter):
    def __init__(self, outputStream, compressOutput = False, identifierString = "MEB"):
        super(CustomWriter, self).__init__(outputStream, compressOutput, identifierString)

    def WriteMatrix(self, value):
        self.outStream.Write(value.M11)
        self.outStream.Write(value.M12)
        self.outStream.Write(value.M13)
        self.outStream.Write(value.M14)
        self.outStream.Write(value.M21)
        self.outStream.Write(value.M22)
        self.outStream.Write(value.M23)
        self.outStream.Write(value.M24)
        self.outStream.Write(value.M31)
        self.outStream.Write(value.M32)
        self.outStream.Write(value.M33)
        self.outStream.Write(value.M34)
        self.outStream.Write(value.M41)
        self.outStream.Write(value.M42)
        self.outStream.Write(value.M43)
        self.outStream.Write(value.M44)
    
    def WriteVector3(self, value):
        self.outStream.Write(value.X)
        self.outStream.Write(value.Y)
        self.outStream.Write(value.Z)
    
    def WriteBoundingSphere(self, value):
        self.WriteVector3(value.Center)
        self.WriteSingle(value.Radius)

class Bone(object):
    def __init__(self, index, name):
        self.index = index
        self.name = name
        self.parent = None
        self.children = []
        self.transform = Matrix4.Identity
        self.absoluteInverseTransform = Matrix4.Identity

class PositionNormalTextureBoneIDWeights(object):
    def __init__(self):
        self.position = Vector3(0,0,0)
        self.texCoord = Vector2(0,0)
        self.normal = Vector3(0,0,0)
        self.boneIDs = [0,0,0,0]
        self.boneWeights = Vector4(0,0,0,0)
    
    def toBinaryArray(self):
        res = List[Byte]()
        res.AddRange(BitConverter.GetBytes(self.position.X))
        res.AddRange(BitConverter.GetBytes(self.position.Y))
        res.AddRange(BitConverter.GetBytes(self.normal.X))
        res.AddRange(BitConverter.GetBytes(self.normal.Y))
        res.AddRange(BitConverter.GetBytes(self.normal.Z))
        res.AddRange(BitConverter.GetBytes(self.position.Z))
        res.AddRange(BitConverter.GetBytes(self.texCoord.X))
        res.AddRange(BitConverter.GetBytes(self.texCoord.Y))
        res.AddRange(BitConverter.GetBytes(clr.Convert(self.boneIDs[0], Int32)))
        res.AddRange(BitConverter.GetBytes(clr.Convert(self.boneIDs[1], Int32)))
        res.AddRange(BitConverter.GetBytes(clr.Convert(self.boneIDs[2], Int32)))
        res.AddRange(BitConverter.GetBytes(clr.Convert(self.boneIDs[3], Int32)))
        res.AddRange(BitConverter.GetBytes(self.boneWeights.X))
        res.AddRange(BitConverter.GetBytes(self.boneWeights.Y))
        res.AddRange(BitConverter.GetBytes(self.boneWeights.Z))
        res.AddRange(BitConverter.GetBytes(self.boneWeights.W))
        return Enumerable.ToArray(res)

class VertexElement(object):
    def __init__(self, dimension, t, usage, getData, isFloat=True):
        self.dimension = dimension
        self.type = t
        self.usage = usage
        self.isFloat = isFloat
        self.getData = getData
    
    def toBinaryArray(self, mesh, i):
        res = List[Byte]()
        for x in self.getData(mesh, i):
            if self.isFloat:
                res.AddRange(BitConverter.GetBytes(clr.Convert(x, Single)))
            else:
                res.AddRange(BitConverter.GetBytes(clr.Convert(x, Int32)))
        return res
    
    @property
    def stride(self):
        size = 1
        if self.type in [2, 3, 8]:
            size = 2
        elif self.type in [4, 5, 6]:
            size = 4
        elif self.type == 7:
            size = 8
        
        return size * self.dimension
    
    @staticmethod
    def Position():
        return VertexElement(3, 6, 0, lambda mesh, i: (mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z))
    
    # @staticmethod
    # def Color():
        # return VertexElement(4, 6, 1)
        
    @staticmethod
    def Texture():
        return VertexElement(2, 6, 2, lambda mesh, i: (mesh.GetTextureCoords(0)[i].X, 1 - mesh.GetTextureCoords(0)[i].Y))
        
    @staticmethod
    def Normal():
        return VertexElement(3, 6, 3, lambda mesh, i:(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z))
        
    @staticmethod
    def BlendIndices():
        return VertexElement(4, 4, 6, lambda mesh, i: (0,0,0,0), False)
    
    @staticmethod
    def BlendWeight():
        return VertexElement(4, 6, 7, lambda mesh, i: (0,0,0,0))

class VertexDeclaration(object):
    def __init__(self, elements = None):
        if elements == None:
            self.elements = []
        else:
            self.elements = elements
    
    def add(self, element):
        self.elements.append(element)
    
    def toBinaryArray(self, mesh, i):
        res = List[Byte]()
        for e in self.elements:
                res.AddRange(e.toBinaryArray(mesh, i))
        return Enumerable.ToArray(res)
    
    def __getattr__(self, key):
        if isinstance(key, str):
            key = key.lower()
            if key == "position":
                index = 0
            elif key == "color":
                index = 1
            elif key == "texture":
                index = 2
            elif key == "normal":
                index = 4
            elif key == "blendindices":
                index = 6
            elif key == "blendweights":
                index = 7
            else:
                raise ValueError("unknown attribute type: %s" % key)
        else:
            index = key
        e = [x for x in self.elements if x.usage == index]
        return e[0] if len(e) == 1 else None
    
    @property
    def elementCount(self):
        return len(self.elements)
    
    @property
    def stride(self):
        s = 0
        for e in self.elements:
            s += e.stride
        return s
        
class MeshPart(object):
    def __init__(self):
        self.baseVertex = 0
        self.baseIndex = 0
        self.numIndices = 0
        self.materialIndex = 0
        self.numVertices = 0
        self.primitiveCount = 0

class BoundingSphere(object):
    def __init__(self):
        self.Center = Vector3(0,0,0)
        self.Radius = 0
    
    def CreateFromPoints(self, points):
        numPoints = len(points)
        center = Vector3(0,0,0)
        for p in points:
            center += p / numPoints
        radius = 0
        for p in points:
            distance = (p - center).Length
            if distance > radius:
                radius = distance
        
        self.Center = center
        self.Radius = radius

class Serializer:
    def __init__(self, id):
        self.id = Guid(id)
        
vertices = List[Byte]()
indices = []
parts = []
points = []
def InitMesh(scene, vertexDeclaration):
    numVertices = 0
    numIndices = 0
    
    for mesh in scene.Meshes:
        part = MeshPart()
        part.materialIndex = mesh.MaterialIndex
        part.numIndices = mesh.FaceCount * 3
        part.baseVertex = numVertices
        part.baseIndex = numIndices
        part.numVertices = mesh.VertexCount
        part.primitiveCount = mesh.FaceCount
        
        numVertices += mesh.VertexCount
        numIndices += mesh.FaceCount * 3
        
        parts.append(part)
        for i in xrange(mesh.VertexCount):
            # vertex = PositionNormalTextureBoneIDWeights()
            # vertex.position = Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z)
            # vertex.texCoord = Vector2(mesh.GetTextureCoords(0)[i].X, 1 - mesh.GetTextureCoords(0)[i].Y)
            # vertex.normal = Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z)
            vertices.AddRange(vertexDeclaration.toBinaryArray(mesh, i))
            points.append(Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z))
        
        for face in mesh.Faces:
            assert(face.IndexCount == 3)
            for i in face.Indices:
                indices.append(i + part.baseIndex)

def toMatrix4(m):
    return Matrix4(m.A1, m.A2, m.A3, m.A4, m.B1, m.B2, m.B3, m.B4, m.C1, m.C2, m.C3, m.C4, m.D1, m.D2, m.D3, m.D4)
                
bones = []
def CreateSkeleton(bones, node, index = 0):
    bone = Bone(index, node.Name)
    bone.transform = toMatrix4(node.Transform)
    index += 1
    
    bones.append(bone)
    if node.HasChildren:
        for i in xrange(node.ChildCount):
            child = CreateSkeleton(bones, node.Children[i], index)
            child.parent = bone
            bone.children.append(child)
            
    return bone
    
def processBones(bones):
    root = bones[0]
    root.absoluteInverseTransform = Matrix4.Invert(root.transform)
    for child in root.children:
        processBonesRecursive(child, root.absoluteInverseTransform)

def processBonesRecursive(bone, inverseTransform):
    bone.absoluteInverseTransform = inverseTransform * bone.transform
    for child in bone.children:
        processBonesRecursive(child, bone.absoluteInverseTransform)
    
os.environ["Path"] = os.environ["Path"] + ";..\\External Resources\\;C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319"
        
parser = argparse.ArgumentParser(description='Convert a model file to MEB model format.')
parser.add_argument('input', help='the image file to convert')
parser.add_argument('-n', '--nonormals', action='store_false', default=True, dest='normals', help='do not include normal information in exported model.')
parser.add_argument('-t', '--notexture', action='store_false', default=True, dest='texture', help='do not include texture mapping information in exported model.')
parser.add_argument('-s', '--skin', action='store_true', default=False, dest='skin', help='include bone skinning information in exported model.')
parser.add_argument('-o', metavar='OUTPUT', help='file to output MEB image to.')

args = parser.parse_args()

if args.o:
    outfile = args.o
else:
    outfile = os.path.splitext(os.path.split(args.input)[1])[0] + '.meb'

vertexDeclaration = VertexDeclaration()
vertexDeclaration.add(VertexElement.Position())
if args.normals:
    vertexDeclaration.add(VertexElement.Normal())
if args.texture:
    vertexDeclaration.add(VertexElement.Texture())
if args.skin:
    vertexDeclaration.add(VertexElement.BlendIndices())
    vertexDeclaration.add(VertexElement.BlendWeight())
    
importer = Assimp.AssimpImporter()
scene = importer.ImportFile(args.input, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality)

InitMesh(scene, vertexDeclaration)
CreateSkeleton(bones, scene.RootNode)
processBones(bones)

if args.skin:
    # TODO: skinning weighting and indexes
    pass

boundingSphere = BoundingSphere()
boundingSphere.CreateFromPoints(points)

f = File.Create(outfile)
writer = CustomWriter(f)
writer.typeList.append(Serializer('09b12e6d-acf3-4cf5-a150-923056d88d9b'))  # model
writer.typeList.append(Serializer('1da5cdcd-2f1e-41d5-b7ab-fde163a5336e'))  # vertex buffer
writer.typeList.append(Serializer('f6eded0f-1342-4249-b231-c166a860b224'))  # index buffer
writer.typeList.append(Serializer('6f1be25e-7f37-4faa-b551-7a58c8d91824'))  # effect material
writer.WriteInt(1)  # first type
# skeleton information
writer.WriteUInt32(len(bones))
for bone in bones:
    writer.WriteString(bone.name)
    writer.WriteMatrix(bone.transform)
    writer.WriteMatrix(bone.absoluteInverseTransform)
for bone in bones:
    writer.WriteUInt32(bone.parent.index + 1 if bone.parent else 0)
    writer.WriteUInt32(len(bone.children))
    for child in bone.children:
        writer.WriteUInt32(child.index + 1)

# mesh information
writer.WriteUInt32(1)   # this script only converts single models currently
writer.WriteString(os.path.splitext(os.path.split(args.input)[1])[0])
writer.WriteUInt32(bones[0].index + 1 if bones else 0)  # parent bone is the root node
writer.WriteBoundingSphere(boundingSphere)
writer.WriteInt(0)    # no tag object
writer.WriteUInt32(len(parts))
for part in parts:
    writer.WriteUInt32(part.baseVertex)
    writer.WriteUInt32(part.numVertices)
    writer.WriteUInt32(part.numIndices)
    writer.WriteUInt32(part.baseIndex)
    writer.WriteUInt32(part.primitiveCount)
    writer.WriteInt(0)  # no tag object
    writer.WriteInt(1)  # shared resource 0
    writer.WriteInt(2)  # shared resource 1
    writer.WriteInt(0)

writer.WriteUInt32(bones[0].index + 1 if bones else 0)
writer.WriteInt(0)  # no tag object

# animation reference
# TODO: handle animation 
writer.WriteUInt32(0)

# shared resources
writer.sharedResources.append(1)
writer.sharedResources.append(2)

writer.WriteInt(2)
# # vertex declaration
# writer.WriteUInt32(64)
# writer.WriteUInt32(5)
# # position
# writer.WriteSByte(3)
# writer.WriteSByte(6)
# writer.WriteUInt32(0)
# # normal
# writer.WriteSByte(3)
# writer.WriteSByte(6)
# writer.WriteUInt32(3)
# # texture coordinates
# writer.WriteSByte(2)
# writer.WriteSByte(6)
# writer.WriteUInt32(2)
# # bone indices
# writer.WriteSByte(4)
# writer.WriteSByte(4)
# writer.WriteUInt32(6)
# # bone weights
# writer.WriteSByte(4)
# writer.WriteSByte(6)
# writer.WriteUInt32(7)

# vertex declaration
writer.WriteUInt32(vertexDeclaration.stride)
writer.WriteUInt32(vertexDeclaration.elementCount)
for e in vertexDeclaration.elements:
    writer.WriteSByte(e.dimension)
    writer.WriteSByte(e.type)
    writer.WriteUInt32(e.usage)

writer.WriteUInt32(vertices.Count / vertexDeclaration.stride)
# data = List[Byte]()
# for v in vertices:
    # data.AddRange(vertexDeclaration.toBinaryArray(v))
# assert(data.Count == len(vertices) * vertexDeclaration.stride)
writer.WriteByteArray(Enumerable.ToArray(vertices))

# index buffer
writer.WriteInt(3)
data = List[Byte]()
for i in indices:
    data.AddRange(BitConverter.GetBytes(i))
writer.WriteUInt32(data.Count)
writer.WriteByteArray(Enumerable.ToArray(data))

writer.Flush()
f.Close()
  