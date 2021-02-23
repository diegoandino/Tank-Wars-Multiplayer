namespace TankWars
{
	partial class GameView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ServerLabel = new System.Windows.Forms.Label();
			this.ServerTextBox = new System.Windows.Forms.TextBox();
			this.PlayerNameTextLabel = new System.Windows.Forms.Label();
			this.NameTextBox = new System.Windows.Forms.TextBox();
			this.ConnectButton = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.HelpButton = new System.Windows.Forms.Button();
			this.HelpPanel = new System.Windows.Forms.Panel();
			this.AboutButton = new System.Windows.Forms.Button();
			this.ControlButton = new System.Windows.Forms.Button();
			this.FPSCounter = new System.Windows.Forms.Label();
			this.HelpPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ServerLabel
			// 
			this.ServerLabel.AutoSize = true;
			this.ServerLabel.Location = new System.Drawing.Point(22, 9);
			this.ServerLabel.Name = "ServerLabel";
			this.ServerLabel.Size = new System.Drawing.Size(38, 13);
			this.ServerLabel.TabIndex = 0;
			this.ServerLabel.Text = "Server";
			// 
			// ServerTextBox
			// 
			this.ServerTextBox.Location = new System.Drawing.Point(66, 6);
			this.ServerTextBox.Name = "ServerTextBox";
			this.ServerTextBox.Size = new System.Drawing.Size(141, 20);
			this.ServerTextBox.TabIndex = 1;
			this.ServerTextBox.Text = "localhost";
			// 
			// PlayerNameTextLabel
			// 
			this.PlayerNameTextLabel.AutoSize = true;
			this.PlayerNameTextLabel.Location = new System.Drawing.Point(231, 9);
			this.PlayerNameTextLabel.Name = "PlayerNameTextLabel";
			this.PlayerNameTextLabel.Size = new System.Drawing.Size(67, 13);
			this.PlayerNameTextLabel.TabIndex = 2;
			this.PlayerNameTextLabel.Text = "Player Name";
			// 
			// NameTextBox
			// 
			this.NameTextBox.Location = new System.Drawing.Point(313, 6);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = new System.Drawing.Size(141, 20);
			this.NameTextBox.TabIndex = 3;
			// 
			// ConnectButton
			// 
			this.ConnectButton.Location = new System.Drawing.Point(473, 4);
			this.ConnectButton.Name = "ConnectButton";
			this.ConnectButton.Size = new System.Drawing.Size(141, 23);
			this.ConnectButton.TabIndex = 4;
			this.ConnectButton.Text = "Connect To Server";
			this.ConnectButton.UseVisualStyleBackColor = true;
			this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.AutoSize = false;
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1049, 35);
			this.menuStrip1.TabIndex = 5;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// HelpButton
			// 
			this.HelpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HelpButton.Location = new System.Drawing.Point(962, 6);
			this.HelpButton.Name = "HelpButton";
			this.HelpButton.Size = new System.Drawing.Size(75, 23);
			this.HelpButton.TabIndex = 6;
			this.HelpButton.TabStop = false;
			this.HelpButton.Text = "Help";
			this.HelpButton.UseVisualStyleBackColor = true;
			this.HelpButton.Click += new System.EventHandler(this.HelpButton_Click);
			// 
			// HelpPanel
			// 
			this.HelpPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HelpPanel.Controls.Add(this.AboutButton);
			this.HelpPanel.Controls.Add(this.ControlButton);
			this.HelpPanel.Location = new System.Drawing.Point(962, 35);
			this.HelpPanel.Name = "HelpPanel";
			this.HelpPanel.Size = new System.Drawing.Size(75, 43);
			this.HelpPanel.TabIndex = 7;
			// 
			// AboutButton
			// 
			this.AboutButton.Location = new System.Drawing.Point(0, 23);
			this.AboutButton.Name = "AboutButton";
			this.AboutButton.Size = new System.Drawing.Size(75, 21);
			this.AboutButton.TabIndex = 1;
			this.AboutButton.Text = "About";
			this.AboutButton.UseVisualStyleBackColor = true;
			this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
			// 
			// ControlButton
			// 
			this.ControlButton.Location = new System.Drawing.Point(0, 0);
			this.ControlButton.Name = "ControlButton";
			this.ControlButton.Size = new System.Drawing.Size(75, 25);
			this.ControlButton.TabIndex = 0;
			this.ControlButton.Text = "Controls";
			this.ControlButton.UseVisualStyleBackColor = true;
			this.ControlButton.Click += new System.EventHandler(this.ControlButton_Click);
			// 
			// FPSCounter
			// 
			this.FPSCounter.AutoSize = true;
			this.FPSCounter.Location = new System.Drawing.Point(679, 13);
			this.FPSCounter.Name = "FPSCounter";
			this.FPSCounter.Size = new System.Drawing.Size(0, 13);
			this.FPSCounter.TabIndex = 8;
			// 
			// GameView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1049, 659);
			this.Controls.Add(this.FPSCounter);
			this.Controls.Add(this.HelpPanel);
			this.Controls.Add(this.HelpButton);
			this.Controls.Add(this.PlayerNameTextLabel);
			this.Controls.Add(this.ServerLabel);
			this.Controls.Add(this.ConnectButton);
			this.Controls.Add(this.NameTextBox);
			this.Controls.Add(this.ServerTextBox);
			this.Controls.Add(this.menuStrip1);
			this.Name = "GameView";
			this.Text = "TankWars - Diego Andino & Tarik Vu";
			this.HelpPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label ServerLabel;
		private System.Windows.Forms.TextBox ServerTextBox;
		private System.Windows.Forms.Label PlayerNameTextLabel;
		private System.Windows.Forms.TextBox NameTextBox;
		private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Button HelpButton;
        private System.Windows.Forms.Panel HelpPanel;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.Button ControlButton;
		private System.Windows.Forms.Label FPSCounter;
	}
}

