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
        public static Dictionary<string, string> HashDict = new Dictionary<string, string>();       

        static void Main(string[] args)
        {           
            Console.BackgroundColor = ConsoleColor.DarkRed;

            Console.WriteLine("Generator Hashes");
            Console.WriteLine("Set Directory: ");

            path = Console.ReadLine();
           
            Console.WriteLine("\nNow we generating file hashes in the directory: \n{0}", path);
            Console.WriteLine("\nPress Enter for continue: ");
            Console.ReadLine();
           
            GetGameFileHashes();

            Console.WriteLine("Finished hashing");
            Console.ReadLine();
        }

       

        public static void GetGameFileHashes()
        {
            FileStream hash_file = new FileStream("File_Hashes.txt", FileMode.Truncate);
            StreamWriter writer = new StreamWriter(hash_file);
            int count = 0;

            //получить польный список файлов(где-то 17-18к шт.)
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            /*foreach (var file in files)
            {
                if (!file.Contains(".git") && !file.Contains("GenerateHash")) //не счытивать файлы гита
                {
                    string file_name = file.ToString().Remove(0, Path.Length);
                    using (var stream = File.OpenRead(file))
                    {
                        writer.WriteLine(GetHash_MD5(stream).ToString() + " - " + file_name); //записать на файле
                        count++;
                        Console.WriteLine(count);
                        //HashDict.Add(file_name, GetHash_MD5(stream).ToString());
                    }
                }
            }
            */


            Action action = new Action(() => {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;               
                Parallel.ForEach(files, (file) => {
                    if (!file.Contains(".git") && !file.Contains("GenerateHash")) //не счытивать файлы гита
                    {
                        string file_name = file.ToString().Remove(0, path.Length);
                        using (var stream = File.OpenRead(file))
                        {
                            //writer.WriteLine(GetHash_MD5(stream).ToString() + " - " + file_name); //записать на файле                        
                            count++;
                            Console.WriteLine(count);
                            //Console.WriteLine("Pririoty: {0}, ID: {1}", Thread.CurrentThread.Priority, Thread.CurrentThread.ManagedThreadId);
                            HashDict.Add(file_name, GetHash_MD5(stream).ToString());                           
                        }
                    }
                });
            });

            Task taskParallel = new Task(action);
            taskParallel.Start();
            taskParallel.Wait();

            //Отсортировать словарь по ключом
            HashDict = HashDict.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair=>pair.Value);      

            Task taskWriter = new Task(() => {  
                foreach (var item in HashDict)
                {
                    writer.WriteLine(item.Value + " - " + item.Key);
                }
            });

            taskWriter.Start();
            taskWriter.Wait();

            /*Task task = new Task(() =>
            {            
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                //Console.WriteLine("Pririoty: {0}", Thread.CurrentThread.Priority);

                foreach (var file in files)
                {
                    if (!file.Contains(".git") && !file.Contains("GenerateHash")) //не счытивать файлы гита
                    {
                        string file_name = file.ToString().Remove(0, path.Length);
                        using (var stream = File.OpenRead(file))
                        {
                            writer.WriteLine(GetHash_MD5(stream).ToString() + " - " + file_name); //записать на файле
                            count++;
                            Console.WriteLine(count);
                            
                            //HashDict.Add(file_name, GetHash_MD5(stream).ToString());
                        }
                    }
                }
            });
            
            task.Start();
            task.Wait();
            */

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
