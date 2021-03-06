﻿using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CipherCryptanalysisTool
{

    public partial class Main : Form
    {
        int minLimitNumber = 0;
        int maxLimitNumber = 0;
        SubstitutionCiphers substitutionCipher = new SubstitutionCiphers();
        AES aes = new AES();
        Info info;
        int countforfile = 0;

        public Main()
        {
            InitializeComponent();

            //Custom Init
            radioBtn_Cipher.Select();
            comboBox1.Items.Add("Additive Cipher");
            comboBox1.Items.Add("Multiplicative Cipher");
            comboBox1.Items.Add("AES - 128bit");
            comboBox1.Items.Add("AES - 192bit");
            comboBox1.Items.Add("AES - 256bit");
            comboBox1.SelectedIndex = 2;
            minLimitNumber = 0;
            maxLimitNumber = 25;

            /*
             * refer : http://blog.daum.net/limdongmyung/16
             * C# TrackPopup 처리(마우스 우클릭시 메뉴처리)
             * MouseClick 속성추가
             */
            EventHandler eh = new EventHandler(MenuClick);
            MenuItem[] ami = {
                    new MenuItem("파일 열기",eh, Shortcut.CtrlO),
                    new MenuItem("파일 저장",eh,Shortcut.CtrlS),
                    new MenuItem("프로그램 종료",eh,Shortcut.CtrlW),
                };
            ContextMenu = new System.Windows.Forms.ContextMenu(ami);
        }

        /*
         *  파일을 여는 함수
         */
        private void Btn_fOpen_Click(object sender, EventArgs e)
        {
            //reference : http://www.howtosolutions.net/2012/05/winform-open-file-browser-dialog-examples/
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (radioBtn_Cipher.Checked)
                {
                    tabControl1.SelectedIndex = 1;
                    text_Cipher.Text = File.ReadAllText(ofd.FileName);
                }
                else if (radioBtn_Plain.Checked)
                {
                    tabControl1.SelectedIndex = 0;
                    text_Plain.Text = File.ReadAllText(ofd.FileName);
                }
                this.Text = "암호화/복호화 툴 : " + ofd.FileName;
            }
        }

        /*
         * 파일을 저장하는 함수
         */
        private void btn_fSave_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog() == DialogResult.OK)
            {   
                if (radioBtn_Cipher.Checked)
                {
                    File.WriteAllText(sfd.FileName, text_Cipher.Text);
                }
                else if (radioBtn_Plain.Checked)
                {
                    File.WriteAllText(sfd.FileName, text_Plain.Text);
                }
                this.Text = "암호화/복호화 툴 : " + sfd.FileName;
            }
        }
        private void MiddleProcessSave()
        {
            //refer : http://stackoverflow.com/questions/15443739/what-is-the-simplest-way-to-write-the-contents-of-a-stringbuilder-to-a-text-file
            System.IO.StreamWriter sw = new System.IO.StreamWriter("MiddleProcess.txt", false);
            sw.Write(aes.getMiddleProcessString().ToString());
            sw.Close();

            System.Diagnostics.Process.Start("notepad.exe", "MiddleProcess.txt");
        }

        /*
         * 각 암호마다 제한하는 키의 수 등을 정의하는 함수이다.
         */
        private void text_CipherKey_TextChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex == 0 || comboBox1.SelectedIndex == 1)
            {
                int startKey = 0;
                text_CipherKey.MaxLength = 3;
                bool res = Int32.TryParse(((TextBox)sender).Text, out startKey);

                if (res && startKey < minLimitNumber)
                {
                    text_CipherKey.Text = minLimitNumber.ToString();
                    text_CipherKey.Select(text_CipherKey2.Text.Length, 0);
                }
                else if (res && startKey > maxLimitNumber)
                {
                    text_CipherKey.Text = maxLimitNumber.ToString();
                    text_CipherKey.Select(text_CipherKey2.Text.Length, 0);
                }
                text_CipherKey2.Text = text_CipherKey.Text;
            }
            if(comboBox1.SelectedIndex == 2)
            {
                text_CipherKey.MaxLength = 16;
                if (Encoding.Default.GetByteCount(text_CipherKey.Text) > 16)
                {
                    text_CipherKey.Text = text_CipherKey.Text.Substring(0, text_CipherKey.Text.Length - 1);
                    text_CipherKey.Select(text_CipherKey.Text.Length, 0);
                }
            }
            if (comboBox1.SelectedIndex == 3)
            {
                text_CipherKey.MaxLength = 24;
                if (Encoding.Default.GetByteCount(text_CipherKey.Text) > 24)
                {
                    text_CipherKey.Text = text_CipherKey.Text.Substring(0, text_CipherKey.Text.Length - 1);
                    text_CipherKey.Select(text_CipherKey.Text.Length, 0);
                }
            }
            if (comboBox1.SelectedIndex == 4)
            {
                text_CipherKey.MaxLength = 32;
                if (Encoding.Default.GetByteCount(text_CipherKey.Text) > 32)
                {
                    text_CipherKey.Text = text_CipherKey.Text.Substring(0, text_CipherKey.Text.Length - 1);
                    text_CipherKey.Select(text_CipherKey.Text.Length, 0);
                }
            }
        }

        private void text_CipherKey2_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0 || comboBox1.SelectedIndex == 1)
            {
                int startKey = 0;
                text_CipherKey.MaxLength = 3;
                bool res = Int32.TryParse(((TextBox)sender).Text, out startKey);
                if (res && startKey < minLimitNumber)
                    text_CipherKey2.Text = minLimitNumber.ToString();
                else if (res && startKey > maxLimitNumber)
                    text_CipherKey2.Text = maxLimitNumber.ToString();
            }
        }

        /*
         * 복호화 버튼을 클릭하면, Combobox에 맞춰서 복호화를 한다.
         */
        private void btn_Decrypt_Click(object sender, EventArgs e)
        {
            checkBox1.Enabled = false;

            char[] array;
            int startKey = 0;
            bool res = Int32.TryParse(text_CipherKey.Text, out startKey);
            int endKey = 0;
            bool res2 = Int32.TryParse(text_CipherKey2.Text, out endKey);
            
            if (res && res2 && startKey > endKey)
            {
                string temp;
                temp = text_CipherKey.Text;
                text_CipherKey.Text = text_CipherKey2.Text;
                text_CipherKey2.Text = temp;
            }

            if (text_Cipher.Text == "")
            {
                MessageBox.Show("복호화할 문자열이 없습니다.");
                checkBox1.Enabled = true;
                return;
            }
            else
                array = text_Cipher.Text.ToLower().ToCharArray();

            switch (comboBox1.SelectedIndex)
            {
                case 0: //Additive Cipher Decrypt
                    text_Plain.Text = substitutionCipher.AdditiveCipher_Decrypt(array, startKey, endKey);
                    break;
                case 1: //Multiplicative Cipher Decrypt
                    text_Plain.Text = substitutionCipher.MultiplicativeCipher_Decrypt(array, startKey, endKey);
                    break;
                case 2: //AES - 128bit Decrypt
                    aes.setNr(10); //10 Round
                    aes.setMasterKeySize(4); //4 Word = 16byte = 128bit
                    aes.setMiddleProcess(checkBox1.Checked);
                    text_Plain.Text = aes.Decrypt(text_Cipher.Text,text_CipherKey.Text);
                    break;
                case 3: //AES - 192bit Decrypt
                    aes.setNr(12); //12 Round
                    aes.setMasterKeySize(6); //6 Word = 24byte = 192bit
                    aes.setMiddleProcess(checkBox1.Checked);
                    text_Plain.Text = aes.Decrypt(text_Cipher.Text, text_CipherKey.Text);
                    break;
                case 4: //AES - 256bit Decrypt
                    aes.setNr(14); //14 Round
                    aes.setMasterKeySize(8); //8 Word = 32byte = 256bit
                    aes.setMiddleProcess(checkBox1.Checked);
                    text_Plain.Text = aes.Decrypt(text_Cipher.Text, text_CipherKey.Text);
                    break;
                default:
                    break;
            }
            tabControl1.SelectedIndex = 0;
            if (checkBox1.Checked)
                MiddleProcessSave();
            
            checkBox1.Enabled = true;
            
            
        }

        private void btn_Encrypt_Click(object sender, EventArgs e)
        {
            checkBox1.Enabled = false;

            char[] array;
            int startKey;
            bool res = Int32.TryParse(text_CipherKey.Text, out startKey);
            int endKey;
            bool res2 = Int32.TryParse(text_CipherKey2.Text, out endKey);

            if (res && res2 && startKey > endKey)
            {
                string temp;
                temp = text_CipherKey.Text;
                text_CipherKey.Text = text_CipherKey2.Text;
                text_CipherKey2.Text = temp;
            }
            text_Cipher.Text = null;

            if (text_Plain.Text == "")
            {
                MessageBox.Show("암호화할 문자열이 없습니다.");
                checkBox1.Enabled = true;
                return;
            }
            else
                array = text_Plain.Text.ToLower().ToCharArray();

            switch (comboBox1.SelectedIndex)
            {
                case 0: //Additive Cipher
                    text_Cipher.Text = substitutionCipher.AdditiveCipher_Encrypt(array, startKey, endKey);
                    break;
                case 1: //Multiplicative Cipher
                    text_Cipher.Text = substitutionCipher.MultiplicativeCipher_Encrypt(array, startKey, endKey);
                    break;
                case 2: //AES - 128bit Enycrypt
                    aes.setNr(10); //10 Round
                    aes.setMasterKeySize(4); //4 Word = 16byte = 128bit
                    aes.setMiddleProcess(checkBox1.Checked);
                    text_Cipher.Text = aes.Encrypt(text_Plain.Text, text_CipherKey.Text);
                    break;
                case 3: //AES - 192bit Decrypt
                    aes.setNr(12); //12 Round
                    aes.setMasterKeySize(6); //6 Word = 24byte = 192bit
                    aes.setMiddleProcess(checkBox1.Checked);
                    text_Cipher.Text = aes.Encrypt(text_Plain.Text, text_CipherKey.Text);
                    break;
                case 4: //AES - 256bit Decrypt
                    aes.setNr(14); //14 Round
                    aes.setMasterKeySize(8); //8 Word = 32byte = 256bit
                    aes.setMiddleProcess(checkBox1.Checked);
                    text_Cipher.Text = aes.Encrypt(text_Plain.Text, text_CipherKey.Text);
                    break;
                default:
                    break;
            }
            tabControl1.SelectedIndex = 1;
            if (checkBox1.Checked)
                MiddleProcessSave();

            checkBox1.Enabled = true;
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        void MenuClick(object obj, EventArgs ea)
        {
            MenuItem mI = (MenuItem)obj;
            String str = mI.Text;
            if (str == "파일 열기")
                Btn_fOpen_Click(obj, ea);
            if (str == "파일 저장")
                btn_fSave_Click(obj, ea);
            if (str == "프로그램 종료")
                Close();
        }

        /*
         * AES 인지 SubstitutionCipher인지에 따라서 키입력 방식이 다르므로, 그에 맞춰 변경하는 함수이다.
         */
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex == 0 || comboBox1.SelectedIndex == 1)
            {
                text_CipherKey.Text = text_CipherKey2.Text = "";
                label4.Visible = true;
                text_CipherKey2.Visible = true;
                text_CipherKey.Size = new Size(50, 25);
                checkBox1.Visible = false;
            }
            else
            {
                text_CipherKey.Text = text_CipherKey2.Text = "";
                label4.Visible = false;
                text_CipherKey2.Visible = false;
                text_CipherKey.Size = new Size(185, 25);
                checkBox1.Visible = true;
            }
        }

        /*
         * refer : https://msdn.microsoft.com/ko-kr/library/system.windows.forms.timer.tick(v=vs.110).aspx
         * refer : https://msdn.microsoft.com/ko-kr/library/system.windows.forms.form.opacity(v=vs.110).aspx
         * Main 폼 시작 시, info 폼을 생성하여, info form을 0.01초마다 투명도를 이용하여, 애니메이션 효과를 생성하여
         * 정보를 보여주는 함수이다.
         * Background 출처 : http://www.wallpaperup.com/187338/Avicii_Triangles_Logo_HD_Wallpaper_DJ.html
         */
        private void Main_Load(object sender, EventArgs e)
        {
            info = new Info();
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            Opacity = 0;
            info.Show();
            timer1.Start();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (info.UpDown)
                info.Opacity += 0.01;
            else if (!info.UpDown && info.Opacity <= 0.00)
            {
                info.Hide();
                timer1.Stop();
                Visible = true; // Hide form window.
                ShowInTaskbar = true; // Remove from taskbar.
                Opacity = 1;
            }
            else
                info.Opacity -= 0.01;

            if (info.Opacity >= 1.0)
            {
                info.count++;
                if (info.count > 50)
                    info.UpDown = false;
            }
        }

        private void text_Plain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
                if (e.KeyCode == Keys.A)
                    text_Plain.SelectAll();

        }

        private void text_Cipher_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
                if (e.KeyCode == Keys.A)
                    text_Cipher.SelectAll();
        }
    }
}
