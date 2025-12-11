using System;
using System.Reflection;
var asm = Assembly.LoadFrom("lib/Grading - Overhaul.dll");
foreach (var t in asm.GetTypes()) {
    Console.WriteLine($"Type: {t.FullName}");
    if (t.Name.Contains("CompanyStamp")) {
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
            var parms = string.Join(", ", Array.ConvertAll(m.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"  Method: {m.Name}({parms}) -> {m.ReturnType.Name}");
        }
    }
}
