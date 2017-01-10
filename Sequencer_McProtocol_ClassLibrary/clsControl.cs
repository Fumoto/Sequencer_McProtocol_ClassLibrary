using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemControl
{
    //コンボボックスに追加する項目となるクラス
    public class MyComboBoxItem
    {
        private string m_id = string.Empty;
        private string m_name = string.Empty;

        //コンストラクタ
        public MyComboBoxItem(string id, string name)
        {
            m_id = id;
            m_name = name;
        }

        //実際の値
        public string Id
        {
            get
            {
                return m_id;
            }
        }

        //表示名称
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        //オーバーライドしたメソッド
        //これがコンボボックスに表示される
        public override string ToString()
        {
            return m_name;
        }
    }


    public class ConvertDispName
    {

        public string ConvPortName(int PortNo)
        {
            string RetPortNo = "COM" + PortNo.ToString();
            return RetPortNo;

        }
        public string ConvAxis(int Axis)
        {
            if (Axis == 1)
            {
                return "X軸";
            }
            else
            {
                return "Y軸";
            }
        }
        /// <summary>
        /// ConvAngle 数値を角度表示に変換
        /// </summary>
        /// <param name="GetData"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public string ConvAngle(double GetData, int angle)
        {
            const int UD = 1;
            const int LR = 0;
            string WkAngle = string.Empty;
            double AngleData = double.Parse(GetData.ToString("00.00"));
            switch (angle)
            {
                case UD:

                    if (AngleData > 0)
                    {
                        WkAngle = AngleData.ToString("0.00") + "U";
                    }
                    else if (AngleData < 0)
                    {
                        WkAngle = AngleData.ToString("0.00") + "D";
                        WkAngle = WkAngle.Substring(1, WkAngle.ToString().Length - 1);
                    }
                    else
                    {
                        WkAngle = "H";
                    }
                    break;

                case LR:
                    if (AngleData > 0)
                    {
                        WkAngle = AngleData.ToString("0.00") + "R";
                    }
                    else if (AngleData < 0)
                    {
                        WkAngle = AngleData.ToString("0.00") + "L";
                        WkAngle = WkAngle.Substring(1, WkAngle.ToString().Length - 1);
                    }
                    else
                    {
                        WkAngle = "V";
                    }
                    break;
            }

            return WkAngle;
        }
        /// <summary>
        /// ConvDeg　角度データを数値へ変換
        /// </summary>
        /// <param name="GetData"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public double ConvDeg(string GetData, int angle)
        {
            const int UD = 1;
            const int LR = 0;
            double WkAngle = 0;


            WkAngle = Validation.Val(GetData);
            switch (angle)
            {
                case UD:
                    if (VBStrings.Right(GetData, 1) == "U")
                    {
                        WkAngle = Math.Abs(Validation.Val(GetData));
                    }
                    if (VBStrings.Right(GetData, 1) == "D")
                    {
                        WkAngle = -Math.Abs(Validation.Val(GetData));
                    }
                    if (VBStrings.Right(GetData, 1) == "H")
                    {
                        WkAngle = 0;
                    }

                    break;

                case LR:

                    if (VBStrings.Right(GetData, 1) == "L")
                    {
                        WkAngle = -Math.Abs(Validation.Val(GetData));
                    }
                    if (VBStrings.Right(GetData, 1) == "R")
                    {
                        WkAngle = Math.Abs(Validation.Val(GetData));
                    }
                    if (VBStrings.Right(GetData, 1) == "V")
                    {
                        WkAngle = 0;
                    }

                    break;
            }

            return WkAngle;
        }

        /// <summary>
        /// CheckLimitAngle リミット値の判定
        /// </summary>
        /// <param name="GetData"></param>
        /// <param name="LimitMax"></param>
        /// <param name="LimitMin"></param>
        /// <returns></returns>
        public bool CheckLimitAngle(double GetData, double LimitMax, double LimitMin)
        {
            if (LimitMax < GetData) return false;
            if (LimitMin > GetData) return false;
            return true;
        }

    }

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     検証・エラーチェックをサポートした静的クラスです。
    /// </summary>
    /// -----------------------------------------------------------------------------

    public sealed class Validation
    {

        #region　Val メソッド (+2)

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     指定した文字列に含まれる数値を変換して返します。</summary>
        /// <param name="stTarget">
        ///     任意の有効な文字列。</param>
        /// <returns>
        ///     指定した文字列に含まれる数値。</returns>
        /// -----------------------------------------------------------------------------
        public static double Val(string stTarget)
        {
            // Null 値の場合は 0 を返す
            if (stTarget == null)
            {
                return 0D;
            }

            int iCurrent = 0;
            int iLength = stTarget.Length;

            // 評価対象外の文字をスキップする
            for (iCurrent = 0; iCurrent < iLength; iCurrent++)
            {
                char chOne = stTarget[iCurrent];

                if ((chOne != ' ') && (chOne != '　') && (chOne != '\t') && (chOne != '\n') && (chOne != '\r'))
                {
                    break;
                }
            }

            // 終端までに有効な文字が見つからなかった場合は 0 を返す
            if (iCurrent >= iLength)
            {
                return 0D;
            }

            bool bMinus = false;

            // 先頭にある符号を判定する
            switch (stTarget[iCurrent])
            {
                case '-':
                    bMinus = true;
                    iCurrent++;
                    break;
                case '+':
                    iCurrent++;
                    break;
            }

            int ValidLength = 0;
            int Priod = 0;
            double dReturn = 0D;
            bool bDecimal = false;
            bool bShisuMark = false;

            // 1 文字ずつ有効な文字かどうか判定する
            while (iCurrent < iLength)
            {
                char chCurrent = stTarget[iCurrent];

                if ((chCurrent == ' ') || (chCurrent == '　') || (chCurrent == '\t') || (chCurrent == '\n') || (chCurrent == '\r'))
                {
                    iCurrent++;
                }
                else if (chCurrent == '0')
                {
                    iCurrent++;

                    if ((ValidLength != 0) || bDecimal)
                    {
                        ValidLength++;
                        dReturn = (dReturn * 10) + double.Parse(chCurrent.ToString());
                    }
                }
                else if ((chCurrent >= '1') && (chCurrent <= '9'))
                {
                    iCurrent++;
                    ValidLength++;
                    dReturn = (dReturn * 10) + double.Parse(chCurrent.ToString());
                }
                else if (chCurrent == '.')
                {
                    iCurrent++;

                    if (bDecimal)
                    {
                        break;
                    }

                    bDecimal = true;
                    Priod = ValidLength;
                }
                else if ((chCurrent == 'e') || (chCurrent == 'E') || (chCurrent == 'd') || (chCurrent == 'D'))
                {
                    bShisuMark = true;
                    iCurrent++;
                    break;
                }
                else
                {
                    break;
                }
            }

            int iDecimal = 0;

            // 小数点が判定された場合
            if (bDecimal)
            {
                iDecimal = ValidLength - Priod;
            }

            // 指数が判定された場合
            if (bShisuMark)
            {
                bool bShisuValid = false;
                bool bShisuMinus = false;
                double dCoef = 0D;

                // 指数を検証する
                while (iCurrent < iLength)
                {
                    char chCurrent = stTarget[iCurrent];

                    if ((chCurrent == ' ') || (chCurrent == '　') || (chCurrent == '\t') || (chCurrent == '\n') || (chCurrent == '\r'))
                    {
                        iCurrent++;
                    }
                    else if ((chCurrent >= '0') && (chCurrent <= '9'))
                    {
                        dCoef = (dCoef * 10) + double.Parse(chCurrent.ToString());
                        iCurrent++;
                    }
                    else if (chCurrent == '+')
                    {
                        if (bShisuValid)
                        {
                            break;
                        }

                        bShisuValid = true;
                        iCurrent++;
                    }
                    else if ((chCurrent != '-') || bShisuValid)
                    {
                        break;
                    }
                    else
                    {
                        bShisuValid = true;
                        bShisuMinus = true;
                        iCurrent++;
                    }
                }

                // 指数の符号に応じて累乗する
                if (bShisuMinus)
                {
                    dCoef += iDecimal;
                    dReturn *= System.Math.Pow(10, -dCoef);
                }
                else
                {
                    dCoef -= iDecimal;
                    dReturn *= System.Math.Pow(10, dCoef);
                }
            }
            else if (bDecimal && (iDecimal != 0))
            {
                dReturn /= System.Math.Pow(10, iDecimal);
            }

            // 無限大の場合は 0 を返す
            if (double.IsInfinity(dReturn))
            {
                return 0D;
            }

            // マイナス判定の場合はマイナスで返す
            if (bMinus)
            {
                return -dReturn;
            }

            return dReturn;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     指定した文字に含まれる数値を変換して返します。</summary>
        /// <param name="chTarget">
        ///     任意の有効な文字。</param>
        /// <returns>
        ///     指定した文字に含まれる数値。</returns>
        /// -----------------------------------------------------------------------------
        public static int Val(char chTarget)
        {
            return (int)Val(chTarget.ToString());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     指定したオブジェクトに含まれる数値を変換して返します。</summary>
        /// <param name="oTarget">
        ///     任意の有効なオブジェクト。</param>
        /// <returns>
        ///     指定したオブジェクトに含まれる数値。</returns>
        /// -----------------------------------------------------------------------------
        public static double Val(object oTarget)
        {
            if (oTarget != null)
            {
                return Val(oTarget.ToString());
            }

            return 0D;
        }

        #endregion


        #region　IsNumeric メソッド (+1)

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     文字列が数値であるかどうかを返します。</summary>
        /// <param name="stTarget">
        ///     検査対象となる文字列。<param>
        /// <returns>
        ///     指定した文字列が数値であれば true。それ以外は false。</returns>
        /// -----------------------------------------------------------------------------
        public static bool IsNumeric(string stTarget)
        {
            double dNullable;

            return double.TryParse(
                stTarget,
                System.Globalization.NumberStyles.Any,
                null,
                out dNullable
            );
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     オブジェクトが数値であるかどうかを返します。</summary>
        /// <param name="oTarget">
        ///     検査対象となるオブジェクト。<param>
        /// <returns>
        ///     指定したオブジェクトが数値であれば true。それ以外は false。</returns>
        /// -----------------------------------------------------------------------------
        public static bool IsNumeric(object oTarget)
        {
            return IsNumeric(oTarget.ToString());
        }

        #endregion


    }

    public class ComboItem
    {
        public ComboItem(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }
        private string name;
        private int id;
        public override string ToString()
        {
            return string.Format("{0,2}:{1}", this.id, this.name);
        }
        public static ComboItem FromFormatted(string value)
        {
            if (value != null)
            {
                string[] strs = value.Split(':');
                int result;
                if (strs.Length == 2
                         && int.TryParse(strs[0], out result))
                {
                    return new ComboItem(result, strs[1]);
                }
            }
            return null;
        }
    }

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Microsoft.VisualBasic.Strings をカバーした静的クラスです。
    /// <summary>
    /// -----------------------------------------------------------------------------

    public class VBStrings
    {

        #region　Left メソッド

        /// -----------------------------------------------------------------------------------
        /// <summary>
        ///     文字列の左端から指定された文字数分の文字列を返します。</summary>
        /// <param name="stTarget">
        ///     取り出す元になる文字列。</param>
        /// <param name="iLength">
        ///     取り出す文字数。</param>
        /// <returns>
        ///     左端から指定された文字数分の文字列。
        ///     文字数を超えた場合は、文字列全体が返されます。</returns>
        /// -----------------------------------------------------------------------------------
        public static string Left(string stTarget, int iLength)
        {
            if (iLength <= stTarget.Length)
            {
                return stTarget.Substring(0, iLength);
            }

            return stTarget;
        }

        #endregion

        #region　Mid メソッド (+1)

        /// -----------------------------------------------------------------------------------
        /// <summary>
        ///     文字列の指定された位置以降のすべての文字列を返します。</summary>
        /// <param name="stTarget">
        ///     取り出す元になる文字列。</param>
        /// <param name="iStart">
        ///     取り出しを開始する位置。</param>
        /// <returns>
        ///     指定された位置以降のすべての文字列。</returns>
        /// -----------------------------------------------------------------------------------
        public static string Mid(string stTarget, int iStart)
        {
            if (iStart <= stTarget.Length)
            {
                return stTarget.Substring(iStart - 1);
            }

            return string.Empty;
        }

        /// -----------------------------------------------------------------------------------
        /// <summary>
        ///     文字列の指定された位置から、指定された文字数分の文字列を返します。</summary>
        /// <param name="stTarget">
        ///     取り出す元になる文字列。</param>
        /// <param name="iStart">
        ///     取り出しを開始する位置。</param>
        /// <param name="iLength">
        ///     取り出す文字数。</param>
        /// <returns>
        ///     指定された位置から指定された文字数分の文字列。
        ///     文字数を超えた場合は、指定された位置からすべての文字列が返されます。</returns>
        /// -----------------------------------------------------------------------------------
        public static string Mid(string stTarget, int iStart, int iLength)
        {
            if (iStart <= stTarget.Length)
            {
                if (iStart + iLength - 1 <= stTarget.Length)
                {
                    return stTarget.Substring(iStart - 1, iLength);
                }

                return stTarget.Substring(iStart - 1);
            }

            return string.Empty;
        }

        #endregion

        #region　Right メソッド (+1)

        /// -----------------------------------------------------------------------------------
        /// <summary>
        ///     文字列の右端から指定された文字数分の文字列を返します。</summary>
        /// <param name="stTarget">
        ///     取り出す元になる文字列。</param>
        /// <param name="iLength">
        ///     取り出す文字数。</param>
        /// <returns>
        ///     右端から指定された文字数分の文字列。
        ///     文字数を超えた場合は、文字列全体が返されます。</returns>
        /// -----------------------------------------------------------------------------------
        public static string Right(string stTarget, int iLength)
        {
            if (iLength <= stTarget.Length)
            {
                return stTarget.Substring(stTarget.Length - iLength);
            }

            return stTarget;
        }

        #endregion

    }


}
