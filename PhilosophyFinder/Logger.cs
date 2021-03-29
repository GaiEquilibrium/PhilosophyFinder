//TODO:
//
//вынести параметры из класса в файл
//
//сделать возможность работатьс несколькими файлами в параллель


using System;
using System.IO;

namespace PhilosophyFinder
{
    public class Logger
    {
        private string _messageFormat = "Шаг {0} - {1} - {2}";
        private string fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt";
        private StreamWriter fileStrieam;

        public Logger(string resultDirectory)
        {
            string fullFileName = Path.Combine(resultDirectory, fileName);
            File.Create(fullFileName).Close();
            fileStrieam = new StreamWriter(fullFileName, append: true);
        }

        public void Log(string Line)
        {
            fileStrieam.WriteLine(Line);
        }

        public void Log(string step, string header, string link)
        {
            Log(string.Format(_messageFormat, step, header, link));
        }

        public void stopLogging()
        {
            fileStrieam.Flush();
            fileStrieam.Close();
        }
    }
}
