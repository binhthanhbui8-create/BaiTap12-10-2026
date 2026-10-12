using System.Drawing;
using System.Windows.Forms;

namespace _4._8
{
    public partial class FormProduct : Form
    {
        public FormProduct()
        {
            this.Text = "Quản lý Sản phẩm";
            this.Size = new Size(600, 400);

            Label lblInfo = new Label
            {
                Text = "Màn hình Quản lý Sản phẩm (MDI Child)",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 150)
            };

            this.Controls.Add(lblInfo);
        }
    }
}