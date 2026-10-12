using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace _4._8
{
    public class Student : INotifyPropertyChanged
    {
        private string _maSV = "";
        private string _hoTen = "";
        private int _tuoi = 18;
        private string _gioiTinh = "Nam";
        private string _avatarPath = "";

        [DisplayName("Mã SV")]
        public string MaSV
        {
            get => _maSV;
            set { _maSV = value; OnPropertyChanged(); }
        }

        [DisplayName("Họ và Tên")]
        public string HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(); }
        }

        [DisplayName("Tuổi")]
        public int Tuoi
        {
            get => _tuoi;
            set { _tuoi = value; OnPropertyChanged(); }
        }

        [DisplayName("Giới tính")]
        public string GioiTinh
        {
            get => _gioiTinh;
            set { _gioiTinh = value; OnPropertyChanged(); }
        }

        [DisplayName("Đường dẫn ảnh")]
        public string AvatarPath
        {
            get => _avatarPath;
            set { _avatarPath = value; OnPropertyChanged(); }
        }

        public Student() { }

        public Student(string maSV, string hoTen, int tuoi, string gioiTinh, string avatarPath = "")
        {
            MaSV = maSV;
            HoTen = hoTen;
            Tuoi = tuoi;
            GioiTinh = gioiTinh;
            AvatarPath = avatarPath;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}