using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankPayWSServer.BankPay.Models
{
    public class CasherInfo
    {
        // Token: 0x17000021 RID: 33
        // (get) Token: 0x06000361 RID: 865 RVA: 0x00048124 File Offset: 0x00046324
        // (set) Token: 0x06000360 RID: 864 RVA: 0x0004811B File Offset: 0x0004631B
        public string username
        {
            get
            {
                return this._username;
            }
            set
            {
                this._username = value;
            }
        }

        // Token: 0x17000022 RID: 34
        // (get) Token: 0x06000363 RID: 867 RVA: 0x00048135 File Offset: 0x00046335
        // (set) Token: 0x06000362 RID: 866 RVA: 0x0004812C File Offset: 0x0004632C
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

        // Token: 0x17000023 RID: 35
        // (get) Token: 0x06000365 RID: 869 RVA: 0x00048146 File Offset: 0x00046346
        // (set) Token: 0x06000364 RID: 868 RVA: 0x0004813D File Offset: 0x0004633D
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

        // Token: 0x17000024 RID: 36
        // (get) Token: 0x06000367 RID: 871 RVA: 0x00048157 File Offset: 0x00046357
        // (set) Token: 0x06000366 RID: 870 RVA: 0x0004814E File Offset: 0x0004634E
        public string userpassword
        {
            get
            {
                return this._userpassword;
            }
            set
            {
                this._userpassword = value;
            }
        }

        // Token: 0x17000025 RID: 37
        // (get) Token: 0x06000369 RID: 873 RVA: 0x00048168 File Offset: 0x00046368
        // (set) Token: 0x06000368 RID: 872 RVA: 0x0004815F File Offset: 0x0004635F
        public string newuserpassword
        {
            get
            {
                return this._newuserpassword;
            }
            set
            {
                this._newuserpassword = value;
            }
        }

        // Token: 0x17000026 RID: 38
        // (get) Token: 0x0600036A RID: 874 RVA: 0x00048170 File Offset: 0x00046370
        // (set) Token: 0x0600036B RID: 875 RVA: 0x00048178 File Offset: 0x00046378
        public byte[] Imageurl { get; set; }

        // Token: 0x17000027 RID: 39
        // (get) Token: 0x0600036D RID: 877 RVA: 0x0004818A File Offset: 0x0004638A
        // (set) Token: 0x0600036C RID: 876 RVA: 0x00048181 File Offset: 0x00046381
        public string Phone
        {
            get
            {
                return this._Phone;
            }
            set
            {
                this._Phone = value;
            }
        }

        // Token: 0x17000028 RID: 40
        // (get) Token: 0x0600036F RID: 879 RVA: 0x0004819B File Offset: 0x0004639B
        // (set) Token: 0x0600036E RID: 878 RVA: 0x00048192 File Offset: 0x00046392
        public string QQ
        {
            get
            {
                return this._QQ;
            }
            set
            {
                this._QQ = value;
            }
        }

        // Token: 0x17000029 RID: 41
        // (get) Token: 0x06000371 RID: 881 RVA: 0x000481AC File Offset: 0x000463AC
        // (set) Token: 0x06000370 RID: 880 RVA: 0x000481A3 File Offset: 0x000463A3
        public string Email
        {
            get
            {
                return this._Email;
            }
            set
            {
                this._Email = value;
            }
        }

        // Token: 0x1700002A RID: 42
        // (get) Token: 0x06000373 RID: 883 RVA: 0x000481BD File Offset: 0x000463BD
        // (set) Token: 0x06000372 RID: 882 RVA: 0x000481B4 File Offset: 0x000463B4
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

        // Token: 0x1700002B RID: 43
        // (get) Token: 0x06000375 RID: 885 RVA: 0x000481CE File Offset: 0x000463CE
        // (set) Token: 0x06000374 RID: 884 RVA: 0x000481C5 File Offset: 0x000463C5
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

        // Token: 0x1700002C RID: 44
        // (get) Token: 0x06000377 RID: 887 RVA: 0x000481DF File Offset: 0x000463DF
        // (set) Token: 0x06000376 RID: 886 RVA: 0x000481D6 File Offset: 0x000463D6
        public string deviceid
        {
            get
            {
                return this._deviceid;
            }
            set
            {
                this._deviceid = value;
            }
        }

        // Token: 0x1700002D RID: 45
        // (get) Token: 0x06000379 RID: 889 RVA: 0x000481F0 File Offset: 0x000463F0
        // (set) Token: 0x06000378 RID: 888 RVA: 0x000481E7 File Offset: 0x000463E7
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

        // Token: 0x040004C8 RID: 1224
        private string _username = string.Empty;

        // Token: 0x040004C9 RID: 1225
        private int _cashid;

        // Token: 0x040004CA RID: 1226
        private string _workkey = string.Empty;

        // Token: 0x040004CB RID: 1227
        private string _userpassword = string.Empty;

        // Token: 0x040004CC RID: 1228
        private string _newuserpassword = string.Empty;

        // Token: 0x040004CD RID: 1229
        private string _Phone = string.Empty;

        // Token: 0x040004CE RID: 1230
        private string _QQ = string.Empty;

        // Token: 0x040004CF RID: 1231
        private string _Email = string.Empty;

        // Token: 0x040004D0 RID: 1232
        private string _sign = string.Empty;

        // Token: 0x040004D1 RID: 1233
        private int _client;

        // Token: 0x040004D2 RID: 1234
        private string _deviceid = string.Empty;

        // Token: 0x040004D3 RID: 1235
        private float _version;
    }
}
