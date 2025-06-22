using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }
    }
}







//    string paragraph = "في قديم الزمان، كان هناك رجل يعيش في قرية صغيرة تحيط بها الجبال من كل الجهات. " +
//                         "كان الرجل معروفًا بالحكمة والصبر وكان الناس يأتون إليه من القرى المجاورة لطلب النصيحة والمشورة. " +
//                         "في يوم من الأيام، جاءه شاب متهور يسأله عن معنى الحياة، فأجابه الرجل بابتسامة وقال له: " +
//                         "\"الحياة ليست شيئًا يمكن أن يُفهم في يوم وليلة، ولكن إن بدأت بالسؤال، فقد سلكت الطريق الصحيح.\"\n";

//    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
//    string filePath = Path.Combine(desktopPath, "test_large2.txt");

//    using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
//    {
//        for (int i = 0; i < 20000; i++)
//        {
//            writer.Write(paragraph);
//        }
//    }

//    Console.WriteLine("✅ تم إنشاء الملف بنجاح على سطح المكتب.");
