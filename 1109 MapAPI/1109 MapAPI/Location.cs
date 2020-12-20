using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace _1109_MapAPI
{
    class Coordinate
    {
        public double XCoord { get; set; }
        public double YCoord { get; set; }

        public Coordinate(double x, double y)
        {
            XCoord = x;
            YCoord = y;
        }
    }
    class Location
    {
        public string Sido_sgg_nm { get; private set; }
        public string Spot_nm { get; private set; }
        public long Bjd_cd { get; private set; }
        public double Lo_crd { get; private set; }
        public double La_crd { get; private set; }
        public int Occrrnc_cnt { get; set; }

        public string Geom_json { get; set; }

        private List<Coordinate> coordinates = new List<Coordinate>();
        public List<Coordinate> Coordinates { get { return coordinates; } }


        public Location(string sido, string spot, double lo_crd, double la_crd, int occrrnc_cnt, long bjd, string geom_json)
        {
            Sido_sgg_nm = sido; Spot_nm = spot; Lo_crd = lo_crd; La_crd = la_crd; Occrrnc_cnt = occrrnc_cnt; Bjd_cd = bjd;

            Geom_json = geom_json;

            //Geom_json 저장된 정보가 아래와 같다.
            //"[[[xxx,yy],[zzz,qqq],[eeee,ggg]]]"
            //파싱해서 Coordinates.Add .....
            List<string> stringtemp = JsonPrettyPrint(Geom_json);//111,222
            foreach (string str in stringtemp)
            {
                string[] split = str.Split(',');
                coordinates.Add(new Coordinate(double.Parse(split[0]), double.Parse(split[1])));

            }
        }
        private List<string> JsonPrettyPrint(string json)
        {
            List<string> liststring = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '[':
                        sb.Clear();
                        break;
                    case ']':
                        liststring.Add(sb.ToString());
                        break;
                    case ',':
                        sb.Append(ch);
                        break;
                    default:
                        if (ch != ' ') sb.Append(ch);
                        break;
                }
            }
            return liststring;
        }

        public Location()
        {
            Sido_sgg_nm = null; Spot_nm = null; Lo_crd = 0; La_crd = 0; Occrrnc_cnt = 0; Bjd_cd = 0;
            Geom_json = null;
        }


        static internal Location MakeLocation(XmlNode xn)
        {
            string sido_sgg_nm = string.Empty;
            string spot_nm = string.Empty;

            double lo_crd = 0;
            double la_crd = 0;

            if (xn.SelectSingleNode("sido_sgg_nm") == null)
            {
                MessageBox.Show("자료 없음");
                return new Location();
            }
            XmlNode sido_node = xn.SelectSingleNode("sido_sgg_nm");
            sido_sgg_nm = ConvertString(sido_node.InnerText);

            XmlNode spot_node = xn.SelectSingleNode("spot_nm");
            spot_nm = ConvertString(spot_node.InnerText);

            XmlNode lo_crd_node = xn.SelectSingleNode("lo_crd");
            lo_crd = double.Parse(lo_crd_node.InnerText);
            XmlNode la_crd_node = xn.SelectSingleNode("la_crd");
            la_crd = double.Parse(la_crd_node.InnerText);


            XmlNode occrrnc_cnt_node = xn.SelectSingleNode("occrrnc_cnt");
            int occrrnc_cnt = int.Parse(occrrnc_cnt_node.InnerText);

            XmlNode bjd_cd_node = xn.SelectSingleNode("bjd_cd");
            long bjd_cd = long.Parse(bjd_cd_node.InnerText);

            XmlNode geom_json_node = xn.SelectSingleNode("geom_json");
            string geom_json = geom_json_node.InnerText;
            var jobj = JObject.Parse(geom_json);
            string temp = jobj.SelectToken("coordinates").ToString();

            return new Location(sido_sgg_nm, spot_nm, lo_crd, la_crd, occrrnc_cnt, bjd_cd, temp);
        }

        private static string ConvertString(string str)
        {
            int sindex = 0;
            int eindex = 0;
            while (true)
            {
                sindex = str.IndexOf('<');
                if (sindex == -1)
                {
                    break;
                }
                eindex = str.IndexOf('>');
                str = str.Remove(sindex, eindex - sindex + 1);
            }
            return str;
        }

    }
}
