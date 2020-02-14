import clr

from System import *
from System.IO import *

class ContentWriter(object):
    def __init__(self, outputStream, compressOutput = False, identifierString = "MEB"):
        self.identifierString = identifierString
        self.compressContent = compressOutput
        self.finalOutput = BinaryWriter(outputStream)
        self.headerContent = MemoryStream()
        self.contentData = MemoryStream()
        self.outStream = BinaryWriter(self.contentData)
        self.typeMap = {}
        self.sharedResourceMap = {}
        self.sharedResources = []
        self.typeList = []
        self.typeSerializerMap = {}
        self.version = 1
        
    def WriteBool(self, value):
        self.outStream.Write(clr.Convert(value, Boolean))
        
    def WriteInt16(self, value):
        self.outStream.Write(clr.Convert(value, Int16))
        
    def WriteInt(self, value):
        self.outStream.Write(clr.Convert(value, Int32))
        
    def WriteLong(self, value):
        self.outStream.Write(clr.Convert(value, Int64))
        
    def WriteUInt16(self, value):
        self.outStream.Write(clr.Convert(value, UInt16))
        
    def WriteUInt32(self, value):
        self.outStream.Write(clr.Convert(value, UInt32))
    
    def WriteUint64(self, value):
        self.outStream.Write(clr.Convert(value, UInt64))
    
    def WriteSingle(self, value):
        self.outStream.Write(clr.Convert(value, Single))
    
    def WriteDouble(self, value):
        self.outStream.Write(clr.Convert(value, Double))
    
    def WriteDecimal(self, value):
        self.outStream.Write(clr.Convert(value, Decimal))
    
    def WriteByte(self, value):
        self.outStream.Write(clr.Convert(value, Byte))
        
    def WriteSByte(self, value):
        self.outStream.Write(clr.Convert(value, SByte))
        
    def WriteByteArray(self, value):
        self.outStream.Write(value)
    
    def WriteByteArrayPartial(self, value, offset, count):
        self.outStream.Write(value, offset, count)
        
    def WriteChar(self, value):
        self.outStream.Write(clr.Convert(value, Char))
        
    def WriteString(self, value):
        self.WriteInt(len(value))
        self.outStream.Write(clr.Convert(value, String).ToCharArray(), 0, len(value))
    
    def WriteGuid(self, value):
        self.outStream.Write(value.ToByteArray())
    
    def WriteObject(self, value):
        # TODO: implement writing objects
        pass
    
    def Flush(self):
        self.WriteSharedResources()
        self.WriteHeader()
        self.WriteOutput()
    
    def WriteSharedResources(self):
        for i in xrange(len(self.sharedResources)):
            value = self.sharedResources[i]
            self.WriteObject(value)
    
    def WriteHeader(self):
        writer = BinaryWriter(self.headerContent)
        writer.Write(clr.Convert(len(self.typeList), Int32))
        for serializer in self.typeList:
            writer.Write(serializer.id.ToByteArray())
        writer.Write(clr.Convert(len(self.sharedResources), Int32))
        
    def WriteOutput(self):
        for c in self.identifierString:
            self.finalOutput.Write(clr.Convert(c, Char))
        self.finalOutput.Write(clr.Convert(self.version, Byte))
        flags = 0
        if self.compressContent:
            flags |= 0x1
        self.finalOutput.Write(clr.Convert(flags, Byte))
        
        filesize = self.finalOutput.BaseStream.Length + self.headerContent.Length + self.contentData.Length + 8
        self.finalOutput.Write(clr.Convert(filesize, Int64))
        
        if self.compressContent:
            self.WriteCompressedContent()
        else:
            self.WriteUncompressedContent()
    
    def WriteCompressedContent(self):
        raise NotImplementedError()
        
    def WriteUncompressedContent(self):
        self.headerContent.Seek(0, SeekOrigin.Begin)
        self.contentData.Seek(0, SeekOrigin.Begin)
        self.Pump(self.headerContent, self.finalOutput)
        self.Pump(self.contentData, self.finalOutput)
        
    def Pump(self, input, output):
        bytes = Array.CreateInstance(Byte, 4096)    # 4Kib at a time
        while 1:
            n = input.Read(bytes, 0, bytes.Length)
            if n == 0:
                break
            output.Write(bytes, 0, n)

if __name__ == "__main__":
    f = File.Create("test.MEB")
    writer = ContentWriter(f)
    writer.WriteInt(42)
    writer.Flush()
    f.Close()
            