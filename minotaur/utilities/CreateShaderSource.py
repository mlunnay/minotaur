import clr

clr.AddReferenceToFileAndPath("..\\External Resources\\OpenTK.dll")

from System import *
from System.IO import *
import argparse
import sys
from OpenTK import *
from OpenTK.Graphics import GraphicsContext, GraphicsMode
from OpenTK.Graphics.OpenGL import *
from OpenTK.Graphics.OpenGL import GL
import os.path
from ContentWriter import ContentWriter
import re

glslRegex = re.compile("ERROR: (\d+):(\d+): (.*)")

class Serializer:
    def __init__(self, id):
        self.id = Guid(id)

class GLSLError(Exception):
    def __init__(self, msg):
        self.msg = msg
    
    def __str__(self):
        return self.msg

def ParseGLSLError(error):
    match = glslRegex.match(error)
    if match:
        return match.group(1), match.group(2), match.group(3)
    
    return None
        
parser = argparse.ArgumentParser(description='Convert an font file to MEB font format.')
parser.add_argument('-v', '--vertex', metavar='VERTEX', type=argparse.FileType('r'), help='The vertex shader source file for the shader')
parser.add_argument('-f', '--fragment', metavar='FRAGMENT', type=argparse.FileType('r'), help='The frament shader source file for the shader')
parser.add_argument('-g', '--geometry', metavar='GEOMETRY', type=argparse.FileType('r'), help='The geometry shader source file for the shader')
parser.add_argument('-o', '--output', metavar='OUTPUT', required=True, help='The file to output the shader to.')

args = parser.parse_args()

window = NativeWindow()
context = GraphicsContext(GraphicsMode.Default, window.WindowInfo)
context.MakeCurrent(window.WindowInfo)
context.LoadAll()

vertexSource = ""
fragmentSource = ""
geometrySource = ""

fileMap = []

fileNumber = 0

if args.vertex:
    vertexSource = "#line 0 %d\n%s" % (fileNumber, args.vertex.read())
    fileMap.append([fileNumber, os.path.abspath(args.vertex.name)])
    fileNumber += 1
if args.fragment:
    fragmentSource = "#line 0 %d\n%s" % (fileNumber, args.fragment.read())
    fileMap.append([fileNumber, os.path.abspath(args.fragment.name)])
    fileNumber += 1
if args.geometry:
    geometrySource = "#line 0 %d\n%s" % (fileNumber, args.geometry.read())
    fileMap.append([fileNumber, os.path.abspath(args.geometry.name)])
    fileNumber += 1

# compile and check shaders for errors
sources = [x for x in [[vertexSource, 0], [fragmentSource, 1], [geometrySource, 2]] if x[0] != ""]
programID = GL.CreateProgram()
fileNumber = 0
errors = []
shaderIDs = []
for source, typenum in sources:
    if typenum == 0:
        type = ShaderType.VertexShader
    elif typenum == 1:
        type = ShaderType.FragmentShader
    elif typenum == 2:
        type = ShaderType.GeometryShader
    else:
        raise VakueError("Unknown shader type: %d", typenum)
        
    id = GL.CreateShader(type)
    GL.ShaderSource(id, source)
    GL.CompileShader(id)
    status = GL.GetShader(id, ShaderParameter.CompileStatus)
    if status == 0:
        log = GL.GetShaderInfoLog(id)
        for line in log.strip().split("\n"):
            filenum, linenum, msg = ParseGLSLError(line)
            errors.append("%s line %s: %s" % (fileMap[int(filenum)][1], linenum, msg))
        # GL.DeleteShader(id)
    GL.AttachShader(programID, id)
    shaderIDs.append(id)
        
if errors:
    print "GLSL shader compile errors:"
    print "\n".join(errors)
    sys.exit(1)

# link and check program for errors
GL.LinkProgram(programID)
status = GL.GetProgram(programID, ProgramParameter.LinkStatus)
if status == 0:
    log = GL.GetProgramInfoLog(programID)
    for line in [x for x in log.strip().split("\n") if x.strip()]:
        msg = line.split(" ", 1)[1]
        errors.append(msg)

GL.DeleteProgram(programID)
for s in shaderIDs:
    GL.DeleteShader(id)

if errors:
    print "GLSL program linking errors:"
    print "\n".join(errors)
    sys.exit(1)
    
f = File.Create(args.output)
writer = ContentWriter(f)
writer.typeList.append(Serializer('205801eb-a58e-4627-a2df-7c9dafdd33d6'))
writer.WriteInt(1)
writer.WriteInt(len(fileMap))
for num, name in fileMap:
    writer.WriteInt(num)
    writer.WriteString(name)

writer.WriteInt(len(sources))
for source, type in sources:
    writer.WriteInt(type)
    writer.WriteString(source)

writer.Flush()
