using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Collections.Generic;
using RS.NetDiet.Therapist.Api.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace ApiControllerTests
{
    [TestClass]
    public class AccountController
    {
        private HttpClient client = new HttpClient();
        private string token = string.Empty;
        private string adminId = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            client.BaseAddress = new Uri("http://localhost/Therapist/");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "username", "devadmin" },
                { "password", "j9up1uuU!" }
            });
            var response = client.PostAsync("api/oauth/token", content).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            dynamic loginResponseData = response.Content.ReadAsAsync<object>().Result;
            token = (string)loginResponseData.access_token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var stringContent = new StringContent(JsonConvert.SerializeObject(new CreateAdminDto()
            {
                ConfirmPassword = "adminPassw0rd!",
                Email = "admin@email.com",
                FirstName = "Admin",
                Gender = Gender.Male,
                LastName = "User",
                Password = "adminPassw0rd!",
                PhoneNumber = "+40745024467",
                Title = Title.Mr,
                UserName = "adminuser"
            }), Encoding.UTF8, "application/json");
            response = client.PostAsync("api/accounts/create/admin", stringContent).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            var createAdminResponseData = response.Content.ReadAsAsync<UserReturnDto>().Result;
            adminId = createAdminResponseData.Id;
            Assert.IsFalse(string.IsNullOrEmpty(adminId));
        }

        [TestCleanup]
        public void Cleanup()
        {
            var response = client.DeleteAsync(string.Format("api/accounts/user/{0}", adminId)).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void GetUsers()
        {
            var response = client.GetAsync("api/accounts/users").Result;
            Assert.IsTrue(response.IsSuccessStatusCode);

            var getUsersResponseData = response.Content.ReadAsAsync<IEnumerable<UserReturnDto>>().Result;
            Assert.IsTrue(getUsersResponseData.Count() > 1);
        }

        [TestMethod]
        public void GetUser()
        {
            var response = client.GetAsync(string.Format("api/accounts/user/{0}", adminId)).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);

            var getUserResponseData = response.Content.ReadAsAsync<UserReturnDto>().Result;
            Assert.AreEqual(adminId, getUserResponseData.Id);
        }

        [TestMethod]
        public void GetMyInfo()
        {
            var response = client.GetAsync("api/accounts/info").Result;
            Assert.IsTrue(response.IsSuccessStatusCode);

            var getMyInfoResponseData = response.Content.ReadAsAsync<UserInfoDto>().Result;
            Assert.AreEqual("tuzi92@yahoo.com", getMyInfoResponseData.Email);
        }

        [TestMethod]
        public void CreateTherapist()
        {
            var content = new StringContent(JsonConvert.SerializeObject(new CreateTherapistDto()
            {
                Clinic = "clinic",
                Email = "tuzitamas@gmail.com",
                FirstName = "Test",
                Gender = Gender.Male,
                LastName = "Therapist",
                PhoneNumber = "+40745024467",
                Title = Title.Mr,
            }), Encoding.UTF8, "application/json");
            var response = client.PostAsync("api/accounts/create/therapist", content).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void ChangePassword()
        {
            var content = new StringContent(JsonConvert.SerializeObject(new ChangePasswordDto()
            {
                ConfirmPassword = "j9up1uuU!",
                NewPassword = "j9up1uuU!",
                OldPassword = "j9up1uuU!"
            }), Encoding.UTF8, "application/json");
            var response = client.PostAsync("api/accounts/changepassword", content).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void ResetPassword()
        {
            var content = new StringContent("tuzi92@yahoo.com", Encoding.UTF8, "application/json");
            var response = client.PostAsync("api/accounts/requestpasswordreset", content).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void AssignRolesToUser()
        {
            var content = new StringContent(JsonConvert.SerializeObject(new string[]
            {
                "Therapist", "Admin"
            }), Encoding.UTF8, "application/json");
            var response = client.PostAsync(string.Format("api/accounts/user/{0}/roles", adminId), content).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void UpdateMyInfo_Clinic()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "username", "admin@email.com" },
                { "password", "adminPassw0rd!" }
            });
            var response = client.PostAsync("api/oauth/token", content).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            dynamic loginResponseData = response.Content.ReadAsAsync<object>().Result;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)loginResponseData.access_token);

            var stringContent = new StringContent(JsonConvert.SerializeObject(new UserInfoDto()
            {
                Clinic = "TestClinic"
            }), Encoding.UTF8, "application/json");
            response = client.PostAsync("api/accounts/update", stringContent).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);

            response = client.GetAsync("api/accounts/info").Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            var getMyInfoResponseData = response.Content.ReadAsAsync<UserInfoDto>().Result;
            Assert.AreEqual("TestClinic", getMyInfoResponseData.Clinic);
        }

        [TestMethod]
        public void UpdateMyInfo_Email()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "username", "admin@email.com" },
                { "password", "adminPassw0rd!" }
            });
            var response = client.PostAsync("api/oauth/token", content).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            dynamic loginResponseData = response.Content.ReadAsAsync<object>().Result;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)loginResponseData.access_token);

            var stringContent = new StringContent(JsonConvert.SerializeObject(new UserInfoDto()
            {
                Email = "adminUser@email.com"
            }), Encoding.UTF8, "application/json");
            response = client.PostAsync("api/accounts/update", stringContent).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);

            response = client.GetAsync("api/accounts/info").Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            var getMyInfoResponseData = response.Content.ReadAsAsync<UserInfoDto>().Result;
            Assert.AreEqual("adminUser@email.com", getMyInfoResponseData.Email);
        }
    }
}
