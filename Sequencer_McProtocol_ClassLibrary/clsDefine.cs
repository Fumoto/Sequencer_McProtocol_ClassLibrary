using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sequencer_McProtocol_ClassLibrary
{
    class clsDefine
    {

        #region システム定数設定

        public readonly string SequenceIP = "192.168.0.10";
        public readonly int SequencePort = 5000;

        public const string SysFontName = "メイリオ";
        public const float SysFontSizeL = 11F;
        public const float SysFontSizeM = 9.5F;
        public const float SysFontSizeS = 8F;

        public const int PointMax = 2521;//2101;

        #endregion

        #region "変数定義(列挙体)"
        public enum ConectReadMode : int { TurnRead = 0, OffSet = 1, State = 2, ErrNo = 3, LOCAL = 4, REMOTE = 5, ORG = 6, ErrResetOn = 7, BepOff = 8, AllReset = 9, Move = 10, Stop = 11 };
        #endregion

        #region システム変数定義


        public static string[] SendCmmand = new string[] { "500000FFFF03005E00100001040000DC0700A81700", "500000FFFF03005200100001040000F20300A81400", "500000FFFF030016001000010400001E0000A80400", "500000FFFF03003200100001040000640000A80400",
                                                           "500000FFFF03000E00100001140000220000A8030001000000","500000FFFF03000E00100001140000220000A8030002000000","500000FFFF03000E00100001140000220000A8030006000000",
                                                           "500000FFFF03000E00100001140000220000A8030008000000","500000FFFF03000E00100001140000220000A8030010000000","500000FFFF03000E00100001140000220000A8030000000000",
                                                           "500000FFFF03000E00100001140000220000A8030000010000","500000FFFF03000E00100001140000220000A8030000020000"};

        public static string[] SyateText = new string[] { "非常停止中", "ﾏｯﾄsw異常", "安全柵異常", "U/D ｻｰﾎﾞﾄﾞﾗｲﾊﾞｰ異常", "R/L ｻｰﾎﾞﾄﾞﾗｲﾊﾞｰ異常", "Z軸ｲﾝﾊﾞｰﾀｰ異常", "U/D +ﾘﾐｯﾄｾﾝｻ異常", "U/D -ﾘﾐｯﾄｾﾝｻ異常", "U/D +ｿﾌﾄﾘﾐｯﾄ異常", "U/D -ｿﾌﾄﾘﾐｯﾄ異常", "U/D ﾓｰｼｮﾝｺﾝﾄﾛｰﾗ異常", "R/L +ﾘﾐｯﾄｾﾝｻ異常", "R/L -ﾘﾐｯﾄｾﾝｻ異常", "R/L +ｿﾌﾄﾘﾐｯﾄ異常", "R/L -ｿﾌﾄﾘﾐｯﾄ異常"
                                                   ,"R/L ﾓｰｼｮﾝｺﾝﾄﾛｰﾗ異常","総合警報ﾌﾗｸﾞ","REMOTE/LOCALﾌﾗｸﾞ","*非常停止","*ﾏｯﾄsw","*安全柵","U/D_ｻｰﾎﾞﾚﾃﾞｨ","R/L_ｻｰﾎﾞﾚﾃﾞｨ","動作中","*U/D_ｿﾌﾄﾘﾐｯﾄ+","*U/D_ｿﾌﾄﾘﾐｯﾄ-","*R/L_ｿﾌｯﾄﾘﾐｯﾄ+","*R/L_ｿﾌｯﾄﾘﾐｯﾄ-","*U/D_ﾘﾐｯﾄｾﾝｻ+","*U/D_ﾘﾐｯﾄｾﾝｻ-","*R/L_ﾘﾐｯﾄｾﾝｻ+","*R/L_ﾘﾐｯﾄｾﾝｻ-","原点復帰中","原点復帰済みﾌﾗｸﾞ","強制停止中","位置決め完了","ﾌﾞｻﾞｰ停止中"};

        public static string[] DM_Text = new string[] { "DM30", "DM31", "DM32", "DM33" };

        public static bool MOVE_FIG = true;


        public static string[] HederListItemNo = null;  //ヘッダーリスト番号
        public static string[] HederListItemName = null;//ヘッダーリスト名称
        public static string[] HederFileName = null; 　 //ヘッダーリストファイル名称

        public static int SelItemNo = 0; //ヘッダーリストのインデックス
        public static int ListMax = 0;

        public static string HeaderSelNo = "1";
        public static string HeaderItemNo = string.Empty;
        public static string HeaderItemName = string.Empty;
        public static string HeaderLinkPath = string.Empty;
        public static decimal HeaderOffsetX = 0;
        public static decimal HeaderOffsetY = 0;

        public static decimal[] PointX = new decimal[PointMax];//0：オフセット値 1～2100
        public static decimal[] PointY = new decimal[PointMax];//0：オフセット値 1～2100
        public static int[] PointSpeed = new int[PointMax];//デフォルト10

        #endregion
    }
}
