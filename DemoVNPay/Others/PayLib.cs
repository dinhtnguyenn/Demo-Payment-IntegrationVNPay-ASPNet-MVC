using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace DemoVNPay.Others
{
    public class PayLib
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new PayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new PayCompare());

        //------------------------REQUEST DATA----------------------------------------
        public void AddRequestData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(kv.Key + "=" + HttpUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();
            string rawData = GetRequestRaw();
            baseUrl += "?" + queryString;
            string vnp_SecureHash = Util.Sha256(vnp_HashSecret + rawData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;
            return baseUrl;
        }

        private string GetRequestRaw()
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(kv.Key + "=" + kv.Value + "&");
                }
            }
            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            return data.ToString();
        }



        //------------------------RESPONSE DATA----------------------------------------
        public void AddResponseData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            string retValue;
            if (_responseData.TryGetValue(key, out retValue))
            {
                return retValue;
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetResponseRaw()
        {

            StringBuilder data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }
            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(kv.Key + "=" + kv.Value + "&");
                }
            }

            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            return data.ToString();
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            string rspRaw = GetResponseRaw();
            string myChecksum = Util.Sha256(secretKey + rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}