using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrBin
{
    /// <summary>
    /// モード
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// バイナリファイル入力
        /// </summary>
        B,

        /// <summary>
        /// 16進数テキストファイル入力
        /// </summary>
        T,

        /// <summary>
        /// 16進数文字列入力
        /// </summary>
        S,
    }

    /// <summary>
    /// 引数情報
    /// </summary>
    internal class Arg
    {
        /// <summary>
        /// パラメータキー
        /// </summary>
        private enum Key
        {
            /// <summary>
            /// 入力ファイルパス
            /// </summary>
            InputPath,

            /// <summary>
            /// 出力ファイルパス
            /// </summary>
            OutputPath,

            /// <summary>
            /// 入力文字列
            /// </summary>
            InputText,

            /// <summary>
            /// 出力16進数文字列プレフィクス
            /// </summary>
            Prefix,

            /// <summary>
            /// 出力16進数文字列区切り文字
            /// </summary>
            Separator,
        }

        /// <summary>
        /// モード情報
        /// </summary>
        private class ModeInfo
        {
            /// <summary>
            /// モード
            /// </summary>
            public Mode Mode { get; private set; }

            /// <summary>
            /// モード内容
            /// </summary>
            public string Description { get; private set; }

            /// <summary>
            /// 必須パラメータ
            /// </summary>
            public Key[] Required { get; private set; }

            /// <summary>
            /// 任意パラメータ
            /// </summary>
            public Key[] Optional { get; private set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="mode">モード</param>
            /// <param name="description">モード内容</param>
            /// <param name="required">必須パラメータ</param>
            /// <param name="optional">任意パラメータ</param>
            public ModeInfo(Mode mode, string description, Key[] required, Key[] optional) 
            {
                this.Mode = mode;
                this.Description = description;
                this.Required = required ?? new Key[0];
                this.Optional = optional ?? new Key[0];
            }
        }

        /// <summary>
        /// モード情報
        /// </summary>
        private static readonly List<ModeInfo> ModeInfoList = new List<ModeInfo>()
        {
            new ModeInfo(
                Mode.B,
                "Binary File To Hex String", 
                new Key[] { Key.InputPath }, 
                new Key[] { Key.OutputPath, Key.Prefix, Key.Separator }
            ),
            new ModeInfo(
                Mode.T,
                "Hex String File To Binary File", 
                new Key[] { Key.InputPath, Key.OutputPath }, 
                new Key[] { }
            ),
            new ModeInfo(
                Mode.S,
                "Hex String To Binary File", 
                new Key[] { Key.InputText, Key.OutputPath }, 
                new Key[] { }
            ),
        };

        /// <summary>
        /// 引数ディクショナリ
        /// </summary>
        private readonly Dictionary<Key, string> keyValues;

        /// <summary>
        /// モード
        /// </summary>
        public Mode Mode { get; private set; }

        /// <summary>
        /// 入力ファイルパス
        /// </summary>
        public string InputPath { get { return this.GetValue(Key.InputPath, string.Empty); } }

        /// <summary>
        /// 出力ファイルパス
        /// </summary>
        public string OutputPath { get { return this.GetValue(Key.OutputPath, string.Empty); } }

        /// <summary>
        /// 入力文字列
        /// </summary>
        public string InputText { get { return this.GetValue(Key.InputText, string.Empty); } }

        /// <summary>
        /// 出力16進数文字列プレフィクス
        /// </summary>
        public string Prefix { get { return this.GetValue(Key.Prefix, string.Empty); } }

        /// <summary>
        /// 出力16進数文字列区切り文字
        /// </summary>
        public string Separator { get { return this.GetValue(Key.Separator, string.Empty); } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mode">モード</param>
        /// <param name="keyValues">引数ディクショナリ</param>
        private Arg(Mode mode, Dictionary<Key, string> keyValues)
        {
            this.Mode = mode;
            this.keyValues = keyValues;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        /// <returns>引数情報インスタンス</returns>
        public static Arg Parse(string[] args)
        {
            Arg arg = null;

            try
            {
                // モード
                var mode = (Mode)Enum.Parse(typeof(Mode), args[0]);
                var modeInfo = ModeInfoList.FirstOrDefault(m => m.Mode == mode);
                if (modeInfo == null)
                {
                    // モード情報なし
                    throw new NotSupportedException();
                }

                var dic = new Dictionary<Key, string>();

                // 必須パラメータを取得
                var argIndex = 1;
                foreach (var key in modeInfo.Required)
                {
                    dic.Add(key, args[argIndex]);
                    argIndex++;
                }

                // 任意パラメータ
                Key? lastKey = null;
                for (var i = argIndex; i < args.Length; i++)
                {
                    // キーか判定
                    Key? key = modeInfo.Optional
                                       .Select(o => o as Key?)
                                       .FirstOrDefault(o => GetCode(o.Value).Equals(args[i], StringComparison.OrdinalIgnoreCase));

                    if (lastKey.HasValue)
                    {
                        // キーが取得済みの場合
                        if (key.HasValue)
                        {
                            // キーの連続は不可
                            break;
                        }

                        // キーと値を追加
                        dic.Add(lastKey.Value, args[i]);

                        key = null;
                    }
                    else
                    {
                        // キーが未取得の場合
                        if (!key.HasValue)
                        {
                            // キーでない場合は不可
                            throw new ArgumentException(lastKey.ToString());
                        }
                    }

                    lastKey = key;
                }

                if (lastKey.HasValue)
                {
                    // キーのあとに値がない
                    throw new ArgumentException();
                }

                // インスタンス生成
                arg = new Arg(mode, dic);
            }
            catch (Exception)
            {
                // 解析失敗
                arg = null;
            }

            return arg;
        }

        /// <summary>
        /// 使用方法
        /// </summary>
        /// <returns>使用方法</returns>
        public static string Usage()
        {
            if (!ModeInfoList.Any())
            {
                return string.Empty;
            }

            var descriptionWidth = ModeInfoList.Select(m => m.Description.Length).Max();

            return $"Usage: \n" +
                   string.Join("\n", ModeInfoList.Select(m =>
                   {
                       var usage = string.Join(" ", new string[]
                       {
                           "StrBin.exe",
                           m.Mode.ToString(),
                           string.Join(" ", m.Required.Select(k => $"<{k}>")),
                           string.Join(" ", m.Optional.Select(k => $"[{GetCode(k)} <{k}>]")),
                       }.Where(s => !string.IsNullOrWhiteSpace(s)));
                       return $"  {m.Description.PadRight(descriptionWidth, ' ')} : {usage}";
                   }));
        }

        /// <summary>
        /// キーから省略文字を取得
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>省略文字</returns>
        private static string GetCode(Key key)
        {
            return $"-{key.ToString().First()}";
        }

        /// <summary>
        /// ディクショナリから値取得
        /// </summary>
        /// <typeparam name="T">値の型</typeparam>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">初期値</param>
        /// <returns>値</returns>
        private T GetValue<T>(Key key, T defaultValue)
        {
            T value = defaultValue;
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(typeof(string)))
                {
                    value = (T)converter.ConvertFromString(this.keyValues[key]);
                }
            } 
            catch (Exception)
            {
                value = defaultValue;
            }

            return value;
        }
    }
}
