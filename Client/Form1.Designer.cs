namespace Client
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.buttonEnviarMensagem = new System.Windows.Forms.Button();
            this.textBoxEscreverMensagem = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.button_Registo = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.listBoxConversa = new System.Windows.Forms.ListBox();
            this.button_Login = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(572, 43);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Conversação";
            // 
            // buttonEnviarMensagem
            // 
            this.buttonEnviarMensagem.Enabled = false;
            this.buttonEnviarMensagem.Location = new System.Drawing.Point(589, 353);
            this.buttonEnviarMensagem.Margin = new System.Windows.Forms.Padding(2);
            this.buttonEnviarMensagem.Name = "buttonEnviarMensagem";
            this.buttonEnviarMensagem.Size = new System.Drawing.Size(186, 28);
            this.buttonEnviarMensagem.TabIndex = 11;
            this.buttonEnviarMensagem.Text = "Enviar Mensagem";
            this.buttonEnviarMensagem.UseVisualStyleBackColor = true;
            this.buttonEnviarMensagem.Click += new System.EventHandler(this.buttonEnviarMensagem_Click);
            // 
            // textBoxEscreverMensagem
            // 
            this.textBoxEscreverMensagem.Enabled = false;
            this.textBoxEscreverMensagem.Location = new System.Drawing.Point(493, 317);
            this.textBoxEscreverMensagem.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxEscreverMensagem.Multiline = true;
            this.textBoxEscreverMensagem.Name = "textBoxEscreverMensagem";
            this.textBoxEscreverMensagem.Size = new System.Drawing.Size(378, 23);
            this.textBoxEscreverMensagem.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(354, 320);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(126, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Escreva uma mensagem:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(40, 96);
            this.textBoxUsername.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxUsername.Multiline = true;
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(221, 28);
            this.textBoxUsername.TabIndex = 18;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(38, 81);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Username";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(38, 134);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Password";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(40, 151);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPassword.Multiline = true;
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(221, 28);
            this.textBoxPassword.TabIndex = 21;
            // 
            // button_Registo
            // 
            this.button_Registo.Location = new System.Drawing.Point(40, 199);
            this.button_Registo.Margin = new System.Windows.Forms.Padding(2);
            this.button_Registo.Name = "button_Registo";
            this.button_Registo.Size = new System.Drawing.Size(92, 24);
            this.button_Registo.TabIndex = 22;
            this.button_Registo.Text = "Registo";
            this.button_Registo.UseVisualStyleBackColor = true;
            this.button_Registo.Click += new System.EventHandler(this.button_Registo_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.textBox1.Location = new System.Drawing.Point(12, 11);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(862, 20);
            this.textBox1.TabIndex = 23;
            this.textBox1.Text = "Chat de Conversação";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.UseWaitCursor = true;
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(40, 283);
            this.buttonDisconnect.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(221, 27);
            this.buttonDisconnect.TabIndex = 24;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // listBoxConversa
            // 
            this.listBoxConversa.FormattingEnabled = true;
            this.listBoxConversa.Location = new System.Drawing.Point(359, 72);
            this.listBoxConversa.Name = "listBoxConversa";
            this.listBoxConversa.Size = new System.Drawing.Size(512, 225);
            this.listBoxConversa.TabIndex = 25;
            // 
            // button_Login
            // 
            this.button_Login.Location = new System.Drawing.Point(167, 199);
            this.button_Login.Margin = new System.Windows.Forms.Padding(2);
            this.button_Login.Name = "button_Login";
            this.button_Login.Size = new System.Drawing.Size(94, 24);
            this.button_Login.TabIndex = 26;
            this.button_Login.Text = "Login";
            this.button_Login.UseVisualStyleBackColor = true;
            this.button_Login.Click += new System.EventHandler(this.button_Login_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 400);
            this.Controls.Add(this.button_Login);
            this.Controls.Add(this.listBoxConversa);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button_Registo);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxEscreverMensagem);
            this.Controls.Add(this.buttonEnviarMensagem);
            this.Controls.Add(this.label3);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Aplicação";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonEnviarMensagem;
        private System.Windows.Forms.TextBox textBoxEscreverMensagem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button button_Registo;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.ListBox listBoxConversa;
        private System.Windows.Forms.Button button_Login;
    }
}

