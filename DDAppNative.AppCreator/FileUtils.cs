using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDAppNative.AppCreator
{
    class FileUtils
    {
        public static void ReplaceInFiles(string dirPath, Dictionary<string, string> variables)
        {
            foreach (string newPath in Directory.GetFiles(dirPath, "*.*",
                SearchOption.AllDirectories))
                ReplaceInFile(newPath, variables);
        }

        public static void ReplaceInFile(string filePath, Dictionary<string, string> variables)
        {
            var content = File.ReadAllBytes(filePath);
            foreach (var variable in variables)
            {
                var key = Encoding.ASCII.GetBytes(variable.Key);
                var value = Encoding.ASCII.GetBytes(variable.Value);
                content = ReplaceBytes(content, key, value);
            }
            File.WriteAllBytes(filePath, content);
        }

        public static bool PathExists(string path)
        {
            return File.Exists(path);
        }

        public static void CopyFile(string fromFile, string toFile)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(toFile));
            File.Copy(fromFile, toFile, true);
        }

        public static void CopyFiles(string fromDir, string toDir)
        {
            foreach (string dirPath in Directory.GetDirectories(fromDir, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(fromDir, toDir));

            foreach (string newPath in Directory.GetFiles(fromDir, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(fromDir, toDir), true);
        }

        public static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            // handle the complete source array
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
            int index = FindBytes(src, search);
            if (index >= 0)
            {
                var dst = new byte[src.Length - search.Length + repl.Length];
                // before found array
                Buffer.BlockCopy(src, 0, dst, 0, index);
                // repl copy
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                // rest of src array
                Buffer.BlockCopy(
                    src,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    src.Length - (index + search.Length));

                return dst;
            }
            return src;
        }
    }
}
