using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace _1109_MapAPI
{
    public partial class Form1 : Form
    {
        List<BJD> BJDList = new List<BJD>();
        LocationSearch search = new LocationSearch();

        // 지역명, 위도, 경도  (ex. "문정동", "37.412412", "124.512512")
        List<Tuple<string, double, double>> tuples = new List<Tuple<string, double, double>>();

        public Form1()
        {
            InitializeComponent();
        }

        public void Search(string area) // 지역 검색
        {
            // 요청을 보낼 url 
            string site = "https://dapi.kakao.com/v2/local/search/address.json";
            string query = string.Format("{0}?query={1}", site, area);

            WebRequest request = WebRequest.Create(query); // 요청 생성. 
            string api_key = "105cb428e0eda2e5def79d48ee775f09"; // API 인증키 입력. (각자 발급한 API 인증키를 입력하자)
            string header = "KakaoAK " + api_key;

            request.Headers.Add("Authorization", header); // HTTP 헤더 "Authorization" 에 header 값 설정. 
            WebResponse response = request.GetResponse(); // 요청을 보내고 응답 객체를 받는다. 
            Stream stream = response.GetResponseStream(); // 응답객체의 결과물
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            String json = reader.ReadToEnd(); // JOSN 포멧 문자열

            JavaScriptSerializer js = new JavaScriptSerializer(); // (Reference 에 System.Web.Extensions.dll 을 추가해야한다)
            var dob = js.Deserialize<dynamic>(json);
            var docs = dob["documents"];
            object[] buf = docs;
            int length = buf.Length;

            for (int i = 0; i < length; i++) // 지역명, 위도, 경도 읽어오기. 
            {
                string address_name = docs[i]["address_name"];
                double x = double.Parse(docs[i]["x"]); // 위도
                double y = double.Parse(docs[i]["y"]); // 경도
                tuples.Add(new Tuple<string, double, double>(address_name, x, y));
            }
        }


     

        private void input(string str)
        {
            listBox1.Items.Clear();
            tuples.Clear();

            Search(textBox1.Text);
            for (int i = 0; i < tuples.Count; i++)
            {
                listBox1.Items.Add(tuples[i].Item1);
            }
        }

        private void ShowMap() // 위도, 경도에 해당하는 지역을 지도에 표시
        {
            var sel = tuples[listBox1.SelectedIndex];
            object[] arr = new object[] { sel.Item3, sel.Item2 }; // 위도, 경도
            object res = webBrowser1.Document.InvokeScript("panTo", arr); // html 의 panTo 자바스크립트 함수 호출. 
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {
            // WebBrowser 컨트롤에 "kakaoMap.html" 을 표시한다. 
            Version ver = webBrowser1.Version;
            string name = webBrowser1.ProductName;
            string str = webBrowser1.ProductVersion;
            string html = "kakaoMap.html";
            string dir = System.IO.Directory.GetCurrentDirectory();
            string path = System.IO.Path.Combine(dir, html);
            webBrowser1.Navigate(path);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            BJDList.Clear();
            try
            {
                input(textBox1.Text);
                
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {

             webBrowser1.Document.InvokeScript("zoomIn"); // 줌인
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    input(textBox1.Text);
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("zoomOut"); // 줌아웃
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                textBox1.Text = listBox1.SelectedItem.ToString();
                
                BJDList.Clear();
                search.LocList.Clear();

                try
                {
                    object[] arr = new object[] { textBox1.Text };
                    webBrowser1.Document.InvokeScript("geo", arr); // html 의 geo 자바스크립트 함수 호출. 
                    
                    
                    
                    //텍스트박스의 내용을 법정동 코드 텍스트파일과 비교해 법정동 코드 및 바른 명칭으로 불러옴
                    #region 검색
                    string txt = listBox1.SelectedItem.ToString();
                    char sp = ' ';
                    string[] temp = new string[2];
                    string[] spstring = txt.Split(sp);
                    
                    if (spstring.Length > 2) { 
                        temp[0] = spstring[1]; 
                        temp[1]=spstring[2]; //서울 종로구 ~~동
                    }

                    string path1 = @"bjd.txt";
                    string loc = string.Empty;
                    string code = string.Empty;
                    string[] textValue = System.IO.File.ReadAllLines(path1, Encoding.Default);
                    for (int i = 1; i < textValue.Length; i++)
                    {
                        char sp1 = '\t';
                        string[] spstring1 = textValue[i].Split(sp1);
                        char sp2 = ' ';
                        string[] spstring2 = spstring1[1].Split(sp2);
                        if (spstring1[2] != "폐지" && spstring2.Length > 2)
                        {
                            if (spstring2[2].Equals(temp[1])&& spstring2[1].Equals(temp[0]))
                            {

                                loc = spstring1[1];
                                code = spstring1[0];
                                BJD tbjd = new BJD(code, loc);
                                BJDList.Add(tbjd);
                                break;
                            }

                        }
                    }
                    if (BJDList.Count == 0) {
                        MessageBox.Show("자료 없음");
                        return;
                    }
                    BJD temp1=new BJD();
                    

                    foreach (BJD b in BJDList) {
                        string txt1 = b.Loc;
                        string txt2 = listBox1.SelectedItem.ToString();
                        char sp2= ' ';
                        string[] spstring4 = txt.Split(sp2);
                        string[] spstring3 = txt.Split(sp2);

                        if (spstring3[1].Equals(spstring4[1])&& spstring3[2].Equals(spstring4[2]))
                        {
                            temp1 = b;
                        }
                    }
                    int sido = int.Parse(temp1.Code.Substring(0, 2));
                    int gugun = int.Parse(temp1.Code.Substring(2, 3)); ;

                    string xmlstring;
                    search.Find(sido, gugun, out xmlstring);
                    Location l = search.LocNameToLocation(temp1.Code);
                    if (l == null) return;

                    MessageBox.Show("사고 건수 : "+l.Occrrnc_cnt+"건\n"+ "폴리곤 0번째 x값 : " + l.Coordinates[0].XCoord + "\n폴리곤 0번째 y값 : " + l.Coordinates[0].YCoord);
                    ShowMap();
                    #endregion
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

        }

        public class BJD
        {
            public string Loc { get; set; }
            public string Code { get; set; }

            public BJD(string code, string loc) { Loc = loc; Code = code; }
            public BJD() { Loc = string.Empty; Code = string.Empty; }

            public List<BJD> BJDList { get; set; }
        }
    }

 
}
    