using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankPayWSServer.BankPay.Models
{
    public class PABPay
    {
        // Token: 0x1700002E RID: 46
        // (get) Token: 0x0600037C RID: 892 RVA: 0x00048277 File Offset: 0x00046477
        // (set) Token: 0x0600037B RID: 891 RVA: 0x0004826E File Offset: 0x0004646E
        public string out_trade_no
        {
            get
            {
                return this._out_trade_no;
            }
            set
            {
                this._out_trade_no = value;
            }
        }

        // Token: 0x1700002F RID: 47
        // (get) Token: 0x0600037E RID: 894 RVA: 0x00048288 File Offset: 0x00046488
        // (set) Token: 0x0600037D RID: 893 RVA: 0x0004827F File Offset: 0x0004647F
        public string notify_url
        {
            get
            {
                return this._notify_url;
            }
            set
            {
                this._notify_url = value;
            }
        }

        // Token: 0x17000030 RID: 48
        // (get) Token: 0x06000380 RID: 896 RVA: 0x00048299 File Offset: 0x00046499
        // (set) Token: 0x0600037F RID: 895 RVA: 0x00048290 File Offset: 0x00046490
        public string scene
        {
            get
            {
                return this._scene;
            }
            set
            {
                this._scene = value;
            }
        }

        // Token: 0x17000031 RID: 49
        // (get) Token: 0x06000382 RID: 898 RVA: 0x000482AA File Offset: 0x000464AA
        // (set) Token: 0x06000381 RID: 897 RVA: 0x000482A1 File Offset: 0x000464A1
        public string auth_code
        {
            get
            {
                return this._auth_code;
            }
            set
            {
                this._auth_code = value;
            }
        }

        // Token: 0x17000032 RID: 50
        // (get) Token: 0x06000384 RID: 900 RVA: 0x000482BB File Offset: 0x000464BB
        // (set) Token: 0x06000383 RID: 899 RVA: 0x000482B2 File Offset: 0x000464B2
        public decimal total_amount
        {
            get
            {
                return this._total_amount;
            }
            set
            {
                this._total_amount = value;
            }
        }

        // Token: 0x17000033 RID: 51
        // (get) Token: 0x06000386 RID: 902 RVA: 0x000482CC File Offset: 0x000464CC
        // (set) Token: 0x06000385 RID: 901 RVA: 0x000482C3 File Offset: 0x000464C3
        public decimal undiscountable_amount
        {
            get
            {
                return this._undiscountable_amount;
            }
            set
            {
                this._undiscountable_amount = value;
            }
        }

        // Token: 0x17000034 RID: 52
        // (get) Token: 0x06000388 RID: 904 RVA: 0x000482DD File Offset: 0x000464DD
        // (set) Token: 0x06000387 RID: 903 RVA: 0x000482D4 File Offset: 0x000464D4
        public string subject
        {
            get
            {
                return this._subject;
            }
            set
            {
                this._subject = value;
            }
        }

        // Token: 0x17000035 RID: 53
        // (get) Token: 0x0600038A RID: 906 RVA: 0x000482EE File Offset: 0x000464EE
        // (set) Token: 0x06000389 RID: 905 RVA: 0x000482E5 File Offset: 0x000464E5
        public string body
        {
            get
            {
                return this._body;
            }
            set
            {
                this._body = value;
            }
        }

        // Token: 0x17000036 RID: 54
        // (get) Token: 0x0600038C RID: 908 RVA: 0x000482FF File Offset: 0x000464FF
        // (set) Token: 0x0600038B RID: 907 RVA: 0x000482F6 File Offset: 0x000464F6
        public decimal total_fee
        {
            get
            {
                return this._total_fee;
            }
            set
            {
                this._total_fee = value;
            }
        }

        // Token: 0x17000037 RID: 55
        // (get) Token: 0x0600038E RID: 910 RVA: 0x00048310 File Offset: 0x00046510
        // (set) Token: 0x0600038D RID: 909 RVA: 0x00048307 File Offset: 0x00046507
        public string spbill_create_ip
        {
            get
            {
                return this._spbill_create_ip;
            }
            set
            {
                this._spbill_create_ip = value;
            }
        }

        // Token: 0x17000038 RID: 56
        // (get) Token: 0x06000390 RID: 912 RVA: 0x00048321 File Offset: 0x00046521
        // (set) Token: 0x0600038F RID: 911 RVA: 0x00048318 File Offset: 0x00046518
        public string sub_appid
        {
            get
            {
                return this._sub_appid;
            }
            set
            {
                this._sub_appid = value;
            }
        }

        // Token: 0x17000039 RID: 57
        // (get) Token: 0x06000392 RID: 914 RVA: 0x00048332 File Offset: 0x00046532
        // (set) Token: 0x06000391 RID: 913 RVA: 0x00048329 File Offset: 0x00046529
        public string sub_openid
        {
            get
            {
                return this._sub_openid;
            }
            set
            {
                this._sub_openid = value;
            }
        }

        // Token: 0x1700003A RID: 58
        // (get) Token: 0x06000394 RID: 916 RVA: 0x00048343 File Offset: 0x00046543
        // (set) Token: 0x06000393 RID: 915 RVA: 0x0004833A File Offset: 0x0004653A
        public string sign
        {
            get
            {
                return this._sign;
            }
            set
            {
                this._sign = value;
            }
        }

        // Token: 0x1700003B RID: 59
        // (get) Token: 0x06000396 RID: 918 RVA: 0x00048354 File Offset: 0x00046554
        // (set) Token: 0x06000395 RID: 917 RVA: 0x0004834B File Offset: 0x0004654B
        public string workkey
        {
            get
            {
                return this._workkey;
            }
            set
            {
                this._workkey = value;
            }
        }

        // Token: 0x1700003C RID: 60
        // (get) Token: 0x06000398 RID: 920 RVA: 0x00048365 File Offset: 0x00046565
        // (set) Token: 0x06000397 RID: 919 RVA: 0x0004835C File Offset: 0x0004655C
        public int client
        {
            get
            {
                return this._client;
            }
            set
            {
                this._client = value;
            }
        }

        // Token: 0x1700003D RID: 61
        // (get) Token: 0x0600039A RID: 922 RVA: 0x00048376 File Offset: 0x00046576
        // (set) Token: 0x06000399 RID: 921 RVA: 0x0004836D File Offset: 0x0004656D
        public float version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }

        // Token: 0x1700003E RID: 62
        // (get) Token: 0x0600039C RID: 924 RVA: 0x00048387 File Offset: 0x00046587
        // (set) Token: 0x0600039B RID: 923 RVA: 0x0004837E File Offset: 0x0004657E
        public int cashid
        {
            get
            {
                return this._cashid;
            }
            set
            {
                this._cashid = value;
            }
        }

        // Token: 0x040004D5 RID: 1237
        private string _out_trade_no = string.Empty;

        // Token: 0x040004D6 RID: 1238
        private string _notify_url = string.Empty;

        // Token: 0x040004D7 RID: 1239
        private string _scene = string.Empty;

        // Token: 0x040004D8 RID: 1240
        private string _auth_code = string.Empty;

        // Token: 0x040004D9 RID: 1241
        private decimal _total_amount;

        // Token: 0x040004DA RID: 1242
        private decimal _undiscountable_amount;

        // Token: 0x040004DB RID: 1243
        private string _subject = string.Empty;

        // Token: 0x040004DC RID: 1244
        private string _body = string.Empty;

        // Token: 0x040004DD RID: 1245
        private decimal _total_fee;

        // Token: 0x040004DE RID: 1246
        private string _spbill_create_ip = string.Empty;

        // Token: 0x040004DF RID: 1247
        private string _sub_appid = string.Empty;

        // Token: 0x040004E0 RID: 1248
        private string _sub_openid = string.Empty;

        // Token: 0x040004E1 RID: 1249
        private string _sign = string.Empty;

        // Token: 0x040004E2 RID: 1250
        private string _workkey = string.Empty;

        // Token: 0x040004E3 RID: 1251
        private int _client;

        // Token: 0x040004E4 RID: 1252
        private float _version;

        // Token: 0x040004E5 RID: 1253
        private int _cashid;
    }
}
