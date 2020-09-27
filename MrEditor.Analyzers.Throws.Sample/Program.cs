using System;

namespace MrEditor.Analyzers.Throws.Sample
{
    class Program
    {
        [ThrowsNothing]
        static void Main()
        {
#pragma warning disable TRW002 // Method or property declares [ThrowsNothing] but some exceptions may be thrown inside.
            Console.WriteLine("Hello World");
#pragma warning restore TRW002 // Method or property declares [ThrowsNothing] but some exceptions may be thrown inside.
        }
    }
}
