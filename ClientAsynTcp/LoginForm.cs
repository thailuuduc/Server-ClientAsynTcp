using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientAsynTcp
{
    public partial class LoginForm : Form
    {
        private const string ConnectionString = "Data Source=DESKTOP-096NAA0;Initial Catalog=ChatDatabase;Integrated Security=True;MultipleActiveResultSets=True";

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text;
            string password = txtPass.Text;

            
            if (CheckLogin(username, password))
            {
                if (!IsUserOnline(username))
                {
                    UpdateUserStatusOnline(username);
                    Form1 form2 = new Form1();
                    form2.Username = username;
                    Debug.WriteLine(username);
                    this.Hide();
                    form2.Show();
                }
                else
                {
                    MessageBox.Show("Tài khoản hiện đang được sử dụng!");
                }                    
            }
            else
            {
                MessageBox.Show("Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.");
                txtUser.Focus();
            }
        }
        private bool CheckLogin(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }
        private void UpdateUserStatusOnline(string username)
        {
            try
            {
                
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("UPDATE Users SET Status = 'online' WHERE Username = @Username", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi thiết lập trạng thái 'online': {ex}");
            }
        }
        private bool IsUserOnline(string username)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Status = 'online'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"lỗi kiểm tra trạng thái: {ex}");
                return false;
            }
        }

    }
}
