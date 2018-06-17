using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

/**
 * Сегодня(13.05.2018)
 * Наконец-то я освоил Многопоточного програмирование 
 * Хотя не так профессиально, но я уже начал понимать принципы параллельного програмирование
 * И наконец-то применял на практике 
 * Решил свой задачу применяя параллелизма
 * Задача вот так: нам нужно сгенирировать MD5 хеши файлов в определенном каталоге 
 * Без паралеллизма программа сгенерировал MD5 хеши около 17,000 файлов за 3 минута
 * С помощью многопоточного програмирование, программа сгенерировал MD5 хеши около 17,000 файлов за 15 секунда!
 */ 

namespace GenerateHash
{
    class Program
    {
        private static string path;
        //словарь хранить себя хеши
        public static List<KeyValuePair<string, string>> HashDict = new List<KeyValuePair<string, string>>();       

        static void Main(string[] args)
        {
            bool has_optional_arg = false;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Generator Hashes");

            foreach(var arg in args)
            {
                if (arg == "-path")
                    has_optional_arg = true;
            }

            if (has_optional_arg)
            {
                try
                {
                    path = args[2];
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {             
                Console.WriteLine("Set Directory: ");
                path = Console.ReadLine();
                Console.WriteLine("\nNow we generating file hashes in the directory: \n{0}", path);
                Console.WriteLine("\nPress Enter for continue: ");
                Console.ReadLine();
            }

            Console.WriteLine("\nNow we generating file hashes in the directory: \n{0}", path);
            GetGameFileHashes();
            Console.WriteLine("Finished hashing");
            Console.ReadLine();
        }

       

        public static void GetGameFileHashes()
        {
            FileStream hash_file = new FileStream("Client_Mod_Hashes.txt", FileMode.Truncate);
            StreamWriter writer = new StreamWriter(hash_file);
            int count = 0;

            //получить польный список файлов(где-то 17-18к шт.)
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);        

            //Многопоточный режим
            Action action = new Action(() => {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;

                Parallel.ForEach(files, (file) => {
                    if (!file.Contains(".git") && !file.Contains("GenerateHash")) //не счытивать файлы гита
                    {
                        string file_name = Path.GetFileName(file);
                        using (var stream = File.OpenRead(file))
                        {
                            KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(file_name, GetHash_MD5(stream).ToString());
                            HashDict.Add(keyValuePair);
                            Console.WriteLine(count);
                        }
                    }
                });
            });

            Task taskParallel = new Task(action);
            taskParallel.Start();
            taskParallel.Wait();

            //Отсортировать словарь по ключом
            HashDict = HashDict.OrderBy(pair => pair.Key).ToList();             

            Task taskWriter = new Task(() => {  
                foreach (var item in HashDict)
                {
                    writer.WriteLine(item.Value + " - " + item.Key);
                }
            });

            taskWriter.Start();
            taskWriter.Wait();   
            writer.Close();
        }

        //получить md5 хеша
        public static string GetHash_MD5(Stream stream)
        {
            byte[] bytes;
            using (var md5 = new MD5CryptoServiceProvider())
            {
                bytes = md5.ComputeHash(stream);
            }


            var buffer = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                buffer.AppendFormat("{0:x2}", b);
            }

            return buffer.ToString();
        }
    }
}
