using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _1109_MapAPI
{
    class LocationSearch
    {
        public List<Location> LocList { get; set; }

        public LocationSearch()
        {
            LocList = new List<Location>();
        }

        public void Find(int sido, int gugun, out string xmlstring)
        {
            string url = "http://apis.data.go.kr/B552061/frequentzoneFreezing/getRestFrequentzoneFreezing";
            url += "?ServiceKey=" + "9oe0ec7ET3HVwwQGTBctfytMGun2T8SjEQgcI8IHSqIlTYGhec2KEnQGID7mmd4EEIQMLMxACapXrTmi8tdr1g%3D%3D"; // Service Key
            url += "&ServiceKey=9oe0ec7ET3HVwwQGTBctfytMGun2T8SjEQgcI8IHSqIlTYGhec2KEnQGID7mmd4EEIQMLMxACapXrTmi8tdr1g%3D%3D";
            url += "&searchYearCd=2017"; //외부에서 입력 필요
            url += "&siDo=" + sido;//외부에서 입력 필요
            url += "&guGun=" + gugun;//외부에서 입력 필요
            url += "&type=xml";
            url += "&numOfRows=10";
            url += "&pageNo=1";// 결과가 XML 포맷
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            string results = string.Empty;
            HttpWebResponse response;
            using (response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                xmlstring = reader.ReadToEnd();//results
            }

            Console.WriteLine(xmlstring);

            //3.  xml문서를 dompaser로 파싱
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlstring));
            XmlDocument doc = new XmlDocument();
            doc.Load(ms);
            XmlNode n1 = doc.SelectSingleNode("response");
            XmlNode node = n1.SelectSingleNode("body");
            XmlNode n = node.SelectSingleNode("items");

            Location loc = null;
            foreach (XmlNode el in n.SelectNodes("item"))
            {
                loc = Location.MakeLocation(el);
                LocList.Add(loc);
            }
        }

        public Location LocNameToLocation(string code)
        {
            foreach (Location l in LocList)
            {
                if (l.Bjd_cd == 0) return null;
                string temp = l.Bjd_cd.ToString();

                if (temp.Substring(0, 5) == code.Substring(0, 5))
                {
                    return l;
                }
            }
            return null;
        }
    }
}
