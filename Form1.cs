using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            btnLogin.Focus();
        }

        public UserDto loggedUser;

        private void btnLogin_Click(object sender, EventArgs e)
        {
            UserDto user = new UserDto(txtUsername.Text, txtPassword.Text);
            //var loginResult = Login(user);
            var loginResult = Login2(user);
            this.loggedUser = loginResult.Result;
            txtSessionIdentifier.Text = this.loggedUser.sessid;
            txtSessionName.Text = this.loggedUser.session_name;
            txtToken.Text = this.loggedUser.token;
            //CheckLogin(this.loggedUser);
        }
        public const string BaseUrl = "http://www.mtouton.com";
        public const string LoginUrl = "/api-users/user/login.json";
        public const string ConnectUrl = "/api-users/system/connect.json";
        public const string UsersUrl = "/?q=/api-users/user/&pagesize=3000&fields=uid,name,mail,created,access,status,uuid";
        public const string TokenUrl = "/?q=services/session/token";

        public string GetToken()
        {
            string url = string.Concat(BaseUrl, TokenUrl);
            try
            {
                var request = CreateJsonRequest(url, HttpMethod.Get, new Cookie(txtSessionName.Text, txtSessionIdentifier.Text, "/", "www.mtouton.com"));
                //var newRequest = CreateJsonRequest(url);
                request.Headers.Add("X-CSRF-Token", txtToken.Text);
                var responseText = GetJsonResponseText(request);
                lstResult.Items.Add(responseText);
                return responseText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error in Token call");
                return null;
            }
        }

        public JToken GetUsers()
        {
            string url = string.Concat(BaseUrl, UsersUrl);
            try
            {
                var request = CreateJsonRequest(url, HttpMethod.Get, new Cookie(txtSessionName.Text, txtSessionIdentifier.Text, "/", "www.mtouton.com"));
                //var newRequest = CreateJsonRequest(url);
                request.Headers.Add("X-CSRF-Token", txtToken.Text);
                var responseText = GetJsonResponseText(request);
                var result = JToken.Parse(responseText);
                lstResult.Items.Add(new ListBoxItem(responseText, string.Format("{0} users retrieved.", result.Count())));
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error in Users call");
                return null;
            }
        }

        public UserDto Login(UserDto user)
        {
            string url = string.Concat(BaseUrl, LoginUrl);
            try
            {
                var request = CreateJsonRequest(url, HttpMethod.Post);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string query = JsonConvert.SerializeObject(user);
                    streamWriter.Write(query);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var responseText = GetJsonResponseText(request);
                lstResult.Items.Add(new ListBoxItem(responseText, responseText));
                return JsonConvert.DeserializeObject<UserDto>(responseText);
                //return userJson;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error in Login call");
                return null;
            }
        }

        public async Task<UserDto> Login2(UserDto user)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://www.mtouton.com/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.PostAsJsonAsync("api-users/user/login.json", user).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var strResponse = await response.Content.ReadAsStringAsync();
                        lstResult.Items.Add(new ListBoxItem(strResponse, strResponse));
                        return JsonConvert.DeserializeObject<UserDto>(strResponse);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error in Login call");
                return null;
            }
        }

        protected string GetJsonResponseText(HttpWebRequest request)
        {
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    return responseText;
                }
            }
        }

        protected HttpWebRequest CreateJsonRequest(string url, System.Net.Http.HttpMethod method,  params Cookie[] cookies)
        {
            var newRequest = (HttpWebRequest)WebRequest.Create(url);
            newRequest.ContentType = "application/json";
            newRequest.Method = method.Method;
            newRequest.CookieContainer = new CookieContainer();
            foreach (var cookie in cookies) {
                newRequest.CookieContainer.Add(cookie);
            }
            return newRequest;
        }


        public void CheckLogin(UserDto user)
        {
            try
            {
                string url = string.Concat(BaseUrl, ConnectUrl);
                //var token = userJson["token"].ToString();
                //var sessid = userJson["sessid"].ToString();
                //var session_name = userJson["session_name"].ToString();
                var token = user.token;
                var sessid = user.sessid;
                var session_name = user.session_name;
                var newRequest = CreateJsonRequest(url, HttpMethod.Post, new Cookie(session_name, sessid, "/", "www.mtouton.com"));
                //var newRequest = CreateJsonRequest(url);
                newRequest.Headers.Add("X-CSRF-Token", token);
                var responseText = GetJsonResponseText(newRequest);
                lstResult.Items.Add(new ListBoxItem(responseText, responseText));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error checking authentication");
            }
        }
        
        private void lstResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtDetail.Text = string.Empty;
            if(lstResult.SelectedItem != null) {
                ListBoxItem item = lstResult.SelectedItem as ListBoxItem;
                JToken element = JToken.Parse(item.value);
                txtDetail.Text = Newtonsoft.Json.JsonConvert.SerializeObject(element, Formatting.Indented);
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            var user = this.loggedUser ?? new UserDto(string.Empty, string.Empty);
            if (!string.IsNullOrEmpty(txtSessionName.Text)) {
                user.session_name = txtSessionName.Text;
            }
            if (!string.IsNullOrEmpty(txtSessionIdentifier.Text))
            {
                user.sessid = txtSessionIdentifier.Text;
            }
            if (!string.IsNullOrEmpty(txtToken.Text)) {
                user.token = txtToken.Text;
            }
            CheckLogin(user);
        }

        private void btnGetUsers_Click(object sender, EventArgs e)
        {
            var users = GetUsers();
        }
    }

    public class ListBoxItem
    {
        public ListBoxItem() { }
        public ListBoxItem(string value, string description) { this.value = value;  this.description = description; }
        public ListBoxItem(string value) { this.value = value; this.description = value; }
        public string value { get; set; }
        public string description { get; set; }
        public override string ToString()
        {
            return this.description;
        }
    }
    public class UserDto
    {
        public string name { get { return this.username; } set { this.username = value; } }
        public string username;
        public string mail;
        public string pass;
        public string token;
        public string sessid;
        public string session_name;

        public UserDto(string username, string pass, string mail = "")
        {
            this.username = username;
            this.mail = mail;
            this.pass = pass;

        }
    }
}
