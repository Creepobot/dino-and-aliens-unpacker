using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DinoAndAliensUnpacker
{
    internal static class Program
    {
        private static int Fail(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Нажмите любую клавишу чтобы выйти... Press any key to exit...");
            Console.ReadKey();
            return 1;
        }

        private static bool XorToggle()
        {
            do
            {
                Console.Write("Использовать Xor шифрование? (Рекомендуется по умолчанию) [Д/Н]. Use Xor encryption? (Recommended by default) [Y/N]: ");
                var key = Console.ReadLine();
                switch (key?.ToLower())
                {
                    case "д":
                    case "y":
                        return true;
                    case "н":
                    case "n":
                        return false;
                    default:
                        Console.WriteLine("Неправильный аргумент. Выберите между Д и Н. Invalid argument. Choose between Y and N");
                        break;
                }
            } while (true);
        }

        private static void Xor(IList<byte> data, string key)
        {
            for (var i = 0; i < data.Count; i++)
            {
                data[i] ^= (byte) key[i % key.Length];
            }
        }

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length < 1) return Fail("Для того чтобы запаковать папку или распаковать .dat - перетащите файл на DinoAndAliensUnpacker.exe\nIn order to pack folder, or unpack .dat - drag and drop the file on the DinoAndAliensUnpacker.exe");

            if (File.Exists(args[0]))
            {
                var basePath = Path.Combine(Path.GetDirectoryName(args[0]) ?? ".", Path.GetFileNameWithoutExtension(args[0]));

                var xor = XorToggle();
                
                using (var file = File.OpenRead(args[0]))
                using (var reader = new BinaryReader(file))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var name = reader.ReadSZString();
                        var len = reader.ReadInt32();
                        var data = reader.ReadBytes(len);

                        var path = Path.Combine(basePath, name);

                        var directory = Path.GetDirectoryName(path);
                        if (directory != null) Directory.CreateDirectory(directory);
                        
                        Console.WriteLine($"Распаковка {name}... Unpacking {name}...");
                        
                        if (xor)
                            Xor(data, name);
                        File.WriteAllBytes(path, data);
                    }
                }
            }
            else if (Directory.Exists(args[0]))
            {
                var files = Directory.EnumerateFiles(args[0], "*.*", SearchOption.TopDirectoryOnly).ToArray();
                var datFile = Path.Combine(Path.GetDirectoryName(args[0]) ?? ".", Path.GetFileName(args[0]) + ".dat");
                if (File.Exists(datFile)) File.Delete(datFile);
                
                var xor = XorToggle();

                using (var stream = File.OpenWrite(datFile))
                using (var writer = new BinaryWriter(stream))
                {
                    foreach (var file in files)
                    {
                        var name = Path.GetFileName(file);
                        var data = File.ReadAllBytes(file);
                        var len = data.Length;
                        
                        if (xor)
                            Xor(data, name);

                        Console.WriteLine($"Запаковка {name}... Packing {name}...");
                        writer.Write(Encoding.UTF8.GetBytes(name));
                        writer.Write('\0');
                        writer.Write(len);
                        writer.Write(data);
                    }
                }
            }
            else
            {
                return Fail("Файл не найден. File not found.");
            }

            Console.WriteLine("Готово! Done!");
            Console.WriteLine("Нажмите любую клавишу чтобы выйти... Press any key to exit...");
            Console.ReadKey();
            return 0;
        }
    }
}
