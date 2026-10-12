using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _4._8
{
    public partial class Formstudent : Form
    {
        // Sự kiện gửi thông điệp về FormMain (MDI Parent)
        public event Action<string>? StatusUpdated;

        private BindingList<Student> _studentList = new BindingList<Student>();
        private BindingSource _bindingSource = new BindingSource();

        // Controls nhập liệu
        private TextBox txtStudentId = null!;
        private TextBox txtFullName = null!;
        private NumericUpDown numAge = null!;
        private ComboBox cboGender = null!;
        private PictureBox picAvatar = null!;
        private Button btnBrowseAvatar = null!;

        // Controls thao tác
        private DataGridView dgvStudent = null!;
        private ContextMenuStrip cmsDataGrid = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnExportCsv = null!;

        // CONTROLS BẤT ĐỒNG BỘ & PROGRESSBAR (BÀI 4.8)
        private Button btnLoadAsync = null!;
        private ProgressBar prgLoading = null!;
        private Label lblProgress = null!;

        public Formstudent()
        {
            InitCustomGUI();
            InitDataBinding();
        }

        private void InitCustomGUI()
        {
            this.Text = "Quản lý Sinh viên";
            this.Size = new Size(880, 580);

            GroupBox gbInput = new GroupBox
            {
                Text = "Thông tin chi tiết & Thao tác Bất đồng bộ",
                Location = new Point(12, 12),
                Size = new Size(840, 200)
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

            btnAdd = new Button { Text = "Thêm", Location = new Point(550, 25), Size = new Size(110, 32) };
            btnUpdate = new Button { Text = "Cập nhật", Location = new Point(550, 65), Size = new Size(110, 32) };
            btnDelete = new Button { Text = "Xóa", Location = new Point(550, 105), Size = new Size(110, 32) };
            btnExportCsv = new Button { Text = "Xuất CSV", Location = new Point(550, 145), Size = new Size(110, 32), BackColor = Color.LightSkyBlue };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnExportCsv.Click += BtnExportCsv_Click;

            // BÀI 4.8: CONTROLS NẠP DỮ LIỆU BẤT ĐỒNG BỘ (ASYNC)
            btnLoadAsync = new Button
            {
                Text = "⚡ Nạp Async",
                Location = new Point(680, 25),
                Size = new Size(140, 35),
                BackColor = Color.LightGreen,
                Font = new Font(this.Font, FontStyle.Bold)
            };
            btnLoadAsync.Click += BtnLoadAsync_Click;

            prgLoading = new ProgressBar
            {
                Location = new Point(680, 75),
                Size = new Size(140, 23),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            lblProgress = new Label
            {
                Text = "Trạng thái: Sẵn sàng",
                Location = new Point(680, 105),
                Size = new Size(140, 40),
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = Color.DarkBlue
            };

            gbInput.Controls.AddRange(new Control[] {
                lblId, txtStudentId, lblName, txtFullName,
                lblAge, numAge, lblGender, cboGender,
                picAvatar, btnBrowseAvatar,
                btnAdd, btnUpdate, btnDelete, btnExportCsv,
                btnLoadAsync, prgLoading, lblProgress
            });

            dgvStudent = new DataGridView
            {
                Location = new Point(12, 225),
                Size = new Size(840, 300),
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

            _bindingSource.DataSource = _studentList;
            dgvStudent.DataSource = _bindingSource;

            txtStudentId.DataBindings.Add("Text", _bindingSource, "MaSV", true, DataSourceUpdateMode.OnPropertyChanged);
            txtFullName.DataBindings.Add("Text", _bindingSource, "HoTen", true, DataSourceUpdateMode.OnPropertyChanged);
            numAge.DataBindings.Add("Value", _bindingSource, "Tuoi", true, DataSourceUpdateMode.OnPropertyChanged);
            cboGender.DataBindings.Add("SelectedItem", _bindingSource, "GioiTinh", true, DataSourceUpdateMode.OnPropertyChanged);

            _bindingSource.CurrentChanged += (s, e) => UpdateAvatarFromCurrent();
            UpdateAvatarFromCurrent();
        }

        #region XỬ LÝ BẤT ĐỒNG BỘ (ASYNC / AWAIT & PROGRESSBAR) - BÀI 4.8

        /// <summary>
        /// Phương thức nạp dữ liệu bất đồng bộ giả lập tác vụ ngầm tiêu tốn thời gian
        /// </summary>
        private async Task<List<Student>> LoadStudentsAsync(IProgress<int> progress)
        {
            return await Task.Run(() =>
            {
                List<Student> resultList = new List<Student>();
                int totalRows = 20;

                for (int i = 1; i <= totalRows; i++)
                {
                    // Giả lập nạp từng bản ghi mất 150ms
                    Thread.Sleep(150);

                    resultList.Add(new Student(
                        $"SV{i:D3}",
                        $"Sinh viên Bất đồng bộ {i}",
                        18 + (i % 6),
                        (i % 2 == 0) ? "Nam" : "Nữ",
                        ""
                    ));

                    // Tính toán % tiến trình
                    int percentComplete = (i * 100) / totalRows;

                    // Báo cáo tiến trình về UI Thread
                    progress?.Report(percentComplete);
                }

                return resultList;
            });
        }

        /// <summary>
        /// Sự kiện kích hoạt nạp Async
        /// </summary>
        private async void BtnLoadAsync_Click(object? sender, EventArgs e)
        {
            // 1. Quản lý trạng thái UI: Khóa nút bấm tránh kích hoạt trùng
            SetControlsEnabled(false);
            prgLoading.Value = 0;

            // 2. Sử dụng IProgress<int> để nhận phần trăm hoàn thành theo thời gian thực
            var progress = new Progress<int>(percent =>
            {
                prgLoading.Value = percent;
                lblProgress.Text = $"Đang tải: {percent}%";

                // Cập nhật trạng thái phần trăm lên StatusStrip của MDI Parent (FormMain)
                StatusUpdated?.Invoke($"Đang nạp dữ liệu sinh viên... ({percent}%)");
            });

            // 3. Khối try...catch...finally quản lý ngoại lệ và mở lại giao diện
            try
            {
                StatusUpdated?.Invoke("Bắt đầu nạp dữ liệu lớn bất đồng bộ...");

                List<Student> loadedList = await LoadStudentsAsync(progress);

                _studentList.Clear();
                foreach (var student in loadedList)
                {
                    _studentList.Add(student);
                }

                StatusUpdated?.Invoke("Nạp dữ liệu hoàn tất 100%!");
                lblProgress.Text = "Hoàn tất: 100%";
                MessageBox.Show("Nạp dữ liệu bất đồng bộ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke($"Lỗi nạp dữ liệu: {ex.Message}");
                lblProgress.Text = "Xảy ra lỗi!";
                MessageBox.Show($"Xảy ra lỗi trong quá trình nạp dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Mở lại các nút bấm sau khi hoàn tất hoặc xảy ra lỗi
                SetControlsEnabled(true);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnLoadAsync.Enabled = enabled;
            btnAdd.Enabled = enabled;
            btnUpdate.Enabled = enabled;
            btnDelete.Enabled = enabled;
            btnExportCsv.Enabled = enabled;
            btnBrowseAvatar.Enabled = enabled;
        }

        #endregion

        #region THAO TÁC KHÁC

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

        #endregion
    }
}