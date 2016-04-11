using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CookComputing.XmlRpc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestingDrupalAuth
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txtUsername.Text = "npascual";
            txtPassword.Text = "majestic";
            btnLogin.Focus();
        }



        private void btnLogin_Click(object sender, EventArgs e)
        {
            NewMethod();
        }

        public void NewMethod()
        {
            CookieContainer cookieJar = new CookieContainer();
            UserDto user = new UserDto(txtUsername.Text, txtPassword.Text, "npascual", "nicolaspascual@hawkant.com");
            string url = "http://www.mtouton.com/api-users/user/login.json";///user/login.json";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json";
                request.Method = "POST";
                request.CookieContainer = cookieJar;

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string query = JsonConvert.SerializeObject(user);

                    streamWriter.Write(query);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var response = (HttpWebResponse)request.GetResponse();
                Newtonsoft.Json.Linq.JObject userJson;
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    lstResult.Items.Add(responseText);
                    userJson = Newtonsoft.Json.Linq.JObject.Parse(responseText);
                }

                //Check to see if you're logged in
                url = "http://www.mtouton.com/api-users/system/connect.json";
                var newRequest = (HttpWebRequest)WebRequest.Create(url);
                
                newRequest.ContentType = "application/json";
                newRequest.Method = "POST";
                var token = userJson["token"].ToString();
                var sessid = userJson["sessid"].ToString();
                var session_name = userJson["session_name"].ToString();
                var cookie = new Cookie(session_name, sessid, "/", "www.mtouton.com");
                cookieJar.Add(cookie);
                newRequest.CookieContainer = cookieJar;
                newRequest.Headers.Add("X-CSRF-Token", token);
                var newResponse = (HttpWebResponse)newRequest.GetResponse();
                using (var newStreamReader = new StreamReader(newResponse.GetResponseStream()))
                {
                    var newResponseText = newStreamReader.ReadToEnd();
                    lstResult.Items.Add(newResponseText);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Can't access server");
            }
        }

        private void lstResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtDetail.Text = string.Empty;
            JObject element = JObject.Parse(lstResult.SelectedItem.ToString());
            txtDetail.Text = Newtonsoft.Json.JsonConvert.SerializeObject(element, Formatting.Indented);
        }
    }
    public class UserDto
    {
        public string name;
        public string username;
        public string email;
        public string pass;

        public UserDto(string username, string pass, string name = "", string email = "")
        {
            this.name = name;
            this.username = username;
            this.email = email;
            this.pass = pass;

        }
    }
}
