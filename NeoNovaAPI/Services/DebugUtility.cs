using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeoNovaAPI.Services
{
    public static class DebugUtility
    {
        private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        public static void DebugLine(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!string.IsNullOrEmpty(BasePath) && filePath.StartsWith(BasePath))
            {
                filePath = filePath.Remove(0, BasePath.Length);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Debug.WriteLine($"{filePath}({lineNumber}): {memberName} - {"neokage, " + message}");
            Console.ResetColor();
        }

        public static void DebugForeignKeys<T>(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                string message = "Foreign keys found for object: ";
                bool hasForeignKeys = false;

                foreach (var prop in obj.GetType().GetProperties())
                {
                    var foreignKeyAttribute = prop.GetCustomAttribute<ForeignKeyAttribute>();
                    if (foreignKeyAttribute != null)
                    {
                        message += $"{prop.Name} (refers to {foreignKeyAttribute.Name}), ";
                        hasForeignKeys = true;
                    }
                }

                if (hasForeignKeys)
                {
                    message = message.TrimEnd(',', ' ');
                }
                else
                {
                    message += "None";
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Debug.WriteLine(message);
                Console.ResetColor();
            }
        }

        public static void DebugAttributes<T>(T obj)
        {
            string message = "Attributes found for object: ";
            bool hasAttributes = false;

            foreach (var prop in obj.GetType().GetProperties())
            {
                var attributes = prop.GetCustomAttributes();
                foreach (var attr in attributes)
                {
                    message += $"{prop.Name} (Attribute: {attr.GetType().Name}), ";
                    hasAttributes = true;
                }
            }

            if (hasAttributes)
            {
                message = message.TrimEnd(',', ' ');
            }
            else
            {
                message += "None";
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Debug.WriteLine(message);
            Console.ResetColor();
        }
    }
}
