using System;

namespace StrBin
{
    /// <summary>
    /// Program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// エントリポイント
        /// </summary>
        /// <param name="args">引数</param>
        static void Main(string[] args)
        {
            var arg = Arg.Parse(args);
            if (arg == null)
            {
                Console.WriteLine(Arg.Usage());
                return;
            }

            try
            {
                switch (arg.Mode)
                {
                    case Mode.B:
                        if (string.IsNullOrEmpty(arg.OutputPath))
                        {
                            Console.WriteLine(StrBin.BinaryFileToHexString(arg.InputPath, arg.Prefix, arg.Separator));
                        } 
                        else
                        {
                            StrBin.BinaryFileToTextFile(arg.InputPath, arg.Prefix, arg.Separator, arg.OutputPath);
                            Console.WriteLine($"Created {arg.OutputPath}");
                        }
                        break;

                    case Mode.T:
                        StrBin.TextFileToBinaryFile(arg.InputPath, arg.OutputPath);
                        Console.WriteLine($"Created {arg.OutputPath}");
                        break;

                    case Mode.S:
                        StrBin.HexStringBinaryFile(arg.InputText, arg.OutputPath);
                        Console.WriteLine($"Created {arg.OutputPath}");
                        break;

                    default:
                        Console.WriteLine(Arg.Usage());
                        break;
                }
            } 
            catch (StrBinException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception)
            {
                Console.WriteLine("Error.");
                Console.WriteLine(Arg.Usage());
            }

            return;
        }
    }
}
