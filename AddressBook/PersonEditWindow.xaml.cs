using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AddressBook.Data;
using AddressBook.Models;

namespace AddressBook
{
    public partial class PersonEditWindow : Window
    {
        private readonly PersonRepository _personRepo = new();
        private readonly AddressRepository _addressRepo = new();

        private Person _person;
        private Address? _editingAddress; // null => yeni ekleme

        public PersonEditWindow(int personId)
        {
            InitializeComponent();

            _person = _personRepo.GetAll().First(x => x.Id == personId);

            EditFirstName.Text = _person.FirstName;
            EditLastName.Text = _person.LastName;
            EditPhone.Text = _person.Phone;

            RefreshAddresses();
            ClearAddressForm();
        }

        private void RefreshAddresses()
        {
            AddressesList.ItemsSource = _addressRepo.GetByPersonId(_person.Id);
        }

        // --------- Kişi Kaydet (UPDATE) ----------
        private void SavePerson_Click(object sender, RoutedEventArgs e)
        {
            _person.FirstName = EditFirstName.Text?.Trim() ?? "";
            _person.LastName = EditLastName.Text?.Trim() ?? "";
            _person.Phone = string.IsNullOrWhiteSpace(EditPhone.Text) ? null : EditPhone.Text.Trim();

            _personRepo.Update(_person);

            // Ana ekrana “kaydedildi” bilgisini döndür
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // --------- Adres CRUD ----------
        private void NewAddress_Click(object sender, RoutedEventArgs e)
        {
            _editingAddress = null;
            ClearAddressForm();
        }

        private void EditAddressRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Address a) return;

            _editingAddress = a;
            AddrTitle.Text = a.Title;
            AddrCity.Text = a.City;
            AddrDistrict.Text = a.District;
            AddrLine.Text = a.AddressLine;
        }

        private void DeleteAddressRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Address a) return;

            var ok = MessageBox.Show("Adres silinsin mi?", "Silme Onayı",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (ok != MessageBoxResult.Yes) return;

            _addressRepo.Delete(a.Id);
            RefreshAddresses();

            if (_editingAddress?.Id == a.Id)
            {
                _editingAddress = null;
                ClearAddressForm();
            }
        }

        private void SaveAddress_Click(object sender, RoutedEventArgs e)
        {
            var title = AddrTitle.Text?.Trim() ?? "";
            var city = AddrCity.Text?.Trim() ?? "";
            var dist = AddrDistrict.Text?.Trim() ?? "";
            var line = AddrLine.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(line))
            {
                MessageBox.Show("Adres başlığı ve adres alanı boş olamaz.");
                return;
            }

            if (_editingAddress == null)
            {
                // ADD
                var a = new Address
                {
                    PersonId = _person.Id,
                    Title = title,
                    City = city,
                    District = dist,
                    AddressLine = line
                };
                _addressRepo.Add(a);
            }
            else
            {
                // UPDATE
                _editingAddress.Title = title;
                _editingAddress.City = city;
                _editingAddress.District = dist;
                _editingAddress.AddressLine = line;

                _addressRepo.Update(_editingAddress);
            }

            RefreshAddresses();
            _editingAddress = null;
            ClearAddressForm();
        }

        private void ClearAddressForm()
        {
            AddrTitle.Clear();
            AddrCity.Clear();
            AddrDistrict.Clear();
            AddrLine.Clear();
        }

        private void AddressesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // İstersen seçince form dolsun diye burada da yapabiliriz.
            // Şimdilik satırdaki "Düzenle" butonu dolduruyor.
        }
    }
}
