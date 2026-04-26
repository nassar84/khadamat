using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

class Program
{
    static void Main()
    {
        string path = @"d:\DEV\khadamat\src\Khadamat.BlazorUI\bin\Release\net8.0\Khadamat.BlazorUI.dll";
        using var stream = File.OpenRead(path);
        using var reader = new PEReader(stream);
        var metadataReader = reader.GetMetadataReader();

        using var fileOut = new StreamWriter(@"d:\DEV\khadamat\types.txt");
        foreach (var typeHandle in metadataReader.TypeDefinitions)
        {
            var typeDef = metadataReader.GetTypeDefinition(typeHandle);
            string ns = metadataReader.GetString(typeDef.Namespace);
            string name = metadataReader.GetString(typeDef.Name);
            fileOut.WriteLine($"{ns}.{name}");
        }
        Console.WriteLine("Done scanning metadata.");
    }
}
