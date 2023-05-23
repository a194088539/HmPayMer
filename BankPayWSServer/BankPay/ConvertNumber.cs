using Newtonsoft.Json;

namespace BankPayWSServer.BankPay
{
    public static class ConvertNumber
    {
        // Token: 0x06000087 RID: 135 RVA: 0x000031DF File Offset: 0x000013DF
        public static string ConvertDecimal(this decimal obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
