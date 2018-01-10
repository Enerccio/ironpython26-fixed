using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using CommandAndConquer.CLI.Attributes;
using Microsoft.Scripting.Hosting;

namespace CommandAndConquer.CLI.Template1.Controllers
{
    [CliController("example", "This is a description of the controller.")]
    public static class ExampleController
    {
        private static object BindConstructor(Type type, Type[] parameters, Type delegateType)
        {
            DynamicMethod dm = new DynamicMethod("Constructor", type, parameters, type, true);
            ILGenerator il = dm.GetILGenerator();

            ConstructorInfo ctor = type.GetConstructor(parameters);
            if (ctor == null && type.IsValueType)
            {
                il.DeclareLocal(type);
                il.Emit(OpCodes.Ldloca_S, 0);
                il.Emit(OpCodes.Initobj, type);
                il.Emit(OpCodes.Ldloc, 0);
                il.Emit(OpCodes.Ret);
            }
            else
            {

                for (int i = 0; i < parameters.Length; i++)
                    il.Emit(OpCodes.Ldarg, i);
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ret);
            }

            return dm.CreateDelegate(delegateType);
        }

        public struct MyStruct
        {

            private int _x;

            public int x
            {
                get
                {
                    return _x;
                }

                set
                {
                    _x = value;
                }
            }

            public int z;

        }

        private static void Log(object o)
        {
            Console.Write(o.ToString());
            Console.WriteLine();
        }

        [CliCommand("test", "This is an example of how you can setup methods.")]
        public static void MethodExample(string something, List<string> someMoreThings = null)
        {
            ScriptEngine scriptEngine = IronPython.Hosting.Python.CreateEngine();
            ScriptScope scriptScope = scriptEngine.CreateScope();

            scriptScope.SetVariable("MyStruct", BindConstructor(typeof(MyStruct), new Type[] { }, typeof(Func<MyStruct>)));
            scriptScope.SetVariable("log", new Action<object>(Log));

            List<string> paths = new List<string>();
            // to load python stdlib - not needed
            // paths.Add (Application.dataPath + "/StreamingAssets/pystdlib");
            scriptEngine.SetSearchPaths(paths);

            string scriptPath = "d:\\dev\\unity\\pytest\\PythonTest\\Assets\\StreamingAssets\\MyScript.py";
            string script = File.ReadAllText(scriptPath);

            ScriptSource src = scriptEngine.CreateScriptSourceFromString(script);
            src.Execute(scriptScope);

            Console.ReadKey();
        }
    }
}