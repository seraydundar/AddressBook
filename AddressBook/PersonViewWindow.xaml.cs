using System.Linq;
using System.Windows;
using AddressBook.Data;
using AddressBook.Models;

namespace AddressBook
{
    public partial class PersonViewWindow : Window
    {
        private readonly PersonRepository _personRepo = new();
        private readonly AddressRepository _addressRepo = new();

        public Person Person { get; private set; }
        public System.Collections.Generic.List<Address> Addresses { get; private set; }

        public string FullName => Person.FullName;
        public string? Phone => Person.Phone;
        public string AddressHeader => $"Adresler ({Addresses.Count})";

        public PersonViewWindow(int personId)
        {
            InitializeComponent();

            // Repo'da GetById yok, o yüzden GetAll içinden buluyoruz
            Person = _personRepo.GetAll().First(x => x.Id == personId);
            Addresses = _addressRepo.GetByPersonId(personId);

            DataContext = this;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
