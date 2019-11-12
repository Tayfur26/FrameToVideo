using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SON
{
    class FFmpeg
    {
        public void Function(string arg, string TransitionPath,string framelistpath,string framelistname,string[] pixeltest)
        {
            try
            {
                string duration = "duration 0.1";
                StreamWriter Framelist = new StreamWriter(framelistpath + framelistname + ".txt");
                Console.WriteLine(framelistpath + framelistname + ".txt");
                string[] framefiles = arg.Split('~');
                //C:\\inetpub\\wwwroot\\node1\\IMG\\\\7\\3\\LOADING#7263#1#77 yapı
                for (int i = 0; i < framefiles.Length; i++)
                {
                    //Framelist.WriteLine("duration 0.0033");
                    //Daha önceden oluşmuş geçiş frameleri olup olmadığına bakılan yer
                    if (i > 0)
                    {
                        //Console.WriteLine("Geçiş Frame Var");
                        
                        string TransitionPathcheck = TransitionPath + "\\" + Frame(framefiles[i - 1])[1] + "_" + Frame(framefiles[i])[1];
                        if (Directory.Exists(TransitionPathcheck)) //Geçiş Frame kalsörü var mı ?
                        {
                            try
                            {
                                //Varsa framelistesine ekleniyor
                                if(File.Exists(TransitionPathcheck + @"\1.png")==true &&
                                    File.Exists(TransitionPathcheck + @"\2.png")==true &&
                                    File.Exists(TransitionPathcheck + @"\3.png")==true )
                                {
                                    Framelist.WriteLine("file '" + TransitionPathcheck + @"\1.png'");
                                    Framelist.WriteLine(duration);
                                    Framelist.WriteLine("file '" + TransitionPathcheck + @"\2.png'");
                                    Framelist.WriteLine(duration);
                                    Framelist.WriteLine("file '" + TransitionPathcheck + @"\3.png'");
                                    Framelist.WriteLine(duration);
                                }
                            }
                            catch(Exception ex)
                            {
                                Directory.Delete(TransitionPathcheck);
                                Console.WriteLine("Geçiş Frame Klasörü mevcut ama içeriği boş"+ ex.ToString());
                               
                            }
                            
                        }
                        else
                        {
                            
                            
                            //Yoksa Ara-frameler oluşturuluyor
                            Bitmap LeftImage = new Bitmap(Frame(framefiles[i - 1])[0] + @"\" + Frame(framefiles[i - 1])[3] + @".png");
                            Bitmap RightImage = new Bitmap(Frame(framefiles[i])[0] + @"\" + Frame(framefiles[i])[2] + @".png");

                            Directory.CreateDirectory(@TransitionPathcheck);
                           
                            FFmpeg.GetImage(LeftImage, RightImage, 0.75f, 0.25f, TransitionPathcheck + @"\1.png");
                            FFmpeg.GetImage(LeftImage, RightImage, 0.50f, 0.50f, TransitionPathcheck + @"\2.png");
                            FFmpeg.GetImage(LeftImage, RightImage, 0.25f, 0.75f, TransitionPathcheck + @"\3.png");
                            //Oluşan Framelerin piksel kontrolü
                            //Console.WriteLine("Geçiş Frame Oluşturuluyor . Folder Path " + @TransitionPathcheck);
                            //Oluşturulan Frameler listeye ekleniyor
                            Framelist.WriteLine("file '" + TransitionPathcheck + @"\1.png'");
                            Framelist.WriteLine(duration);
                            Framelist.WriteLine("file '" + TransitionPathcheck + @"\2.png'");
                            Framelist.WriteLine(duration);
                            Framelist.WriteLine("file '" + TransitionPathcheck + @"\3.png'");
                            Framelist.WriteLine(duration);
                        }
                    }
                    string[] fileArray = Directory.GetFiles(Frame(framefiles[i])[0]);
                    //dosya yolundaki frameleri alıyor

                    ArrayList liste = new ArrayList();
                    //Bu frameleri listeliyor
                    foreach (string item in fileArray)
                    {
                        liste.Add(Convert.ToInt32(Path.GetFileNameWithoutExtension(item)));
                    }
                    liste.Sort();
                    for (int j = Convert.ToInt32(Frame(framefiles[i])[2]) - 1; j < Convert.ToInt32(Frame(framefiles[i])[3]); j++)
                    {
                        //Console.WriteLine(Frame(framefiles[i])[0] + @"\" + liste[j] + ".png");
                        Bitmap img = new Bitmap(Frame(framefiles[i])[0] + @"\" + liste[j] + ".png");
                        //pikselkontrolü listesi id 7
                        if (ColorTest(Frame(framefiles[i])[0] + @"\" + liste[j] + ".png", pixeltest) == true)
                        { 
                            Framelist.WriteLine(Frame("file '" + framefiles[i])[0] + @"\" + liste[j] + ".png'");
                            Framelist.WriteLine(duration);
                        }
                        else
                        {
                            Console.WriteLine("Frame istenilen standartta değil . Frame Path:"+Frame(framefiles[i])[0] + @"\" + liste[j] + ".png");
                        }
                        //Console.WriteLine(Frame(framefiles[i])[0] + @"\" + liste[j] + ".png");
                    }
                }
                Framelist.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Frame Listesi Oluşturmada Hata" +ex.ToString());
               
            }
        }

        private static Bitmap GetImage( Bitmap leftImage,  Bitmap rightImage, float leftAlpa, float rightAlpha, string SavePath)
        {
            Bitmap bmp = new Bitmap(leftImage.Width, leftImage.Height);
            try
            {
                for (int i = 0; i < leftImage.Width; i++)
                {
                    for (int j = 0; j < leftImage.Height; j++)
                    {
                        try
                        {
                            Color sol = leftImage.GetPixel(i, j);
                            Color sag = rightImage.GetPixel(i, j);
                            bmp.SetPixel(i, j, Color.FromArgb(
                                (byte)((sol.R * leftAlpa) + (sag.R * rightAlpha)),
                                (byte)((sol.G * leftAlpa) + (sag.G * rightAlpha)),
                                (byte)((sol.B * leftAlpa) + (sag.B * rightAlpha))));
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Geçiş Frameleri Oluşturmada Hata  "+ex.ToString());
                        }
                    }
                }
                bmp.Save(SavePath);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Geçiş Frameleri Oluşturmada Hata"+ex.ToString());
            }
            return bmp;
        }

        private string[] Frame(string args)
        {
            try
            {
                string[] framearg = args.Split('#');
                return framearg;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Argumanları Bölerken Hata"+ex.ToString());
                return null;
            }
            
        }
        
        private Boolean ColorTest(string path,string[] pixeltest)
        {
            
            Bitmap img = new Bitmap(path);
            
                //string[] pixeltest = "150#200#49#49#57.220#200#49#49#57.150#300#137#94#86.220#300#137#94#86.200#70#137#94#86".Split('.');

                Color dot1 = Color.FromArgb(Convert.ToInt32(Splitsharp(pixeltest[0])[2]), Convert.ToInt32(Splitsharp(pixeltest[0])[3]), Convert.ToInt32(Splitsharp(pixeltest[0])[4]));
                Color dot2 = Color.FromArgb(Convert.ToInt32(Splitsharp(pixeltest[1])[2]), Convert.ToInt32(Splitsharp(pixeltest[1])[3]), Convert.ToInt32(Splitsharp(pixeltest[1])[4]));
                Color dot3 = Color.FromArgb(Convert.ToInt32(Splitsharp(pixeltest[2])[2]), Convert.ToInt32(Splitsharp(pixeltest[2])[3]), Convert.ToInt32(Splitsharp(pixeltest[2])[4]));
                Color dot4 = Color.FromArgb(Convert.ToInt32(Splitsharp(pixeltest[3])[2]), Convert.ToInt32(Splitsharp(pixeltest[3])[3]), Convert.ToInt32(Splitsharp(pixeltest[3])[4]));
                Color dot5 = Color.FromArgb(Convert.ToInt32(Splitsharp(pixeltest[4])[2]), Convert.ToInt32(Splitsharp(pixeltest[4])[3]), Convert.ToInt32(Splitsharp(pixeltest[4])[4]));


            if (img.GetPixel(Convert.ToInt32(Splitsharp(pixeltest[0])[0]), Convert.ToInt32(Splitsharp(pixeltest[0])[1])) == dot1 &&
                img.GetPixel(Convert.ToInt32(Splitsharp(pixeltest[1])[0]), Convert.ToInt32(Splitsharp(pixeltest[1])[1])) == dot2 &&
                img.GetPixel(Convert.ToInt32(Splitsharp(pixeltest[2])[0]), Convert.ToInt32(Splitsharp(pixeltest[2])[1])) == dot3 &&
                img.GetPixel(Convert.ToInt32(Splitsharp(pixeltest[3])[0]), Convert.ToInt32(Splitsharp(pixeltest[3])[1])) == dot4 &&
                img.GetPixel(Convert.ToInt32(Splitsharp(pixeltest[4])[0]), Convert.ToInt32(Splitsharp(pixeltest[4])[1])) == dot5
                )
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        private string[] Splitsharp(string arg)
        {
            string[] values = arg.Split('#');
            return values;
        }
    }

    class Program
    {
         static void Main(string[] args)
        {
            StreamWriter TimeWriter = new StreamWriter(@"C:\Users\tyfr_\Desktop\Times.txt");
            Stopwatch Txthazirlama = new Stopwatch();
            Stopwatch Cmdyazma = new Stopwatch();
            Txthazirlama.Start();
           // args = "7 Videoadi.mp4 26153 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACI_2#7263#1#30~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\AÇIK#7449#1#35 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)".Split(' ');
            args = "7 Videoson1.mp4 26153 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\AÇIK#7549#1#38~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\AÇIKLAMAK_1#7449#1#40~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\AÇILMAK_2#8449#1#42 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\Outfile\\ C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)".Split(' ');
            //args = "7 Videoson.mp4 26153 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(Alpha)\\png2#5454#1#62 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)".Split(' '); 

            //args = "7 Videoson.mp4 26153 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\ACİL#7268#1#37~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\AÇIK#7549#1#38~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\AÇIKLAMAK_1#7449#1#40~C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\AÇILMAK_2#8449#1#42 C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)\\Outfile\\ C:\\Users\\tyfr_\\Desktop\\Frame_to_Video\\Frames(No_Alpha)".Split(' ');


            FFmpeg ffmpeg = new FFmpeg();

            string[] pixeltest;
            String VideoName = args[1].Split('.')[0];
            String VideoPath= args[4].Replace("%20", " ");
            String TransitionPath = args[5].Replace("%20", " ");
            String CodecPath = @"C:/Program Files/ffmpeg/bin"; //ffmpeg windows codec kurulumun yapıldığı yol
            String FramelistName = VideoName;   //Frame Listesi için oluşan txt adı 
            String FramelistPath= @"C:\Program Files\ffmpeg\bin\"; // Framelist in yolu
            String VideoUzantisi = "webm";
            try
            {
                if (Convert.ToInt32(args[0]) == 7)//translator id ye göre framelerin belirli piksellerin rgb kontrolü
                {
                    pixeltest = "150#200#49#49#57.220#200#49#49#57.150#300#137#94#86.220#300#137#94#86.200#70#137#94#86".Split('.');
                }
                else
                {
                    pixeltest = "150#200#49#49#57.220#200#49#49#57.150#300#137#94#86.220#300#137#94#86.200#70#137#94#86".Split('.');
                }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Piksel kontrolü Hatası" + ex.ToString());
                pixeltest = null;
            }
            ffmpeg.Function(args[3], TransitionPath, FramelistPath, FramelistName, pixeltest);

            //Hazırlanan framelerin ffmpeg(CMD) ile yazılması
            string command = "cd " + CodecPath + " & ffmpeg -benchmark -safe 0 -r 29 -f concat -i " + FramelistName + ".txt -pix_fmt yuva420p -c:v libvpx-vp9 -threads 3 " + @VideoPath+VideoName+"."+VideoUzantisi;
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo("cmd")
                {
                //    RedirectStandardInput = true,
                //    RedirectStandardOutput = true,
                };
                startInfo.Verb = "runas";
                startInfo.WorkingDirectory = @"C:/Program Files/ffmpeg/bin";//codec exe nin kurulu olduğu dizin
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/user:tyfr \"cmd /K " + command /*+ "& exit"*/;
                
                
                process.StartInfo = startInfo;
                

                process.Start();
                
                Txthazirlama.Stop();
                Cmdyazma.Start();
                //process.BeginOutputReadLine();

                //StreamWriter inputWriter = process.StandardInput;
                //StreamReader outputReader = process.StandardOutput;
                //StreamReader errorReader = process.StandardError;
                

                
                Console.WriteLine("Video Oluşturuluyor");
                 process.WaitForExit();
                
                if (process.HasExited)
                {
                    File.Delete(FramelistPath + FramelistName + ".txt");//Cmd kapandığında txt dosyası siliniyor

                    Cmdyazma.Stop();
                    TimeWriter.WriteLine((Cmdyazma.ElapsedMilliseconds/1000).ToString()+" saniye yazılma işlemi " +"\n"+ (Txthazirlama.ElapsedMilliseconds/1000).ToString()+" Saniye Araframe ve liste oluşturma ");
                    TimeWriter.Close();
                    process.Kill();
                }
            }
            catch(Exception ex)
            {
                
                Console.WriteLine("Video oluşturma Hatası" +ex.ToString());
            }
        }
    }
}

