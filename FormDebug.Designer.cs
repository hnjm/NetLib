namespace Micro.NetLib {
    partial class FormDebug {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Servers");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Connected");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Disconnected");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("All");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Clients", new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode3,
            treeNode4});
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitLeft = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.cbRmUnused = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbDirectives = new System.Windows.Forms.CheckBox();
            this.cbCommands = new System.Windows.Forms.CheckBox();
            this.cbRaw = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitRaw = new System.Windows.Forms.SplitContainer();
            this.tableRaw = new System.Windows.Forms.DataGridView();
            this.rawDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rawDirection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rawID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rawLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rawInternal = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.rawAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableRawMsg = new System.Windows.Forms.DataGridView();
            this.msgIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.msgLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.msgData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableCommands = new System.Windows.Forms.DataGridView();
            this.intDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.intDirection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.intID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.intCommand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.intLinkState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.splitHigh = new System.Windows.Forms.SplitContainer();
            this.tableHigh = new System.Windows.Forms.DataGridView();
            this.highDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.highAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.highCommand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.highInvolved = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.highAdditional = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableHighMsg = new System.Windows.Forms.DataGridView();
            this.dirIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dirMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).BeginInit();
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRaw)).BeginInit();
            this.splitRaw.Panel1.SuspendLayout();
            this.splitRaw.Panel2.SuspendLayout();
            this.splitRaw.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableRaw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableRawMsg)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableCommands)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitHigh)).BeginInit();
            this.splitHigh.Panel1.SuspendLayout();
            this.splitHigh.Panel2.SuspendLayout();
            this.splitHigh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableHigh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableHighMsg)).BeginInit();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitLeft);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.tabControl1);
            this.splitMain.Size = new System.Drawing.Size(884, 461);
            this.splitMain.SplitterDistance = 164;
            this.splitMain.TabIndex = 1;
            // 
            // splitLeft
            // 
            this.splitLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLeft.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitLeft.Location = new System.Drawing.Point(0, 0);
            this.splitLeft.Name = "splitLeft";
            this.splitLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitLeft.Panel1
            // 
            this.splitLeft.Panel1.Controls.Add(this.treeView);
            // 
            // splitLeft.Panel2
            // 
            this.splitLeft.Panel2.Controls.Add(this.cbRmUnused);
            this.splitLeft.Panel2.Controls.Add(this.label1);
            this.splitLeft.Panel2.Controls.Add(this.cbDirectives);
            this.splitLeft.Panel2.Controls.Add(this.cbCommands);
            this.splitLeft.Panel2.Controls.Add(this.cbRaw);
            this.splitLeft.Size = new System.Drawing.Size(164, 461);
            this.splitLeft.SplitterDistance = 358;
            this.splitLeft.TabIndex = 2;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            treeNode1.Checked = true;
            treeNode1.ForeColor = System.Drawing.Color.Red;
            treeNode1.Name = "servers";
            treeNode1.Text = "Servers";
            treeNode2.Checked = true;
            treeNode2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            treeNode2.Name = "clientsOn";
            treeNode2.Text = "Connected";
            treeNode3.Checked = true;
            treeNode3.ForeColor = System.Drawing.Color.Green;
            treeNode3.Name = "clientsOff";
            treeNode3.Text = "Disconnected";
            treeNode4.Checked = true;
            treeNode4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            treeNode4.Name = "clientsAll";
            treeNode4.Text = "All";
            treeNode5.Checked = true;
            treeNode5.ForeColor = System.Drawing.Color.Blue;
            treeNode5.Name = "clients";
            treeNode5.Text = "Clients";
            this.treeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode5});
            this.treeView.PathSeparator = "/";
            this.treeView.Size = new System.Drawing.Size(164, 358);
            this.treeView.TabIndex = 0;
            // 
            // cbRmUnused
            // 
            this.cbRmUnused.AutoSize = true;
            this.cbRmUnused.Location = new System.Drawing.Point(6, 78);
            this.cbRmUnused.Name = "cbRmUnused";
            this.cbRmUnused.Size = new System.Drawing.Size(141, 17);
            this.cbRmUnused.TabIndex = 3;
            this.cbRmUnused.Text = "Remove unused objects";
            this.cbRmUnused.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Data to log:";
            // 
            // cbDirectives
            // 
            this.cbDirectives.AutoSize = true;
            this.cbDirectives.Location = new System.Drawing.Point(20, 60);
            this.cbDirectives.Name = "cbDirectives";
            this.cbDirectives.Size = new System.Drawing.Size(77, 17);
            this.cbDirectives.TabIndex = 1;
            this.cbDirectives.Text = "High-Level";
            this.cbDirectives.UseVisualStyleBackColor = true;
            // 
            // cbCommands
            // 
            this.cbCommands.AutoSize = true;
            this.cbCommands.Location = new System.Drawing.Point(20, 40);
            this.cbCommands.Name = "cbCommands";
            this.cbCommands.Size = new System.Drawing.Size(115, 17);
            this.cbCommands.TabIndex = 1;
            this.cbCommands.Text = "Internal commands";
            this.cbCommands.UseVisualStyleBackColor = true;
            // 
            // cbRaw
            // 
            this.cbRaw.AutoSize = true;
            this.cbRaw.Location = new System.Drawing.Point(20, 20);
            this.cbRaw.Name = "cbRaw";
            this.cbRaw.Size = new System.Drawing.Size(89, 17);
            this.cbRaw.TabIndex = 1;
            this.cbRaw.Text = "Raw packets";
            this.cbRaw.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(716, 461);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitRaw);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(708, 435);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Raw packets";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitRaw
            // 
            this.splitRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRaw.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitRaw.Location = new System.Drawing.Point(3, 3);
            this.splitRaw.Name = "splitRaw";
            this.splitRaw.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitRaw.Panel1
            // 
            this.splitRaw.Panel1.Controls.Add(this.tableRaw);
            // 
            // splitRaw.Panel2
            // 
            this.splitRaw.Panel2.Controls.Add(this.tableRawMsg);
            this.splitRaw.Size = new System.Drawing.Size(702, 429);
            this.splitRaw.SplitterDistance = 317;
            this.splitRaw.TabIndex = 1;
            // 
            // tableRaw
            // 
            this.tableRaw.AllowUserToAddRows = false;
            this.tableRaw.AllowUserToDeleteRows = false;
            this.tableRaw.AllowUserToResizeRows = false;
            this.tableRaw.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tableRaw.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableRaw.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableRaw.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tableRaw.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableRaw.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.tableRaw.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableRaw.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.rawDate,
            this.rawDirection,
            this.rawID,
            this.rawLength,
            this.rawInternal,
            this.rawAmount});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableRaw.DefaultCellStyle = dataGridViewCellStyle2;
            this.tableRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRaw.EnableHeadersVisualStyles = false;
            this.tableRaw.Location = new System.Drawing.Point(0, 0);
            this.tableRaw.Name = "tableRaw";
            this.tableRaw.ReadOnly = true;
            this.tableRaw.RowHeadersVisible = false;
            this.tableRaw.RowTemplate.Height = 20;
            this.tableRaw.RowTemplate.ReadOnly = true;
            this.tableRaw.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tableRaw.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tableRaw.Size = new System.Drawing.Size(702, 317);
            this.tableRaw.TabIndex = 1;
            // 
            // rawDate
            // 
            this.rawDate.HeaderText = "At";
            this.rawDate.Name = "rawDate";
            this.rawDate.ReadOnly = true;
            this.rawDate.Width = 41;
            // 
            // rawDirection
            // 
            this.rawDirection.HeaderText = "Direction";
            this.rawDirection.Name = "rawDirection";
            this.rawDirection.ReadOnly = true;
            this.rawDirection.Width = 73;
            // 
            // rawID
            // 
            this.rawID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.rawID.HeaderText = "From ~ To";
            this.rawID.Name = "rawID";
            this.rawID.ReadOnly = true;
            // 
            // rawLength
            // 
            this.rawLength.HeaderText = "Length";
            this.rawLength.Name = "rawLength";
            this.rawLength.ReadOnly = true;
            this.rawLength.Width = 64;
            // 
            // rawInternal
            // 
            this.rawInternal.HeaderText = "Internal";
            this.rawInternal.Name = "rawInternal";
            this.rawInternal.ReadOnly = true;
            this.rawInternal.Width = 47;
            // 
            // rawAmount
            // 
            this.rawAmount.HeaderText = "Size";
            this.rawAmount.Name = "rawAmount";
            this.rawAmount.ReadOnly = true;
            this.rawAmount.Width = 51;
            // 
            // tableRawMsg
            // 
            this.tableRawMsg.AllowUserToAddRows = false;
            this.tableRawMsg.AllowUserToDeleteRows = false;
            this.tableRawMsg.AllowUserToResizeRows = false;
            this.tableRawMsg.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tableRawMsg.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableRawMsg.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableRawMsg.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tableRawMsg.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableRawMsg.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.tableRawMsg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableRawMsg.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.msgIndex,
            this.msgLength,
            this.msgData});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableRawMsg.DefaultCellStyle = dataGridViewCellStyle4;
            this.tableRawMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRawMsg.EnableHeadersVisualStyles = false;
            this.tableRawMsg.Location = new System.Drawing.Point(0, 0);
            this.tableRawMsg.Name = "tableRawMsg";
            this.tableRawMsg.ReadOnly = true;
            this.tableRawMsg.RowHeadersVisible = false;
            this.tableRawMsg.RowTemplate.Height = 20;
            this.tableRawMsg.RowTemplate.ReadOnly = true;
            this.tableRawMsg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tableRawMsg.Size = new System.Drawing.Size(702, 108);
            this.tableRawMsg.TabIndex = 2;
            // 
            // msgIndex
            // 
            this.msgIndex.HeaderText = "N°";
            this.msgIndex.Name = "msgIndex";
            this.msgIndex.ReadOnly = true;
            this.msgIndex.Width = 43;
            // 
            // msgLength
            // 
            this.msgLength.HeaderText = "Message length";
            this.msgLength.Name = "msgLength";
            this.msgLength.ReadOnly = true;
            this.msgLength.Width = 106;
            // 
            // msgData
            // 
            this.msgData.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.msgData.HeaderText = "Message data";
            this.msgData.Name = "msgData";
            this.msgData.ReadOnly = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableCommands);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(708, 435);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Internal commands";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableCommands
            // 
            this.tableCommands.AllowUserToAddRows = false;
            this.tableCommands.AllowUserToDeleteRows = false;
            this.tableCommands.AllowUserToResizeRows = false;
            this.tableCommands.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tableCommands.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableCommands.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableCommands.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tableCommands.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableCommands.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.tableCommands.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableCommands.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.intDate,
            this.intDirection,
            this.intID,
            this.intCommand,
            this.intLinkState});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableCommands.DefaultCellStyle = dataGridViewCellStyle6;
            this.tableCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableCommands.EnableHeadersVisualStyles = false;
            this.tableCommands.Location = new System.Drawing.Point(3, 3);
            this.tableCommands.Name = "tableCommands";
            this.tableCommands.ReadOnly = true;
            this.tableCommands.RowHeadersVisible = false;
            this.tableCommands.RowTemplate.Height = 20;
            this.tableCommands.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tableCommands.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tableCommands.Size = new System.Drawing.Size(702, 429);
            this.tableCommands.TabIndex = 2;
            // 
            // intDate
            // 
            this.intDate.HeaderText = "At";
            this.intDate.Name = "intDate";
            this.intDate.ReadOnly = true;
            this.intDate.Width = 41;
            // 
            // intDirection
            // 
            this.intDirection.HeaderText = "Direction";
            this.intDirection.Name = "intDirection";
            this.intDirection.ReadOnly = true;
            this.intDirection.Width = 73;
            // 
            // intID
            // 
            this.intID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.intID.HeaderText = "From ~ To";
            this.intID.Name = "intID";
            this.intID.ReadOnly = true;
            // 
            // intCommand
            // 
            this.intCommand.HeaderText = "Command";
            this.intCommand.Name = "intCommand";
            this.intCommand.ReadOnly = true;
            this.intCommand.Width = 78;
            // 
            // intLinkState
            // 
            this.intLinkState.HeaderText = "Link State";
            this.intLinkState.Name = "intLinkState";
            this.intLinkState.ReadOnly = true;
            this.intLinkState.Width = 79;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.splitHigh);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(708, 435);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "High-Level Data";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // splitHigh
            // 
            this.splitHigh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitHigh.Location = new System.Drawing.Point(3, 3);
            this.splitHigh.Name = "splitHigh";
            this.splitHigh.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitHigh.Panel1
            // 
            this.splitHigh.Panel1.Controls.Add(this.tableHigh);
            // 
            // splitHigh.Panel2
            // 
            this.splitHigh.Panel2.Controls.Add(this.tableHighMsg);
            this.splitHigh.Size = new System.Drawing.Size(702, 429);
            this.splitHigh.SplitterDistance = 247;
            this.splitHigh.TabIndex = 4;
            // 
            // tableHigh
            // 
            this.tableHigh.AllowUserToAddRows = false;
            this.tableHigh.AllowUserToDeleteRows = false;
            this.tableHigh.AllowUserToResizeRows = false;
            this.tableHigh.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tableHigh.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableHigh.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableHigh.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tableHigh.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableHigh.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.tableHigh.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableHigh.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.highDate,
            this.highAction,
            this.highCommand,
            this.highInvolved,
            this.highAdditional});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableHigh.DefaultCellStyle = dataGridViewCellStyle8;
            this.tableHigh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableHigh.EnableHeadersVisualStyles = false;
            this.tableHigh.Location = new System.Drawing.Point(0, 0);
            this.tableHigh.Name = "tableHigh";
            this.tableHigh.ReadOnly = true;
            this.tableHigh.RowHeadersVisible = false;
            this.tableHigh.RowTemplate.Height = 20;
            this.tableHigh.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tableHigh.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tableHigh.Size = new System.Drawing.Size(702, 247);
            this.tableHigh.TabIndex = 3;
            // 
            // highDate
            // 
            this.highDate.HeaderText = "At";
            this.highDate.Name = "highDate";
            this.highDate.ReadOnly = true;
            this.highDate.Width = 41;
            // 
            // highAction
            // 
            this.highAction.HeaderText = "Action";
            this.highAction.Name = "highAction";
            this.highAction.ReadOnly = true;
            this.highAction.Width = 61;
            // 
            // highCommand
            // 
            this.highCommand.HeaderText = "Command";
            this.highCommand.Name = "highCommand";
            this.highCommand.ReadOnly = true;
            this.highCommand.Width = 78;
            // 
            // highInvolved
            // 
            this.highInvolved.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.highInvolved.HeaderText = "From → To ~ User";
            this.highInvolved.Name = "highInvolved";
            this.highInvolved.ReadOnly = true;
            // 
            // highAdditional
            // 
            this.highAdditional.HeaderText = "Size ~ Reason";
            this.highAdditional.Name = "highAdditional";
            this.highAdditional.ReadOnly = true;
            this.highAdditional.Width = 101;
            // 
            // tableHighMsg
            // 
            this.tableHighMsg.AllowUserToAddRows = false;
            this.tableHighMsg.AllowUserToDeleteRows = false;
            this.tableHighMsg.AllowUserToResizeRows = false;
            this.tableHighMsg.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tableHighMsg.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableHighMsg.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableHighMsg.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tableHighMsg.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableHighMsg.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.tableHighMsg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableHighMsg.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dirIndex,
            this.dirMessage});
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tableHighMsg.DefaultCellStyle = dataGridViewCellStyle10;
            this.tableHighMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableHighMsg.EnableHeadersVisualStyles = false;
            this.tableHighMsg.Location = new System.Drawing.Point(0, 0);
            this.tableHighMsg.Name = "tableHighMsg";
            this.tableHighMsg.ReadOnly = true;
            this.tableHighMsg.RowHeadersVisible = false;
            this.tableHighMsg.RowTemplate.Height = 20;
            this.tableHighMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tableHighMsg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tableHighMsg.Size = new System.Drawing.Size(702, 178);
            this.tableHighMsg.TabIndex = 7;
            // 
            // dirIndex
            // 
            this.dirIndex.HeaderText = "N°";
            this.dirIndex.Name = "dirIndex";
            this.dirIndex.ReadOnly = true;
            this.dirIndex.Width = 43;
            // 
            // dirMessage
            // 
            this.dirMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dirMessage.HeaderText = "Message";
            this.dirMessage.Name = "dirMessage";
            this.dirMessage.ReadOnly = true;
            // 
            // FormDebug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 461);
            this.Controls.Add(this.splitMain);
            this.DoubleBuffered = true;
            this.Name = "FormDebug";
            this.Text = "NetLib Debug";
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel2.ResumeLayout(false);
            this.splitLeft.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).EndInit();
            this.splitLeft.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitRaw.Panel1.ResumeLayout(false);
            this.splitRaw.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRaw)).EndInit();
            this.splitRaw.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableRaw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableRawMsg)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableCommands)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.splitHigh.Panel1.ResumeLayout(false);
            this.splitHigh.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitHigh)).EndInit();
            this.splitHigh.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableHigh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableHighMsg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitRaw;
        private System.Windows.Forms.DataGridView tableRaw;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView tableRawMsg;
        private System.Windows.Forms.SplitContainer splitLeft;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbDirectives;
        private System.Windows.Forms.CheckBox cbCommands;
        private System.Windows.Forms.CheckBox cbRaw;
        private System.Windows.Forms.DataGridView tableCommands;
        private System.Windows.Forms.DataGridView tableHigh;
        private System.Windows.Forms.SplitContainer splitHigh;
        private System.Windows.Forms.DataGridViewTextBoxColumn msgIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn msgLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn msgData;
        private System.Windows.Forms.CheckBox cbRmUnused;
        private System.Windows.Forms.DataGridView tableHighMsg;
        private System.Windows.Forms.DataGridViewTextBoxColumn dirIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn dirMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn rawDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn rawDirection;
        private System.Windows.Forms.DataGridViewTextBoxColumn rawID;
        private System.Windows.Forms.DataGridViewTextBoxColumn rawLength;
        private System.Windows.Forms.DataGridViewCheckBoxColumn rawInternal;
        private System.Windows.Forms.DataGridViewTextBoxColumn rawAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn intDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn intDirection;
        private System.Windows.Forms.DataGridViewTextBoxColumn intID;
        private System.Windows.Forms.DataGridViewTextBoxColumn intCommand;
        private System.Windows.Forms.DataGridViewTextBoxColumn intLinkState;
        private System.Windows.Forms.DataGridViewTextBoxColumn highDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn highAction;
        private System.Windows.Forms.DataGridViewTextBoxColumn highCommand;
        private System.Windows.Forms.DataGridViewTextBoxColumn highInvolved;
        private System.Windows.Forms.DataGridViewTextBoxColumn highAdditional;
    }
}