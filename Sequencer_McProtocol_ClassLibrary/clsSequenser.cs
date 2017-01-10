using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SystemControl;
using System.Net.Sockets;
using System.Text;

namespace Sequencer_McProtocol_ClassLibrary
{
    public class ClassMcProtocol
    {
        #region 変数定義
        // TCPｸﾗｲｱﾝﾄ
        private TcpClient client;
        // 通信ストリームの取得
        private NetworkStream stream;
        //エンコーディング形式
        private Encoding enc = Encoding.ASCII;

        private string[,] StateArray = null;

        #endregion

        #region 変数定義(列挙体)
        private enum BIT_SET : int { ON = 1, OFF = 0 };
        private enum ANGLE : int { UD = 1, LR = 0 };
        private enum LIMIT_STATE : int { UDHOME = 0, UDSLimit_P = 6, UDSLimit_M = 8, LRHOME = 10, LRSLimit_P = 16, LRSLimit_M = 18 };
        private enum ERR_DM : int { UD_Err = 0, UD_Wer = 1, LR_Err = 2, LR_Wer = 3 };
        private enum STATE_DM : int { DM30 = 0, DM31 = 1, DM32 = 2, DM33 = 3 };
        private enum GRID_COL : int { DM = 0, BIT = 1, NOTE = 2, VALUE = 3 }
        #endregion

        #region 定数定義
        //MCプロトコル QnA互換3Eフレーム
        private const string NETWORK_NO = "00";
        private const string PC_NO = "FF";
        private const string UNIT_IO = "FF03";
        private const string UNIT_NO = "00";
        private const string HEDER = "5000";
        private const string DEVICECODE_F = "B0";
        private const string DEVICECODE_D = "A8";
        private const string READ_COMMAND = "100001040000";
        private const string SEND_COMMAND = "100001140000";
        #endregion

        #region クラス定義
        private ConvertDispName CNV = new ConvertDispName();
        #endregion

        #region 接続処理
        public bool Connection(string SequenceIP, int SequencePort)
        {

            try
            {
                // TCP/IP接続を行う
                client = new TcpClient();
                client.Connect(System.Net.IPAddress.Parse(SequenceIP), SequencePort);

                stream = client.GetStream();

                //読み取り、書き込みのタイムアウトを10秒にする
                //デフォルトはInfiniteで、タイムアウトしない
                //(.NET Framework 2.0以上が必要)
                stream.ReadTimeout = 10000;
                stream.WriteTimeout = 10000;

                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                GC.Collect();
            }

        }
        #endregion

        #region 切断処理
        public void disConnect()
        {
            //閉じる
            stream.Close();
            client.Close();
        }

        #endregion

        #region 回転台位置読取
        public bool DgrGet(ref double prUD, ref double prLR)
        {
            bool Ret = true;

            try
            {

                #region U/D座標 L/R座標読取(DM2012～2034)

                Ret = Sequencer_Send((int)clsDefine.ConectReadMode.TurnRead);

                //ウェイト(msec)
                System.Threading.Thread.Sleep(10);

                Ret = Sequencer_Position_Read(ref prUD, ref prLR);

                #endregion

                return Ret;
            }
            catch
            { return false; }
            finally
            { GC.Collect(); }
        }


        #endregion

        #region 回転台位置指定
        public bool DegSet(double prUD, double prLR)
        {
            #region シーケンサー送信


            try
            {
                clsDefine.MOVE_FIG = true;

                #region リモート指定

                Sequencer_Send((int)clsDefine.ConectReadMode.REMOTE);

                #endregion


                #region 角度移動


                #region メモリ設定(L/R)

                //メモリの開始番号
                int DM_St = 20;
                int DM_En = 24;


                string DM_St_Hex = DM_St.ToString("X2").PadLeft(6, '0');
                string DM_En_Hex = DM_En.ToString("X2").PadLeft(6, '0');


                int DeviceNo = (DM_En - DM_St) + 1;

                string DeviceHex = ConvMCFormat(DeviceNo.ToString("X2").PadLeft(4, '0'), false);
                int CalcByte = ((DM_En - DM_St) + 1) * 4 + 2;

                //先頭デバイス
                string StDeviceHex = ConvMCFormat(DM_St_Hex, false);

                string strValue = StDeviceHex + DEVICECODE_D + DeviceHex;


                #endregion

                #region コマンド生成

                int[] intSendArray = new int[2];
                StringBuilder sbSender = new StringBuilder();
                string StrSendWrite = string.Empty;

                sbSender.Clear();
                intSendArray[1] = (int)(CNV.ConvDeg(prLR.ToString("#0.00"), (int)ANGLE.LR) * 100);


                //16進数変換
                //sbSender.Append(ConvMCFormat(intSendArray[0].ToString("X").PadLeft(8, '0'), false));
                sbSender.Append(ConvMCFormat(intSendArray[1].ToString("X").PadLeft(8, '0'), false));

                if (intSendArray[1] < 0)
                {
                    sbSender.Append("FFFFFFFF");
                }



                string SendCommnd = HEDER + NETWORK_NO + PC_NO + UNIT_IO + UNIT_NO
                      + ConvMCFormat(CalcByte.ToString("X2").PadLeft(4, '0'), false) + SEND_COMMAND + strValue
                      + sbSender.ToString();

                #endregion

                #region コマンド送信

                //16進数文字列を2文字づつ区切って数値配列に設定する
                int SendLength = SendCommnd.Length / 2;

                char[] Sendchar = new char[SendCommnd.Length + 2];
                byte[] SendBuffer = new byte[SendCommnd.Length + 2];
                int[] IntArray = new int[SendCommnd.Length + 2];
                int jj = 0;

                for (int ii = 0; ii < SendCommnd.Length; ii += 2)
                {
                    if (SendCommnd.Length > ii + 2)
                    {
                        IntArray[jj] = Convert.ToInt32(SendCommnd.Substring(ii, 2), 16);
                        SendBuffer[jj] = (byte)IntArray[jj];
                        jj++;
                    }
                }


                stream.Write(SendBuffer, 0, SendBuffer.Length);
                stream.Flush(); // フラッシュ(強制書き出し)


                #endregion

                #region ウェイト(msec)
                System.Threading.Thread.Sleep(10);
                #endregion

                #region メモリ設定(L/R)

                //メモリの開始番号
                DM_St = 10;
                DM_En = 14;


                DM_St_Hex = DM_St.ToString("X2").PadLeft(6, '0');
                DM_En_Hex = DM_En.ToString("X2").PadLeft(6, '0');


                DeviceNo = (DM_En - DM_St) + 1;

                DeviceHex = ConvMCFormat(DeviceNo.ToString("X2").PadLeft(4, '0'), false);
                CalcByte = ((DM_En - DM_St) + 1) * 4 + 2;

                //先頭デバイス
                StDeviceHex = ConvMCFormat(DM_St_Hex, false);

                strValue = StDeviceHex + DEVICECODE_D + DeviceHex;


                #endregion

                #region コマンド生成

                intSendArray = new int[2];
                sbSender = new StringBuilder();
                StrSendWrite = string.Empty;

                sbSender.Clear();

                intSendArray[0] = (int)(CNV.ConvDeg(prUD.ToString("#0.00"), (int)ANGLE.UD) * 100);

                //16進数変換
                sbSender.Append(ConvMCFormat(intSendArray[0].ToString("X").PadLeft(8, '0'), false));

                if (intSendArray[0] < 0)
                {
                    sbSender.Append("FFFFFFFF");
                }

                SendCommnd = HEDER + NETWORK_NO + PC_NO + UNIT_IO + UNIT_NO
                     + ConvMCFormat(CalcByte.ToString("X2").PadLeft(4, '0'), false) + SEND_COMMAND + strValue
                     + sbSender.ToString();

                #endregion

                #region コマンド送信

                //16進数文字列を2文字づつ区切って数値配列に設定する
                SendLength = SendCommnd.Length / 2;

                Sendchar = new char[SendCommnd.Length + 2];
                SendBuffer = new byte[SendCommnd.Length + 2];
                IntArray = new int[SendCommnd.Length + 2];
                jj = 0;

                for (int ii = 0; ii < SendCommnd.Length; ii += 2)
                {
                    if (SendCommnd.Length > ii + 2)
                    {
                        IntArray[jj] = Convert.ToInt32(SendCommnd.Substring(ii, 2), 16);
                        SendBuffer[jj] = (byte)IntArray[jj];
                        jj++;
                    }
                }


                stream.Write(SendBuffer, 0, SendBuffer.Length);
                stream.Flush(); // フラッシュ(強制書き出し)


                #endregion

                #region 移動フラグON

                Sequencer_Send((int)clsDefine.ConectReadMode.Move);

                #endregion

                #region ステータス読取(DM31～33)

                while (clsDefine.MOVE_FIG)
                {

                    Sequencer_Send((int)clsDefine.ConectReadMode.State);

                    //ウェイト(msec)
                    System.Threading.Thread.Sleep(10);

                    Sequencer_State_Read(ref StateArray);
                }

                #endregion


                //ウェイト(msec)
                System.Threading.Thread.Sleep(10);
                Sequencer_Send((int)clsDefine.ConectReadMode.AllReset);

                #region ローカル指定

                //rdobtnLocal.Checked = true;

                #endregion

                #endregion


                return true; 

            }
            catch (Exception ex)
            {
                return false; 
            }
            finally
            {
                GC.Collect();
            }

            #endregion
        }
        #endregion

        #region リモート
        public bool Remote_Set()
        {
            return Sequencer_Send((int)clsDefine.ConectReadMode.REMOTE);
        }
        #endregion

        #region ローカル
        public bool Local_Set()
        {
            return Sequencer_Send((int)clsDefine.ConectReadMode.LOCAL);
        }
        #endregion

        #region 非常停止
        public bool Emergency_Stop()
        {
            return  Sequencer_Send((int)clsDefine.ConectReadMode.Stop);
        }
        #endregion

        #region 警告リセット
        public bool Alarm_Reset()
        {
            bool Ret = true;

            Ret = Sequencer_Send((int)clsDefine.ConectReadMode.ErrResetOn);
            //ウェイト(msec)
            System.Threading.Thread.Sleep(10);
            Ret = Sequencer_Send((int)clsDefine.ConectReadMode.AllReset);

            return Ret;
        }
        #endregion

        #region ブザー停止
        public bool Beep_Off()
        {
            bool Ret = true;

            Ret = Sequencer_Send((int)clsDefine.ConectReadMode.BepOff);
            //ウェイト(msec)
            System.Threading.Thread.Sleep(10);
            Ret = Sequencer_Send((int)clsDefine.ConectReadMode.AllReset);

            return Ret;
        }
        #endregion

        #region U/D　L/R　ホームポジション読取(DM1010～1029)
        public void Read_HomePosition(ref string[] strLimit_Array)
        {
            Sequencer_Send((int)clsDefine.ConectReadMode.OffSet);
            //ウェイト(msec)
            System.Threading.Thread.Sleep(10);
            Sequencer_Home_Read(ref strLimit_Array);
        }

        #endregion

        #region 原点復帰
        public bool ORG()
        {
            bool Ret = true;

            #region リモート指定

            Ret = Sequencer_Send((int)clsDefine.ConectReadMode.REMOTE);

            #endregion

            //ウェイト(msec)
            System.Threading.Thread.Sleep(10);

            Ret = Sequencer_Send((int)clsDefine.ConectReadMode.ORG);

            return Ret; 
        }

        #endregion

        #region バイト列を16進数表記の文字列に変換
        private static string BytesToHexString(byte[] bytes, int GetLength)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < GetLength; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion

        #region MCプロトコル用バイナリー変換前文字列操作
        /// <summary>
        /// ConvMCFormat（MCプロトコル用バイナリー変換前文字列操作）
        /// </summary>
        /// <param name="prCommand">送信コマンド</param>
        /// <returns>送信コマンド上位・中位・下位変換</returns>
        private string ConvMCFormat(string prCommand, bool ReciveMode)
        {

            string RetFormat = string.Empty;

            try
            {

                switch (prCommand.Length)
                {
                    case 4:

                        RetFormat = prCommand.Substring(2, 2) + prCommand.Substring(0, 2);
                        break;

                    case 6:

                        if (ReciveMode)
                        {
                            if (prCommand.Substring(0, 2) == "00" && prCommand.Substring(2, 2) == "00")
                            {
                                RetFormat = prCommand.Substring(4, 2);
                            }
                            else if (prCommand.Substring(0, 2) == "00")
                            {
                                RetFormat = prCommand.Substring(4, 2) + prCommand.Substring(2, 2);
                            }
                            else
                            {
                                RetFormat = prCommand.Substring(4, 2) + prCommand.Substring(2, 2) + prCommand.Substring(0, 2);
                            }
                        }
                        else
                        {
                            RetFormat = prCommand.Substring(4, 2) + prCommand.Substring(2, 2) + prCommand.Substring(0, 2);
                        }

                        //終了コードの確認

                        if (prCommand.Substring(4, 2) == "50" && prCommand.Substring(2, 2) == "80")
                        {
                            RetFormat = prCommand.Substring(0, 2);
                        }
                        break;

                    case 8:

                        if (ReciveMode)
                        {
                            if (prCommand.Substring(0, 2) == "00" && prCommand.Substring(2, 2) == "00")
                            {
                                RetFormat = prCommand.Substring(4, 2);
                            }
                            else if (prCommand.Substring(0, 2) == "00")
                            {
                                RetFormat = prCommand.Substring(4, 2) + prCommand.Substring(2, 2);
                            }
                            else
                            {
                                RetFormat = prCommand.Substring(6, 2) + prCommand.Substring(4, 2) + prCommand.Substring(2, 2) + prCommand.Substring(0, 2);
                            }
                        }
                        else
                        {

                            RetFormat = prCommand.Substring(6, 2) + prCommand.Substring(4, 2) + prCommand.Substring(2, 2) + prCommand.Substring(0, 2);

                        }

                        //終了コードの確認

                        if (prCommand.Substring(6, 2) == "50" && prCommand.Substring(4, 2) == "80")
                        {
                            RetFormat = prCommand.Substring(0, 2);
                        }

                        break;

                    default:
                        RetFormat = prCommand;
                        break;
                }



                return RetFormat;
            }
            catch
            {
                return RetFormat;
            }
            finally
            {
            }
        }
        #endregion

        #region シーケンサーコマンド送信
        public bool Sequencer_Send(int Mode)
        {
            try
            {

                //--------------
                // サーバーへ送信
                #region コマンド送信

                string SendCommnd = clsDefine.SendCmmand[Mode];

                //16進数文字列を2文字づつ区切って数値配列に設定する
                int SendLength = SendCommnd.Length / 2;

                char[] Sendchar = new char[1024];
                byte[] SendBuffer = new byte[1024];
                int[] IntArray = new int[1024];
                int jj = 0;

                for (int ii = 0; ii < SendCommnd.Length; ii += 2)
                {
                    if (SendCommnd.Length > ii + 2)
                    {
                        IntArray[jj] = Convert.ToInt32(SendCommnd.Substring(ii, 2), 16);
                        SendBuffer[jj] = (byte)IntArray[jj];
                        jj++;
                    }
                }
                stream.Write(SendBuffer, 0, SendBuffer.Length);
                stream.Flush(); // フラッシュ(強制書き出し)
                return true; 

            }
            catch
            {
                return false; 
            }
            finally
            { GC.Collect(); } 

            #endregion
        }
        #endregion

        #region シーケンサーコマンド受信
        private bool Sequencer_Home_Read(ref string[] RET_LIMIT_STATE)
        {
            try
            {

                #region 受信処理

                RET_LIMIT_STATE = new string[6];

                string strReceivedData = string.Empty;
                string ArrayData = string.Empty;

                //リモートホストからの返信を受信します。
                do
                {
                    if (stream.DataAvailable)
                    {
                        byte[] bytReceiveBuffer = new byte[255];
                        int intDataLength = stream.Read(bytReceiveBuffer, 0, bytReceiveBuffer.Length);
                        strReceivedData = BytesToHexString(bytReceiveBuffer, intDataLength);
                        ArrayData += strReceivedData;
                    }
                    else if (strReceivedData != null)
                    {
                        break;
                    }
                } while (true);

                if (ArrayData.Length > 10)
                {

                    //文字列分解
                    string GetRecValue = ArrayData.Substring(22, ArrayData.Length - 22);


                    if (GetRecValue.Length > 10)
                    {
                        int jj = 0;
                        int num = 0;
                        int BitIndex = 4;
                        int[] BitGetLength = new int[] { 2, 2 };
                        int[] DispDecimal = new int[] { 100, 100 };
                        //16進数を10進数変換　バイナリ上位下位の入替
                        //48bit(QWORD)6バイト
                        string[] QWORD_Array = new string[GetRecValue.Length / BitIndex];
                        decimal[] DispValue = new decimal[2];

                        for (int ii = 0; ii < GetRecValue.Length / BitIndex; ii++)
                        {

                            QWORD_Array[ii] = GetRecValue.Substring(jj * BitIndex, BitIndex);
                            jj++;

                            if (QWORD_Array[ii].Length == BitIndex)
                            {
                                int StrLength = 0;

                                //文字列を2つに分解
                                string[] M_DATA = new string[2];
                                for (int kk = 0; kk < 2; kk++)
                                {
                                    M_DATA[kk] = ConvMCFormat(QWORD_Array[ii].Substring(StrLength, BitGetLength[kk]), true);
                                    StrLength += BitGetLength[kk];
                                }

                                // Convertクラスを利用
                                num = Convert.ToInt32(M_DATA[1] + M_DATA[0], 16);

                                string hexStr = M_DATA[1] + M_DATA[0];

                                string Bittext = Validation.Val(Convert.ToString(Convert.ToInt32(hexStr, 16), 2)).ToString("0000000000000000");

                                if (Bittext.Substring(0, 1) == "1")
                                {
                                    num -= 65536;
                                }

                                string txtFormat = "##0.00";

                                switch (ii)
                                {
                                    case (int)LIMIT_STATE.UDHOME:
                                        //U/D
                                        RET_LIMIT_STATE[0] = ((decimal)num / DispDecimal[0]).ToString(txtFormat);
                                        break;
                                    case (int)LIMIT_STATE.UDSLimit_P:
                                        //U/D
                                        RET_LIMIT_STATE[1] = ((decimal)num / DispDecimal[0]).ToString(txtFormat);
                                        break;
                                    case (int)LIMIT_STATE.UDSLimit_M:
                                        //U/D
                                        RET_LIMIT_STATE[2] = ((decimal)num / DispDecimal[0]).ToString(txtFormat);
                                        break;
                                    case (int)LIMIT_STATE.LRHOME:
                                        //L/R
                                        RET_LIMIT_STATE[3] = ((decimal)num / DispDecimal[0]).ToString(txtFormat);
                                        break;

                                    case (int)LIMIT_STATE.LRSLimit_M:
                                        //L/R
                                        RET_LIMIT_STATE[4] = ((decimal)num / DispDecimal[0]).ToString(txtFormat);
                                        break;

                                    case (int)LIMIT_STATE.LRSLimit_P:
                                        //L/R
                                        RET_LIMIT_STATE[5] = ((decimal)num / DispDecimal[0]).ToString(txtFormat);
                                        break;
                                }

                            }
                        }

                    }
                }
                return true;
                #endregion
            }
            catch
            { return false; }
            finally
            { GC.Collect();}
        }
        private bool Sequencer_State_Read(ref string[,] retState)
        {

            try
            {
                #region 受信処理

                string strReceivedData = string.Empty;
                string ArrayData = string.Empty;

                retState = new string[3, 20];

                //リモートホストからの返信を受信します。
                do
                {
                    if (stream.DataAvailable)
                    {
                        byte[] bytReceiveBuffer = new byte[255];
                        int intDataLength = stream.Read(bytReceiveBuffer, 0, bytReceiveBuffer.Length);
                        strReceivedData = BytesToHexString(bytReceiveBuffer, intDataLength);
                        ArrayData += strReceivedData;
                    }
                    else if (strReceivedData != null)
                    {
                        break;
                    }
                } while (true);

                if (ArrayData.Length > 10)
                {

                    //文字列分解
                    string GetRecValue = ArrayData.Substring(22, ArrayData.Length - 22);


                    if (GetRecValue.Length > 10)
                    {
                        int jj = 0;
                        int BitCnt = 0;
                        int num = 0;
                        int BitIndex = 4;
                        int GridIndex = 0;
                        int[] BitGetLength = new int[] { 2, 2 };
                        int[] DispDecimal = new int[] { 100, 100 };
                        //16進数を10進数変換　バイナリ上位下位の入替
                        //48bit(QWORD)6バイト
                        string[] QWORD_Array = new string[GetRecValue.Length / BitIndex];
                        decimal[] DispValue = new decimal[2];


                        for (int ii = 0; ii < GetRecValue.Length / BitIndex; ii++)
                        {

                            QWORD_Array[ii] = GetRecValue.Substring(jj * BitIndex, BitIndex);
                            jj++;

                            if (QWORD_Array[ii].Length == BitIndex)
                            {
                                int StrLength = 0;

                                //文字列を2つに分解
                                string[] M_DATA = new string[2];
                                for (int kk = 0; kk < 2; kk++)
                                {
                                    M_DATA[kk] = ConvMCFormat(QWORD_Array[ii].Substring(StrLength, BitGetLength[kk]), true);
                                    StrLength += BitGetLength[kk];
                                }


                                // Convertクラスを利用
                                num = Convert.ToInt32(M_DATA[1] + M_DATA[0], 16);

                                string hexStr = M_DATA[1] + M_DATA[0];

                                string Bittext = Validation.Val(Convert.ToString(Convert.ToInt32(hexStr, 16), 2)).ToString("0000000000000000");

                                if (Bittext.Substring(0, 1) == "1")
                                {
                                    num -= 65536;
                                }


                                // stTarget を Char 型の 1 次元配列に変換する
                                char[] chArray = Bittext.ToCharArray();

                                switch (ii)
                                {
                                    case (int)STATE_DM.DM30:

                                        BitCnt = 0;
                                        for (int kk = chArray.Length - 1; kk > -1; kk--)
                                        {
                                            if (BitCnt != 3 && BitCnt != 5)
                                            {
                                                retState[0, GridIndex] = "DM30";
                                                retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:警報" : "OFF:正常";
                                                GridIndex++;
                                            }
                                            BitCnt++;
                                        }
                                        break;


                                    case (int)STATE_DM.DM31:


                                        BitCnt = 0;
                                        for (int kk = chArray.Length - 1; kk > -1; kk--)
                                        {
                                            if (BitCnt < 2)
                                            {
                                                retState[0, GridIndex] = "DM31";
                                                retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:警報" : "OFF:正常";
                                                GridIndex++;
                                            }
                                            BitCnt++;
                                        }
                                        break;


                                    case (int)STATE_DM.DM32:


                                        BitCnt = 0;
                                        for (int kk = chArray.Length - 1; kk > -1; kk--)
                                        {

                                            switch (BitCnt)
                                            {
                                                case 0:

                                                    retState[0, GridIndex] = "DM32";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:警報" : "OFF:正常";
                                                    GridIndex++;
                                                    break;

                                                case 1:

                                                    retState[0, GridIndex] = "DM32";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:ローカル" : "OFF:リモート";
                                                    GridIndex++;
                                                    break;

                                                case 2:

                                                    retState[0, GridIndex] = "DM32";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:正常" : "OFF:非常停止";
                                                    GridIndex++;
                                                    break;

                                                case 7://動作中フラグ取得

                                                    clsDefine.MOVE_FIG = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? false : true;
                                                    retState[0, GridIndex] = "DM32";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:停止中" : "OFF:動作中";
                                                    GridIndex++;
                                                    break;

                                                case 3:
                                                case 4:
                                                case 5:
                                                case 6:
                                                case 8:
                                                case 9:
                                                case 10:
                                                case 11:
                                                case 12:
                                                case 13:
                                                case 14:
                                                case 15:

                                                    retState[0, GridIndex] = "DM32";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:正常" : "OFF:異常";
                                                    GridIndex++;
                                                    break;

                                            }
                                            BitCnt++;
                                        }
                                        break;

                                    case (int)STATE_DM.DM33:


                                        BitCnt = 0;
                                        for (int kk = chArray.Length - 1; kk > -1; kk--)
                                        {

                                            switch (BitCnt)
                                            {
                                                case 0:
                                                    retState[0, GridIndex] = "DM33";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:実行中" : "OFF:停止";
                                                    GridIndex++;
                                                    break;

                                                case 1:
                                                    retState[0, GridIndex] = "DM33";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:実施済" : "OFF:未実施";
                                                    GridIndex++;
                                                    break;

                                                case 2:
                                                case 4:
                                                    retState[0, GridIndex] = "DM33";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:正常" : "OFF:停止中";
                                                    GridIndex++;
                                                    break;

                                                case 3:
                                                    retState[0, GridIndex] = "DM33";
                                                    retState[1, GridIndex] = "bit" + BitCnt.ToString();
                                                    retState[2, GridIndex] = (int)Validation.Val(chArray[kk]) == (int)BIT_SET.ON ? "ON:完了" : "OFF:未完了";
                                                    GridIndex++;
                                                    break;

                                            }
                                            BitCnt++;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                return true;
                #endregion
            }
            catch
            {
                return false; 
            }
            finally
            { GC.Collect(); }
        }
        private bool Sequencer_Position_Read(ref double retUD, ref double retLR)
        {
            try
            {

                #region 受信処理

                string strReceivedData = string.Empty;
                string ArrayData = string.Empty;

                //リモートホストからの返信を受信します。
                do
                {
                    if (stream.DataAvailable)
                    {
                        byte[] bytReceiveBuffer = new byte[255];
                        int intDataLength = stream.Read(bytReceiveBuffer, 0, bytReceiveBuffer.Length);
                        strReceivedData = BytesToHexString(bytReceiveBuffer, intDataLength);
                        ArrayData += strReceivedData;
                    }
                    else if (strReceivedData != null)
                    {
                        break;
                    }
                } while (true);

                if (ArrayData.Length > 10)
                {

                    //文字列分解
                    string GetRecValue = ArrayData.Substring(22, ArrayData.Length - 22);


                    if (GetRecValue.Length > 10)
                    {
                        int jj = 0;
                        int num = 0;
                        int BitIndex = 4;
                        int[] BitGetLength = new int[] { 2, 2 };
                        int[] DispDecimal = new int[] { 100, 100 };
                        //16進数を10進数変換　バイナリ上位下位の入替
                        //48bit(QWORD)6バイト
                        string[] QWORD_Array = new string[GetRecValue.Length / BitIndex];
                        decimal[] DispValue = new decimal[2];


                        for (int ii = 0; ii < GetRecValue.Length / BitIndex; ii++)
                        {

                            QWORD_Array[ii] = GetRecValue.Substring(jj * BitIndex, BitIndex);
                            jj++;

                            if (QWORD_Array[ii].Length == BitIndex)
                            {
                                int StrLength = 0;

                                //文字列を2つに分解
                                string[] M_DATA = new string[2];
                                for (int kk = 0; kk < 2; kk++)
                                {
                                    M_DATA[kk] = ConvMCFormat(QWORD_Array[ii].Substring(StrLength, BitGetLength[kk]), true);
                                    StrLength += BitGetLength[kk];
                                }


                                // Convertクラスを利用
                                num = Convert.ToInt32(M_DATA[1] + M_DATA[0], 16);

                                string hexStr = M_DATA[1] + M_DATA[0];

                                string Bittext = Validation.Val(Convert.ToString(Convert.ToInt32(hexStr, 16), 2)).ToString("0000000000000000");

                                if (Bittext.Substring(0, 1) == "1")
                                {
                                    num -= 65536;
                                }

                                switch (ii)
                                {
                                    case 0:
                                        //U/D
                                        DispValue[(int)ANGLE.UD] = ((decimal)num / DispDecimal[0]);
                                        retUD = (double)DispValue[(int)ANGLE.UD];
                                        break;

                                    case 10:
                                        //L/R
                                        DispValue[(int)ANGLE.LR] = ((decimal)num / DispDecimal[0]);
                                        retLR = (double)DispValue[(int)ANGLE.LR];
                                        break;
                                }
                            }
                        }
                    }
                }
                return true; 
                #endregion

            }
            catch
            { return false; }
            finally
            { GC.Collect(); }
        }
        #endregion
    }
    
}

