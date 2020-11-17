using System;
using System.Collections.Generic;

namespace SourceGenerator.Test
{
    public class A
    {
        public string Ab { get; set; }
        public int B { get; set; }
    }

    public class B
    {
        public int MyProperty { get; set; }
        public string MyProperty2 { get; set; }
    }


    public enum Accessibility
    {
        Private,
        Protected,
        Internal,
        Public
    }

    public partial class CompiledPropertyInfo
    {
        public CompiledPropertyInfo(string name, string typeName, Accessibility getAccessibility, Accessibility setAccessibility, bool isInitOnly)
        {
            Name = name;
            TypeName = typeName;
            GetAccessibility = getAccessibility;
            SetAccessibility = setAccessibility;
            IsInitOnly = isInitOnly;
        }

        public string Name { get; }
        public string TypeName { get; }
        public Accessibility GetAccessibility { get; }
        public Accessibility SetAccessibility { get; }
        public bool IsInitOnly { get; }

        public partial object GetValue(object instance);

        public partial object GetValue(object instance)
        {
            return instance switch
            {
                A i => GetValue(i),
                B i => GetValue(i),
                _ => null
            };
        }

        private object GetValue(A instance)
        {
            return Name switch
            {
                "Ab" => instance.Ab,
                "B" => instance.B,
                _ => throw new NotSupportedException()
            };
        }

        private object GetValue(B instance)
        {
            return Name switch
            {
                "MyProperty" => instance.MyProperty,
                "MyProperty2" => instance.MyProperty2,
                _ => throw new NotSupportedException()
            };
        }

        public partial bool TrySetValue(object instance, object value);

        public partial bool TrySetValue(object instance, object value)
        {
            return instance switch
            {
                A i => TrySetValueInternal(i, value),
                B i => TrySetValueInternal(i, value),
                _ => false
            };
        }

        private bool TrySetValueInternal(A instance, object value)
        {
            bool success = true;

            switch ((Name, value))
            {
                case ("Ab", string v): instance.Ab = v; break;
                case ("B", int v): instance.B = v; break;
                default: success = false; break;
            }

            return success;
        }

        private bool TrySetValueInternal(B instance, object value)
        {
            bool success = true;

            switch ((Name, value))
            {
                case ("MyProperty", int v): instance.MyProperty = v; break;
                case ("MyProperty2", string v): instance.MyProperty2 = v; break;
                default: success = false; break;
            }

            return success;
        }
    }

    public static partial class CompiledReflection
    {
        private static Dictionary<string, Dictionary<string, object>> _registeredTypes = new Dictionary<string, Dictionary<string, object>>();

        public static IReadOnlyDictionary<string, object> GetProperties<T>()
        {
            _registeredTypes.TryGetValue(typeof(T).FullName, out var result);

            return result ?? new Dictionary<string, object>();
        }

        public static partial IEnumerable<string> GetPropertyNames<T>();

        public static partial IEnumerable<string> GetPropertyNames<T>()
        {
            var wrapper = new TypeWrapper<T>();

            return wrapper switch
            {
                TypeWrapper<A> w => GetPropertyNames(w),
                TypeWrapper<B> w => GetPropertyNames(w),
                _ => throw new System.NotImplementedException()
            };
        }

        public static IEnumerable<CompiledPropertyInfo> GetPropertyInfo<T>()
        {
            var wrapper = new TypeWrapper<T>();

            return wrapper switch
            {
                TypeWrapper<A> w => GetPropertyInfo(w),
                TypeWrapper<B> w => GetPropertyInfo(w),
                _ => throw new System.NotImplementedException()
            };
        }

        private static IEnumerable<CompiledPropertyInfo> GetPropertyInfo(TypeWrapper<A> wrapper)
        {
            return new List<CompiledPropertyInfo>
            {
                new CompiledPropertyInfo("Ab", typeof(string).FullName, Accessibility.Public, Accessibility.Public, false),
                new CompiledPropertyInfo("B", typeof(int).FullName, Accessibility.Public, Accessibility.Public, false),
            };
        }

        private static IEnumerable<CompiledPropertyInfo> GetPropertyInfo(TypeWrapper<B> wrapper)
        {
            return new List<CompiledPropertyInfo>
            {

            };
        }

        private static IEnumerable<string> GetPropertyNames(TypeWrapper<A> wrapper)
        {
            return new List<string>
            {
                ""
            };
        }

        private static IEnumerable<string> GetPropertyNames(TypeWrapper<B> wrapper)
        {
            return null;
        }
    }

    public class TypeWrapper<T>
    {

    }
}
