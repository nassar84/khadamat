using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var lib = Assembly.LoadFrom(@"d:\DEV\khadamat\src\Khadamat.BlazorUI\bin\Debug\net8.0\Khadamat.BlazorUI.dll");
        var type = lib.GetType("Khadamat.BlazorUI.Helpers.CategoryIconResolver");
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        Console.WriteLine("Static constructor executed successfully without ArgumentException!");
    }
}
