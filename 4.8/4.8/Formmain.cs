using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace _4._8
{
    public partial class FormMain : Form
    {
        private MenuStrip menuStrip1 = null!;
        private ToolStrip toolStrip1 = null!;
        private StatusStrip statusStrip1 = null!;
        private ToolStripStatusLabel lblStatus = null!;
        private ToolStripStatusLabel lblUser = null!;
        private ToolStripStatusLabel lblTime = null!;
        private System.Windows.Forms.Timer timerSystem = null!;

        public FormMain()
        {
            InitCustomGUI();
        }

        private void InitCustomGUI()
        {
            this.IsMdiContainer = true;
            this.Text = "HỆ THỐNG QUẢN LÝ TẬP TRUNG (MDI)";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            menuStrip1 = new MenuStrip();

            var mnuSystem = new ToolStripMenuItem("Hệ thống");
            var mnuLogout = new ToolStripMenuItem("Đăng xuất", null, (s, e) => Logout());
            var mnuExit = new ToolStripMenuItem("Thoát", null, (s, e) => Application.Exit());
            mnuSystem.DropDownItems.AddRange(new ToolStripItem[] { mnuLogout, mnuExit });

            var mnuCategory = new ToolStripMenuItem("Danh mục");
            var mnuStudent = new ToolStripMenuItem("Quản lý Sinh viên", null, (s, e) => OpenChildForm<Formstudent>());
            var mnuProduct = new ToolStripMenuItem("Quản lý Sản phẩm", null, (s, e) => OpenChildForm<FormProduct>());
            mnuCategory.DropDownItems.AddRange(new ToolStripItem[] { mnuStudent, mnuProduct });

            var mnuWindow = new ToolStripMenuItem("Cửa sổ");
            var mnuCascade = new ToolStripMenuItem("Sắp xếp Cascade", null, (s, e) => this.LayoutMdi(MdiLayout.Cascade));
            var mnuTileH = new ToolStripMenuItem("Sắp xếp Tile Horizontal", null, (s, e) => this.LayoutMdi(MdiLayout.TileHorizontal));
            var mnuTileV = new ToolStripMenuItem("Sắp xếp Tile Vertical", null, (s, e) => this.LayoutMdi(MdiLayout.TileVertical));
            mnuWindow.DropDownItems.AddRange(new ToolStripItem[] { mnuCascade, mnuTileH, mnuTileV });

            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuCategory, mnuWindow });
            menuStrip1.MdiWindowListItem = mnuWindow;

            toolStrip1 = new ToolStrip();
            var tsbStudent = new ToolStripButton("Sinh viên", null, (s, e) => OpenChildForm<Formstudent>());
            var tsbProduct = new ToolStripButton("Sản phẩm", null, (s, e) => OpenChildForm<FormProduct>());
            var tsbExit = new ToolStripButton("Thoát", null, (s, e) => Application.Exit());
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbStudent, tsbProduct, new ToolStripSeparator(), tsbExit });

            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Hệ thống sẵn sàng") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            lblUser = new ToolStripStatusLabel("Người dùng: Admin | ");
            lblTime = new ToolStripStatusLabel(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, lblUser, lblTime });

            timerSystem = new System.Windows.Forms.Timer { Interval = 1000, Enabled = true };
            timerSystem.Tick += (s, e) =>
            {
                lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };

            this.Controls.Add(toolStrip1);
            this.Controls.Add(menuStrip1);
            this.Controls.Add(statusStrip1);
            this.MainMenuStrip = menuStrip1;
        }

        private void OpenChildForm<T>() where T : Form, new()
        {
            Form? childForm = this.MdiChildren.FirstOrDefault(f => f is T);

            if (childForm == null)
            {
                childForm = new T();
                childForm.MdiParent = this;

                if (childForm is Formstudent frmStudent)
                {
                    frmStudent.StatusUpdated += UpdateStatusMessage;
                }

                childForm.Show();
            }
            else
            {
                childForm.Activate();
            }
        }

        public void UpdateStatusMessage(string message)
        {
            lblStatus.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }

        private void Logout()
        {
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
            }
            UpdateStatusMessage("Đã đăng xuất khỏi hệ thống.");
        }
    }
}