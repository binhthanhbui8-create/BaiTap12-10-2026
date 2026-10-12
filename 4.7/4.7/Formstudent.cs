using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace _4._7
{
    public partial class Formstudent : Form
    {
        public event Action<string>? StatusUpdated;

        private BindingList<Student> _studentList = new BindingList<Student>();
        private BindingSource _bindingSource = new BindingSource();

        private TextBox txtStudentId = null!;
        private TextBox txtFullName = null!;
        private NumericUpDown numAge = null!;
        private ComboBox cboGender = null!;
        private PictureBox picAvatar = null!;
        private Button btnBrowseAvatar = null!;

        private DataGridView dgvStudent = null!;
        private ContextMenuStrip cmsDataGrid = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnExportCsv = null!;

        public Formstudent()
        {
            InitCustomGUI();
            InitDataBinding();
        }

        private void InitCustomGUI()
        {
            this.Text = "Quản lý Sinh viên";
            this.Size = new Size(880, 560);

            GroupBox gbInput = new GroupBox
            {
                Text = "Thông tin chi tiết Sinh viên",
                Location = new Point(12, 12),
                Size = new Size(840, 190)
            };

            Label lblId = new Label { Text = "Mã SV:", Location = new Point(20, 30), AutoSize = true };
            Label lblName = new Label { Text = "Họ tên:", Location = new Point(20, 65), AutoSize = true };
            Label lblAge = new Label { Text = "Tuổi:", Location = new Point(20, 100), AutoSize = true };
            Label lblGender = new Label { Text = "Giới tính:", Location = new Point(20, 135), AutoSize = true };

            txtStudentId = new TextBox { Location = new Point(90, 27), Size = new Size(180, 23) };
            txtFullName = new TextBox { Location = new Point(90, 62), Size = new Size(180, 23) };
            numAge = new NumericUpDown { Location = new Point(90, 98), Size = new Size(100, 23), Minimum = 15, Maximum = 100, Value = 20 };

            cboGender = new ComboBox { Location = new Point(90, 132), Size = new Size(100, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new string[] { "Nam", "Nữ" });
            cboGender.SelectedIndex = 0;

            picAvatar = new PictureBox
            {
                Location = new Point(300, 25),
                Size = new Size(130, 140),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnBrowseAvatar = new Button { Text = "Chọn ảnh...", Location = new Point(440, 25), Size = new Size(90, 32) };
            btnBrowseAvatar.Click += BtnBrowseAvatar_Click;

            btnAdd = new Button { Text = "Thêm", Location = new Point(560, 25), Size = new Size(110, 35) };
            btnUpdate = new Button { Text = "Cập nhật", Location = new Point(560, 70), Size = new Size(110, 35) };
            btnDelete = new Button { Text = "Xóa", Location = new Point(560, 115), Size = new Size(110, 35) };
            btnExportCsv = new Button { Text = "Xuất CSV", Location = new Point(690, 25), Size = new Size(120, 35), BackColor = Color.LightSkyBlue };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnExportCsv.Click += BtnExportCsv_Click;

            gbInput.Controls.AddRange(new Control[] {
                lblId, txtStudentId, lblName, txtFullName,
                lblAge, numAge, lblGender, cboGender,
                picAvatar, btnBrowseAvatar,
                btnAdd, btnUpdate, btnDelete, btnExportCsv
            });

            dgvStudent = new DataGridView
            {
                Location = new Point(12, 215),
                Size = new Size(840, 290),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            cmsDataGrid = new ContextMenuStrip();
            var tsmiDelete = new ToolStripMenuItem("Xóa dòng", null, (s, e) => BtnDelete_Click(s, e));
            var tsmiRefresh = new ToolStripMenuItem("Làm mới dữ liệu", null, (s, e) => RefreshData());
            var tsmiDetail = new ToolStripMenuItem("Xem chi tiết", null, (s, e) => ShowDetail());
            cmsDataGrid.Items.AddRange(new ToolStripItem[] { tsmiDelete, tsmiRefresh, tsmiDetail });

            dgvStudent.ContextMenuStrip = cmsDataGrid;

            this.Controls.Add(gbInput);
            this.Controls.Add(dgvStudent);

            this.Load += (s, e) => StatusUpdated?.Invoke("Đã mở chức năng Quản lý Sinh viên.");
        }

        private void InitDataBinding()
        {
            _studentList.Add(new Student("SV001", "Nguyễn Văn Trung", 20, "Nam", ""));
            _studentList.Add(new Student("SV002", "Phạm Gia Bảo", 22, "Nữ", ""));
            _studentList.Add(new Student("SV004", "Bùi Thanh Bình", 21, "Nữ", ""));
            _studentList.Add(new Student("SV003", "Trần Đình Duy", 21, "Nam", ""));

            _bindingSource.DataSource = _studentList;
            dgvStudent.DataSource = _bindingSource;

            txtStudentId.DataBindings.Add("Text", _bindingSource, "MaSV", true, DataSourceUpdateMode.OnPropertyChanged);
            txtFullName.DataBindings.Add("Text", _bindingSource, "HoTen", true, DataSourceUpdateMode.OnPropertyChanged);
            numAge.DataBindings.Add("Value", _bindingSource, "Tuoi", true, DataSourceUpdateMode.OnPropertyChanged);
            cboGender.DataBindings.Add("SelectedItem", _bindingSource, "GioiTinh", true, DataSourceUpdateMode.OnPropertyChanged);

            _bindingSource.CurrentChanged += (s, e) => UpdateAvatarFromCurrent();
            UpdateAvatarFromCurrent();
        }

        private void UpdateAvatarFromCurrent()
        {
            if (_bindingSource.Current is Student currentStudent && !string.IsNullOrEmpty(currentStudent.AvatarPath) && File.Exists(currentStudent.AvatarPath))
            {
                try
                {
                    using (FileStream fs = new FileStream(currentStudent.AvatarPath, FileMode.Open, FileAccess.Read))
                    {
                        picAvatar.Image = Image.FromStream(fs);
                    }
                }
                catch
                {
                    picAvatar.Image = null;
                }
            }
            else
            {
                picAvatar.Image = null;
            }
        }

        private void BtnBrowseAvatar_Click(object? sender, EventArgs e)
        {
            if (_bindingSource.Current is Student currentStudent)
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Chọn ảnh đại diện";
                    dlg.Filter = "Tệp hình ảnh (*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        currentStudent.AvatarPath = dlg.FileName;
                        UpdateAvatarFromCurrent();
                        _bindingSource.ResetCurrentItem();
                        StatusUpdated?.Invoke($"Đã chọn ảnh đại diện cho sinh viên: {currentStudent.MaSV}");
                    }
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã SV!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Student newStudent = new Student(
                txtStudentId.Text.Trim(),
                txtFullName.Text.Trim(),
                (int)numAge.Value,
                cboGender.SelectedItem?.ToString() ?? "Nam",
                ""
            );

            _studentList.Add(newStudent);
            _bindingSource.MoveLast();
            StatusUpdated?.Invoke($"Đã thêm sinh viên mới: {newStudent.MaSV} - {newStudent.HoTen}");
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            _bindingSource.EndEdit();
            _bindingSource.ResetCurrentItem();
            dgvStudent.Refresh();
            StatusUpdated?.Invoke("Đã cập nhật thông tin sinh viên.");
            MessageBox.Show("Cập nhật thông tin sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_bindingSource.Current is Student currentStudent)
            {
                if (MessageBox.Show($"Bạn có chắc muốn xóa sinh viên {currentStudent.HoTen}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string maSV = currentStudent.MaSV;
                    _bindingSource.RemoveCurrent();
                    StatusUpdated?.Invoke($"Đã xóa sinh viên thành công: {maSV}");
                }
            }
        }

        private void RefreshData()
        {
            _bindingSource.ResetBindings(false);
            StatusUpdated?.Invoke("Đã làm mới dữ liệu danh sách sinh viên.");
        }

        private void ShowDetail()
        {
            if (_bindingSource.Current is Student currentStudent)
            {
                MessageBox.Show($"Mã SV: {currentStudent.MaSV}\nHọ và Tên: {currentStudent.HoTen}\nTuổi: {currentStudent.Tuoi}\nGiới tính: {currentStudent.GioiTinh}\nĐường dẫn ảnh: {currentStudent.AvatarPath}",
                                "Chi tiết Sinh viên", MessageBoxButtons.OK, MessageBoxIcon.Information);
                StatusUpdated?.Invoke($"Xem chi tiết sinh viên: {currentStudent.MaSV}");
            }
        }

        private void BtnExportCsv_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Xuất danh sách sinh viên ra tệp CSV";
                dlg.Filter = "Tệp CSV (*.csv)|*.csv";
                dlg.FileName = $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(dlg.FileName, false, Encoding.UTF8))
                        {
                            writer.WriteLine("Mã SV,Họ và Tên,Tuổi,Giới tính,Đường dẫn ảnh");
                            foreach (Student s in _studentList)
                            {
                                writer.WriteLine($"\"{s.MaSV}\",\"{s.HoTen}\",{s.Tuoi},\"{s.GioiTinh}\",\"{s.AvatarPath}\"");
                            }
                        }
                        StatusUpdated?.Invoke($"Xuất file CSV thành công: {dlg.FileName}");
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}